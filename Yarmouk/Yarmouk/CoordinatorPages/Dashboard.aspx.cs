using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public partial class Dashboard : CoordinatorPage
    {
        protected override Task<object> LoadDataAsync(AuthSession session)
        { return Service.GetDashboardAsync(session); }
    }

    public class CoordinatorService
    {
        private readonly CoordinatorDatabase firebase;
        private readonly CoordinatorAccounts authentication;
        public CoordinatorService() : this(new CoordinatorDatabase(), new CoordinatorAccounts()) { }
        public CoordinatorService(CoordinatorDatabase firebase, CoordinatorAccounts authentication)
        { this.firebase = firebase; this.authentication = authentication; }

        private static void RequireCoordinator(AuthSession session)
        {
            if (session == null || session.ExpiresUtc <= DateTime.UtcNow || session.User == null
                || !session.User.IsActive || AuthPolicy.NormalizeRole(session.User.Role) != "coordinator")
                throw new UnauthorizedAccessException("Sign in with an active coordinator account.");
        }
        private Task<Dictionary<string, T>> ReadAsync<T>(string path, AuthSession session)
        {
            RequireCoordinator(session);
            return firebase.GetAsync<Dictionary<string, T>>(path, session.IdToken);
        }
        private static string Text(string value, string label, int maximum, bool required = true)
        {
            value = (value ?? "").Trim();
            if ((required && value.Length == 0) || value.Length > maximum)
                throw new ArgumentException(label + " is required and must be at most " + maximum + " characters.");
            return value;
        }
        private static void CheckId(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 128 || id.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
                throw new ArgumentException("Invalid record ID.");
        }
        public async Task<object> GetDirectoryAsync(string role, AuthSession session)
        {
            var users = await ReadAsync<CoordinatorUser>(DatabasePaths.Users, session) ?? new Dictionary<string, CoordinatorUser>();
            var students = await ReadAsync<Student>(DatabasePaths.Students, session) ?? new Dictionary<string, Student>();
            var majors = await ReadAsync<Major>(DatabasePaths.Majors, session) ?? new Dictionary<string, Major>();
            var companies = role == "company" ? await ReadAsync<Company>(DatabasePaths.Companies, session) ?? new Dictionary<string, Company>() : new Dictionary<string, Company>();
            var rows = new List<DirectoryEntry>();
            foreach (var pair in users.Where(p => p.Value != null && AuthPolicy.NormalizeRole(p.Value.Role) == role))
            {
                var user = pair.Value;
                Student student; students.TryGetValue(pair.Key, out student);
                Major major = null;
                if (student != null && !string.IsNullOrEmpty(student.MajorId)) majors.TryGetValue(student.MajorId, out major);
                var company = companies.Values.FirstOrDefault(c => c != null && c.SupervisorUid == pair.Key);
                var row = new DirectoryEntry {
                    Id = pair.Key, Name = role == "company" ? (company == null ? user.FullName : company.Name) : user.FullName,
                    Email = user.Email, IsActive = user.IsActive,
                    Department = major == null ? user.Department : major.Name,
                    College = major == null ? user.College : major.Faculty,
                    Gpa = student == null ? null : student.Gpa,
                    Supervisor = student == null ? "" : student.AcademicSupervisorId ?? "",
                    AssignedStudents = students.Values.Count(st => st != null && st.AcademicSupervisorId == pair.Key),
                    Status = role == "company" ? (company != null && company.IsApproved ? "Approved" : "Pending review") : (user.IsActive ? "Active" : "Inactive"),
                    Industry = company == null ? "" : company.Industry,
                    Location = company == null ? "" : company.Location,
                    CompanyEmail = company == null ? "" : company.CompanyEmail,
                    ContactName = role == "company" ? user.FullName : ""
                };
                if (role == "student" && student != null)
                {
                    student.StudentNumber = string.IsNullOrWhiteSpace(student.StudentNumber) ? student.StudentId : student.StudentNumber;
                    student.CompletedCredits = student.CompletedCredits > 0 ? student.CompletedCredits : student.CompletedCreditHours;
                    JsonConvert.PopulateObject(JsonConvert.SerializeObject(student, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }), row);
                }
                rows.Add(row);
            }
            return new {
                records = rows.OrderBy(r => r.Name).ToList(),
                departments = majors.Values.Where(m => m != null).Select(m => new { value = m.Name, label = m.Name, faculty = m.Faculty }).Distinct().ToList(),
                supervisors = users.Where(p => p.Value != null && AuthPolicy.NormalizeRole(p.Value.Role) == "academic")
                    .Select(p => new { value = p.Key, label = p.Value.FullName + (p.Value.IsActive ? "" : " (inactive)"), disabled = !p.Value.IsActive }).OrderBy(p => p.label).ToList()
            };
        }
        private async Task<CoordinatorUser> ManagedUserAsync(string uid, string role, AuthSession session)
        {
            RequireCoordinator(session); CheckId(uid);
            var user = await firebase.GetAsync<CoordinatorUser>(DatabasePaths.Users + "/" + uid, session.IdToken);
            if (user == null || AuthPolicy.NormalizeRole(user.Role) != role || uid == session.Uid)
                throw new ArgumentException("This account is unavailable in this roster.");
            return user;
        }
        public Task<CoordinatorUser> EnsureManagedStudentAsync(string uid, AuthSession session)
        { return ManagedUserAsync(uid, "student", session); }
        private async Task ValidateSupervisorAsync(string uid, AuthSession session)
        {
            if (string.IsNullOrEmpty(uid)) return;
            var user = await ManagedUserAsync(uid, "academic", session);
            if (!user.IsActive) throw new ArgumentException("Choose an active academic supervisor.");
        }
        public async Task<string> SaveDirectoryAsync(string role, DirectoryEntry entry, AuthSession session)
        {
            RequireCoordinator(session);
            if (role != "student" && role != "academic" && role != "company") throw new ArgumentException("Invalid account role.");
            if (entry == null) throw new ArgumentException("Account details are required.");
            bool creating = string.IsNullOrEmpty(entry.Id);
            entry.Name = Text(entry.Name, "Name", 120);
            entry.Email = Text(entry.Email, "Email", 254);
            if (!AuthPolicy.ValidEmail(entry.Email)) throw new ArgumentException("Enter a valid email address.");
            CoordinatorUser existing = creating ? null : await ManagedUserAsync(entry.Id, role, session);
            if (!creating && !string.Equals(entry.Email, existing.Email, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The sign-in email cannot be changed here. It must also be updated in Firebase Authentication.");
            var changes = new Dictionary<string, object>();
            var studentDetails = role == "student" && entry.ProfileFields != null ? JObject.FromObject(entry.ProfileFields).ToObject<Student>() : null;
            string majorId = null;
            if (role != "company")
            {
                entry.Department = Text(entry.Department, "Department", 120);
                entry.College = Text(entry.College, "College", 120);
            }
            if (role == "student")
            {
                decimal gpa;
                if (!decimal.TryParse(entry.Gpa, NumberStyles.Number, CultureInfo.InvariantCulture, out gpa) || gpa < 0 || gpa > 100)
                    throw new ArgumentException("GPA must be between 0 and 100.");
                entry.Gpa = gpa.ToString("0.00", CultureInfo.InvariantCulture);
                var majors = await ReadAsync<Major>(DatabasePaths.Majors, session) ?? new Dictionary<string, Major>();
                majorId = majors.Where(m => m.Value != null && m.Value.Name == entry.Department && m.Value.Faculty == entry.College).Select(m => m.Key).FirstOrDefault();
                if (majorId == null) throw new ArgumentException("Choose a department and college already listed in majors in Firebase.");
                await ValidateSupervisorAsync(entry.Supervisor, session);
            }
            if (role == "company")
            {
                entry.ContactName = Text(entry.ContactName, "Contact person", 120);
                entry.CompanyEmail = string.IsNullOrWhiteSpace(entry.CompanyEmail) ? "" : Text(entry.CompanyEmail, "Company email", 254);
                if (entry.CompanyEmail.Length > 0 && !AuthPolicy.ValidEmail(entry.CompanyEmail)) throw new ArgumentException("Enter a valid company email address.");
                entry.Industry = Text(entry.Industry, "Industry", 120);
                entry.Location = Text(entry.Location, "Location", 200);
                if (entry.Status != "Approved" && entry.Status != "Pending review") throw new ArgumentException("Choose an approval status.");
            }
            if (role == "academic" && entry.Status != "Active" && entry.Status != "Inactive") throw new ArgumentException("Choose an account status.");
            // Load related data before creating an Authentication account.
            var supervisorUser = role == "student" && !string.IsNullOrEmpty(entry.Supervisor)
                ? await firebase.GetAsync<CoordinatorUser>(DatabasePaths.Users + "/" + entry.Supervisor, session.IdToken) : null;
            var existingPlacements = role == "student" && !creating
                ? await ReadAsync<Placement>(DatabasePaths.Placements, session) ?? new Dictionary<string, Placement>() : new Dictionary<string, Placement>();
            var existingCompanies = role == "company"
                ? await ReadAsync<Company>(DatabasePaths.Companies, session) ?? new Dictionary<string, Company>() : new Dictionary<string, Company>();
            // Generate a one-time temporary password for the coordinator to pass to the new user.
            string temporaryPassword = creating ? NewOnboardingPassword() : null;
            AuthSession newAccount = creating ? await authentication.CreateUserAsync(entry.Email, temporaryPassword) : null;
            string uid = creating ? newAccount.Uid : entry.Id;
            string userPath = DatabasePaths.Users + "/" + uid + "/";
            changes[userPath + "fullName"] = role == "company" ? entry.ContactName : entry.Name;
            if (creating)
            {
                changes[userPath + "uid"] = uid;
                changes[userPath + "email"] = entry.Email;
                changes[userPath + "role"] = role;
                changes[userPath + "mustChangePassword"] = true;
                changes[userPath + "onboardingRequestedAtUnixMs"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            changes[userPath + "isActive"] = role == "academic" ? entry.Status == "Active" : creating || existing.IsActive;
            if (role != "company")
            {
                changes[userPath + "department"] = entry.Department;
                changes[userPath + "college"] = entry.College;
            }
            if (role == "student")
            {
                string path = DatabasePaths.Students + "/" + uid + "/";
                changes[path + "uid"] = uid;
                changes[path + "majorId"] = majorId;
                changes[path + "gpa"] = entry.Gpa;
                if (studentDetails != null)
                {
                    var fields = JObject.FromObject(studentDetails, JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
                    foreach (string field in new[] { "englishName", "studentNumber", "nationalId", "gender", "nationality", "birthPlace", "birthDate", "academicStatus", "completedCredits", "expectedGraduation" })
                        if (entry.ProfileFields.ContainsKey(field)) changes[path + field] = fields[field];
                    if (entry.ProfileFields.ContainsKey("studentNumber")) changes[path + "studentId"] = studentDetails.StudentNumber;
                    if (entry.ProfileFields.ContainsKey("completedCredits")) changes[path + "completedCreditHours"] = studentDetails.CompletedCredits;
                }
                changes[path + "academicSupervisorId"] = entry.Supervisor ?? "";
                changes[path + "academicSupervisorName"] = supervisorUser == null ? "" : supervisorUser.FullName;
                foreach (var placement in existingPlacements.Where(p => p.Value != null && p.Value.StudentUid == uid
                    && !string.Equals(p.Value.Status, "completed", StringComparison.OrdinalIgnoreCase)))
                    changes[DatabasePaths.Placements + "/" + placement.Key + "/academicSupervisorUid"] = entry.Supervisor ?? "";
            }
            try
            {
                if (role == "company")
                {
                    var pair = existingCompanies.FirstOrDefault(c => c.Value != null && c.Value.SupervisorUid == uid);
                    string companyId = pair.Key ?? uid;
                    string path = DatabasePaths.Companies + "/" + companyId + "/";
                    changes[path + "id"] = companyId; changes[path + "name"] = entry.Name;
                    changes[path + "contactEmail"] = entry.Email; changes[path + "supervisorUid"] = uid;
                    changes[path + "companyEmail"] = entry.CompanyEmail;
                    changes[path + "industry"] = entry.Industry; changes[path + "location"] = entry.Location;
                    changes[path + "isApproved"] = entry.Status == "Approved";
                }
                // One atomic database update preserves every field that is not being edited.
                await firebase.UpdateAsync("/", changes, session.IdToken);
            }
            catch (Exception error)
            {
                // A rejected write did not commit. Undo only the Authentication account we just made.
                // A network timeout is ambiguous: keep the account so a committed profile stays valid.
                if (newAccount != null && error is InvalidOperationException
                    && new[] { "Firebase HTTP error 400", "Firebase HTTP error 401", "Firebase HTTP error 403", "Firebase HTTP error 404" }.Contains(error.Message))
                {
                    try { await authentication.DeleteNewAccountAsync(newAccount); }
                    catch (Exception) { throw new InvalidOperationException("Registration failed. Check Authentication for UID " + uid + " before retrying."); }
                }
                else if (newAccount != null)
                    throw new InvalidOperationException("Registration could not be confirmed. Check Authentication and users/" + uid + " before retrying.");
                throw;
            }
            return creating ? "Account registered. Temporary password: " + temporaryPassword + ". Give it to the user; they must change it at first sign-in. A Firebase setup email will also be requested." : "Account details saved.";
        }
        private static string NewOnboardingPassword()
        {
            var bytes = new byte[32];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace('+', 'A').Replace('/', 'B').TrimEnd('=') + "aA1!";
        }
        public async Task<string> SetActiveAsync(string uid, string role, bool active, AuthSession session)
        {
            await ManagedUserAsync(uid, role, session);
            await firebase.UpdateAsync(DatabasePaths.Users + "/" + uid, new { isActive = active }, session.IdToken);
            return active ? "Account reactivated." : "Account deactivated. Existing records were kept.";
        }
        public async Task<string> AssignSupervisorAsync(string uid, string supervisorUid, AuthSession session)
        {
            await ManagedUserAsync(uid, "student", session);
            await ValidateSupervisorAsync(supervisorUid, session);
            var supervisor = string.IsNullOrEmpty(supervisorUid) ? null : await firebase.GetAsync<CoordinatorUser>(DatabasePaths.Users + "/" + supervisorUid, session.IdToken);
            var placements = await ReadAsync<Placement>(DatabasePaths.Placements, session) ?? new Dictionary<string, Placement>();
            var changes = new Dictionary<string, object> {
                [DatabasePaths.Students + "/" + uid + "/academicSupervisorId"] = supervisorUid ?? "",
                [DatabasePaths.Students + "/" + uid + "/academicSupervisorName"] = supervisor == null ? "" : supervisor.FullName
            };
            // Existing placement pages read this field, so keep it in sync with the student profile.
            foreach (var placement in placements.Where(p => p.Value != null && p.Value.StudentUid == uid
                && !string.Equals(p.Value.Status, "completed", StringComparison.OrdinalIgnoreCase)))
                changes[DatabasePaths.Placements + "/" + placement.Key + "/academicSupervisorUid"] = supervisorUid ?? "";
            await firebase.UpdateAsync("/", changes, session.IdToken);
            return "Supervisor assignment saved.";
        }
        public async Task<object> GetMessagesAsync(AuthSession session)
        {
            var messages = await ReadAsync<CoordinatorMessage>(DatabasePaths.ContactMessages, session) ?? new Dictionary<string, CoordinatorMessage>();
            foreach (var pair in messages) if (pair.Value != null) pair.Value.Id = pair.Key;
            return messages.Values.Where(m => m != null).OrderByDescending(m => m.CreatedAtUnixMs).ToList();
        }
        public async Task<string> UpdateMessageAsync(string action, string id, bool value, AuthSession session)
        {
            RequireCoordinator(session); CheckId(id);
            string path = DatabasePaths.ContactMessages + "/" + id;
            if (await firebase.GetAsync<CoordinatorMessage>(path, session.IdToken) == null) throw new ArgumentException("Message no longer exists.");
            if (action == "delete") await firebase.DeleteAsync(path, session.IdToken);
            else if (action == "read") await firebase.UpdateAsync(path, new { isRead = value }, session.IdToken);
            else if (action == "archive") await firebase.UpdateAsync(path, new { isArchived = value }, session.IdToken);
            else throw new ArgumentException("Unknown message action.");
            return action == "delete" ? "Message deleted." : "Message updated.";
        }
        public async Task<object> GetDashboardAsync(AuthSession session)
        {
            var users = await ReadAsync<CoordinatorUser>(DatabasePaths.Users, session) ?? new Dictionary<string, CoordinatorUser>();
            var students = await ReadAsync<Student>(DatabasePaths.Students, session) ?? new Dictionary<string, Student>();
            var companies = await ReadAsync<Company>(DatabasePaths.Companies, session) ?? new Dictionary<string, Company>();
            var opportunities = await ReadAsync<Opportunity>(DatabasePaths.Opportunities, session) ?? new Dictionary<string, Opportunity>();
            var placements = await ReadAsync<Placement>(DatabasePaths.Placements, session) ?? new Dictionary<string, Placement>();
            // Keep missing flags distinguishable from explicit false values in older records.
            var logs = await ReadAsync<JObject>(DatabasePaths.TrainingLogs, session) ?? new Dictionary<string, JObject>();
            var evaluations = await ReadAsync<JObject>(DatabasePaths.Evaluations, session) ?? new Dictionary<string, JObject>();
            var active = users.Where(p => p.Value != null && p.Value.IsActive).ToList();
            return new {
                totalUsers = active.Count,
                students = active.Count(p => AuthPolicy.NormalizeRole(p.Value.Role) == "student"),
                companies = companies.Count(p => p.Value != null),
                supervisors = active.Count(p => AuthPolicy.NormalizeRole(p.Value.Role) == "academic"),
                opportunities = opportunities.Values.Count(o => o != null && o.IsActive),
                activeInternships = placements.Values.Count(p => p != null && new[] { "active", "inprogress", "ongoing" }.Contains(Normalize(p.Status))),
                pendingHours = logs.Values.Count(l => l != null && !IsApprovedLog(l)),
                pendingAssignments = active.Count(u => AuthPolicy.NormalizeRole(u.Value.Role) == "student" && (!students.ContainsKey(u.Key) || students[u.Key] == null || string.IsNullOrEmpty(students[u.Key].AcademicSupervisorId) || !active.Any(a => a.Key == students[u.Key].AcademicSupervisorId && AuthPolicy.NormalizeRole(a.Value.Role) == "academic"))),
                pendingCompletion = evaluations.Values.Count(e => e != null && ReadBool(e, "academicSubmitted")
                    && IsCompanyEvaluationSubmitted(e) && !ReadBool(e, "coordinatorApproved"))
            };
        }

        private static string Normalize(string value)
        { return string.IsNullOrWhiteSpace(value) ? "" : value.Trim().Replace(" ", "").Replace("_", "").ToLowerInvariant(); }
        private static bool ReadBool(JObject record, string key)
        {
            bool value;
            return record != null && record[key] != null && (record[key].Type == JTokenType.Boolean
                ? record[key].Value<bool>() : bool.TryParse(record[key].ToString(), out value) && value);
        }
        private static bool IsApprovedLog(JObject record)
        { return record["isApproved"] != null && record["isApproved"].Type != JTokenType.Null
            ? ReadBool(record, "isApproved") : Normalize((string)record["status"]) == "approved"; }
        private static bool IsCompanyEvaluationSubmitted(JObject record)
        {
            if (record == null) return false;
            if (record["companySubmitted"] != null && record["companySubmitted"].Type != JTokenType.Null)
                return ReadBool(record, "companySubmitted");
            double score;
            return record["companyScore"] != null && record["companyScore"].Type != JTokenType.Null
                && double.TryParse(record["companyScore"].ToString(), out score);
        }
    }

    public class CoordinatorUser : User
    {
        public string Department { get; set; }
        public string College { get; set; }
    }
    public class CoordinatorMessage : ContactMessage
    {
        public bool IsRead { get; set; }
        public bool IsArchived { get; set; }
    }
    public class DirectoryEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string College { get; set; }
        public string Gpa { get; set; }
        public string Supervisor { get; set; }
        public int AssignedStudents { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string Industry { get; set; }
        public string Location { get; set; }
        public string ContactName { get; set; }
        public string CompanyEmail { get; set; }
    }
}
