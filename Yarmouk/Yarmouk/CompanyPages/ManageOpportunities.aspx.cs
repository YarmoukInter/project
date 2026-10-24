using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using System.Web.UI.WebControls;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CompanyPages
{
    public partial class ManageOpportunities : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(LoadPageAsync));
        }

        protected void ApplyFiltersButton_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(LoadPageAsync));
        }

        protected void SaveOpportunityButton_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(SaveOpportunityAsync));
        }

        protected void OpportunitiesRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var id = Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture);
            if (e.CommandName == "ToggleStatus")
                RegisterAsyncTask(new PageAsyncTask(() => ToggleStatusAsync(id)));
        }

        private async Task LoadPageAsync()
        {
            try
            {
                var auth = await VerifyCompanyAsync();
                var items = await new CompanyWorkflowService().GetOpportunitiesAsync(auth.CompanyId, auth.Session.IdToken);
                BindPage(items);
                PageFeedbackLabel.Text = "Opportunities loaded from Firebase.";
            }
            catch (UnauthorizedAccessException) { RedirectToLogin(); }
            catch (AuthException) { RedirectToLogin(); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company opportunities could not load: " + error.GetType().Name);
                BindPage(new List<CompanyOpportunityItem>());
                PageFeedbackLabel.Text = "Could not load opportunities. Please try again.";
            }
        }

        private void BindPage(List<CompanyOpportunityItem> allItems)
        {
            allItems = allItems ?? new List<CompanyOpportunityItem>();
            ActiveCountLiteral.Text = allItems.Count(x => Same(x.Status, "active")).ToString(CultureInfo.InvariantCulture);
            DraftCountLiteral.Text = allItems.Count(x => Same(x.Status, "draft")).ToString(CultureInfo.InvariantCulture);
            ApplicationCountLiteral.Text = allItems.Sum(x => x.Applications).ToString(CultureInfo.InvariantCulture);

            string query = (OpportunitySearchTextBox.Text ?? string.Empty).Trim();
            string status = OpportunityStatusFilter.SelectedValue ?? "all";
            var filtered = allItems.Where(item =>
                (string.IsNullOrEmpty(query) || (item.Title ?? string.Empty).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item.Majors ?? new List<string>()).Any(major => major.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                && (status == "all" || Same(item.Status, status))).ToList();

            OpportunitiesRepeater.DataSource = filtered;
            OpportunitiesRepeater.DataBind();
            OpportunityCountLabel.Text = filtered.Count == 1 ? "1 opportunity" : filtered.Count + " opportunities";
            EmptyStatePanel.Visible = filtered.Count == 0;
        }

        private async Task SaveOpportunityAsync()
        {
            OpportunityEditorPanel.Visible = true;
            OpportunityEditorPanel.Attributes.Remove("hidden");
            EditorFeedbackLabel.Visible = false;
            try
            {
                int seats;
                int hours;
                if (!int.TryParse(OpportunitySeatsInput.Text, out seats) || seats < 1 || seats > 500)
                    throw new ArgumentException("Available seats must be between 1 and 500.");
                if (!int.TryParse(OpportunityHoursInput.Text, out hours) || hours < 0 || hours > 10000)
                    throw new ArgumentException("Training hours must be between 0 and 10000.");

                var majors = SelectedMajors();
                if (majors.Count == 0) throw new ArgumentException("Select at least one eligible major.");
                var input = new CompanyOpportunityInput
                {
                    Id = NullIfEmpty(EditingOpportunityId.Value),
                    Status = string.IsNullOrWhiteSpace(EditingOpportunityStatus.Value) ? "active" : EditingOpportunityStatus.Value,
                    Title = OpportunityTitleInput.Text,
                    Deadline = OpportunityDeadlineInput.Text,
                    Seats = seats,
                    Duration = OpportunityDurationInput.Text,
                    Hours = hours,
                    WorkMode = OpportunityWorkModeInput.SelectedValue,
                    Majors = majors,
                    Description = OpportunityDescriptionInput.Text,
                    Requirements = OpportunityRequirementsInput.Text,
                    Questions = SplitLines(OpportunityQuestionsInput.Text)
                };
                var auth = await VerifyCompanyAsync();
                await new CompanyWorkflowService().SaveOpportunityAsync(auth.CompanyId, auth.Session.IdToken, input);
                OpportunityEditorPanel.Attributes["hidden"] = "hidden";
                await LoadPageAsync();
                PageFeedbackLabel.Text = "Opportunity saved to Firebase.";
            }
            catch (UnauthorizedAccessException) { RedirectToLogin(); }
            catch (AuthException) { RedirectToLogin(); }
            catch (ArgumentException error) { ShowEditorError(error.Message); }
            catch (InvalidOperationException error) { ShowEditorError(error.Message); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company opportunity save failed: " + error.GetType().Name);
                ShowEditorError("Firebase could not save this opportunity. Please try again.");
            }
        }

        private async Task ToggleStatusAsync(string id)
        {
            try
            {
                var auth = await VerifyCompanyAsync();
                var workflow = new CompanyWorkflowService();
                var items = await workflow.GetOpportunitiesAsync(auth.CompanyId, auth.Session.IdToken);
                var item = items.FirstOrDefault(x => Same(x.Id, id));
                if (item == null) throw new ArgumentException("Opportunity not found.");
                string nextStatus = Same(item.Status, "active") ? "closed" : "active";
                await workflow.SetOpportunityStatusAsync(auth.CompanyId, id, nextStatus, auth.Session.IdToken);
                await LoadPageAsync();
                PageFeedbackLabel.Text = nextStatus == "closed" ? "Opportunity closed." : "Opportunity published.";
            }
            catch (UnauthorizedAccessException) { RedirectToLogin(); }
            catch (AuthException) { RedirectToLogin(); }
            catch (Exception error) { PageFeedbackLabel.Text = "Could not update the opportunity: " + HttpUtility.HtmlEncode(error.Message); }
        }

        private List<string> SelectedMajors()
        {
            var result = new List<string>();
            if (MajorCisCheckBox.Checked) result.Add("CIS");
            if (MajorCsCheckBox.Checked) result.Add("CS");
            if (MajorSeCheckBox.Checked) result.Add("SE");
            return result;
        }

        private void ShowEditorError(string message) { OpportunityEditorPanel.Visible = true; OpportunityEditorPanel.Attributes.Remove("hidden"); EditorFeedbackLabel.Text = HttpUtility.HtmlEncode(message); EditorFeedbackLabel.Visible = true; }
        private void RedirectToLogin() { Response.Redirect(ResolveUrl("~/PublicPages/Login"), false); Context.ApplicationInstance.CompleteRequest(); }
        private static string NullIfEmpty(string value) { return string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }
        private static bool Same(string left, string right) { return string.Equals(left, right, StringComparison.OrdinalIgnoreCase); }
        private static List<string> SplitLines(string value) { return (value ?? string.Empty).Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0).ToList(); }

        protected string Encode(object value) { return HttpUtility.HtmlEncode(Convert.ToString(value, CultureInfo.CurrentCulture)); }
        protected string Attr(object value) { return HttpUtility.HtmlAttributeEncode(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty); }
        protected string JoinAttribute(object value, string separator) { var values = value as IEnumerable<string>; return Attr(values == null ? string.Empty : string.Join(separator, values)); }
        protected string FormatDateInput(object value) { DateTime date; var text = Convert.ToString(value, CultureInfo.InvariantCulture); return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty; }
        protected string RenderMajors(object value) { return string.Join(string.Empty, ((value as IEnumerable<string>) ?? Enumerable.Empty<string>()).Select(x => "<span class=\"co-major-chip\">" + HttpUtility.HtmlEncode(x) + "</span>")); }
        protected string FormatDeadline(object value) { DateTime date; string text = Convert.ToString(value, CultureInfo.InvariantCulture); return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ? date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : "Not set"; }
        protected string StatusCss(object value) { return "co-status co-status-" + HttpUtility.HtmlAttributeEncode(Convert.ToString(value, CultureInfo.InvariantCulture).ToLowerInvariant()); }
        protected string StatusLabel(object value) { string status = Convert.ToString(value, CultureInfo.InvariantCulture); return string.IsNullOrEmpty(status) ? "Unknown" : HttpUtility.HtmlEncode(char.ToUpperInvariant(status[0]) + status.Substring(1)); }
        protected string ToggleLabel(object value) { return Same(Convert.ToString(value, CultureInfo.InvariantCulture), "active") ? "Close" : Same(Convert.ToString(value, CultureInfo.InvariantCulture), "closed") ? "Reopen" : "Publish"; }
        protected string ToggleCss(object value) { return "co-text-action" + (Same(Convert.ToString(value, CultureInfo.InvariantCulture), "active") ? " close" : string.Empty); }

        private async Task<VerifiedCompany> VerifyCompanyAsync()
        {
            var session = Session == null ? null : Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null || string.IsNullOrWhiteSpace(session.Uid) || string.IsNullOrWhiteSpace(session.IdToken))
                throw new UnauthorizedAccessException();
            // AuthGuard validates this protected page's session and current role.
            // Rechecking ExpiresUtc here rejected actions after 30 minutes while
            // the rest of the authenticated company workspace still worked.
            var user = await new StudentService().GetUserAsync(session.Uid, session.IdToken);
            if (user == null || !user.IsActive || !string.Equals(AuthPolicy.NormalizeRole(user.Role), "company", StringComparison.Ordinal))
                throw new UnauthorizedAccessException();
            session.User = user;

            var firebase = new FirebaseHelper();
            var userRecord = await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + session.Uid, session.IdToken);
            string companyId = userRecord == null ? null : (string)userRecord["companyId"];
            if (string.IsNullOrWhiteSpace(companyId))
            {
                var companies = await firebase.GetAsync<System.Collections.Generic.Dictionary<string, JObject>>(DatabasePaths.Companies, session.IdToken);
                if (companies != null)
                {
                    var match = companies.FirstOrDefault(entry => entry.Value != null
                        && ((entry.Value["supervisorUids"] is JObject supervisors
                                && supervisors[session.Uid] != null && supervisors[session.Uid].Value<bool>())
                            || string.Equals((string)entry.Value["supervisorUid"], session.Uid, StringComparison.Ordinal)));
                    if (!string.IsNullOrWhiteSpace(match.Key)) companyId = match.Key;
                }
            }
            if (string.IsNullOrWhiteSpace(companyId)) throw new UnauthorizedAccessException();
            var company = await firebase.GetAsync<Company>(DatabasePaths.Companies + "/" + companyId, session.IdToken);
            if (company == null || !company.IsApproved) throw new UnauthorizedAccessException();
            return new VerifiedCompany { Session = session, CompanyId = companyId };
        }

        private sealed class VerifiedCompany
        {
            public AuthSession Session { get; set; }
            public string CompanyId { get; set; }
        }
    }
}

namespace Yarmouk.CompanyPages
{
    // Kept in the page code-behind because this project does not include a workflow service.
    public sealed class CompanyOpportunityInput
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.Collections.Generic.List<string> Majors { get; set; }
        public int Seats { get; set; }
        public string Deadline { get; set; }
        public string Status { get; set; }
        public string Duration { get; set; }
        public int Hours { get; set; }
        public string WorkMode { get; set; }
        public string Requirements { get; set; }
        public System.Collections.Generic.List<string> Questions { get; set; }
    }

    public sealed class CompanyOpportunityItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public System.Collections.Generic.List<string> Majors { get; set; }
        public int Seats { get; set; }
        public int TotalSeats { get; set; }
        public string Deadline { get; set; }
        public string Status { get; set; }
        public int Applications { get; set; }
        public string Duration { get; set; }
        public int Hours { get; set; }
        public string WorkMode { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public System.Collections.Generic.List<string> Questions { get; set; }
    }

    public sealed class CompanyApplicationItem
    {
        public string Id { get; set; }
        public string StudentUid { get; set; }
        public string Name { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Number { get; set; }
        public string Major { get; set; }
        public string Email { get; set; }
        public long AppliedAtUnixMs { get; set; }
        public string OpportunityId { get; set; }
        public string Opportunity { get; set; }
        public string Status { get; set; }
        public string CvUrl { get; set; }
        public bool HasUploadedCv { get; set; }
        public System.Collections.Generic.List<string> Answers { get; set; }
        public System.Collections.Generic.List<string> Questions { get; set; }
        public string RejectionReason { get; set; }
        public string DecisionNote { get; set; }
        public bool CanAccept { get; set; }
        public string AcceptanceBlockedReason { get; set; }
    }

    public sealed class CompanyWorkflowService
    {
        private readonly Yarmouk.Helpers.FirebaseHelper firebase = new Yarmouk.Helpers.FirebaseHelper();

        public async System.Threading.Tasks.Task<System.Collections.Generic.List<CompanyOpportunityItem>> GetOpportunitiesAsync(string companyId, string idToken)
        {
            var opportunities = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Opportunities, idToken);
            var applications = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Applications, idToken);
            var result = new System.Collections.Generic.List<CompanyOpportunityItem>();
            foreach (var entry in opportunities)
            {
                if (!Same(Text(entry.Value, "companyId"), companyId)) continue;
                result.Add(ToOpportunity(entry.Key, entry.Value, applications));
            }
            result.Sort((a, b) => string.Compare(a.Title, b.Title, System.StringComparison.CurrentCultureIgnoreCase));
            return result;
        }

        public async System.Threading.Tasks.Task<CompanyOpportunityItem> SaveOpportunityAsync(string companyId, string idToken, CompanyOpportunityInput input)
        {
            Validate(input);
            var company = await firebase.GetAsync<Yarmouk.Models.Company>(Yarmouk.Helpers.DatabasePaths.Companies + "/" + companyId, idToken);
            if (company == null || !company.IsApproved) throw new System.UnauthorizedAccessException();
            JObject existing = null;
            string id = input.Id;
            if (!string.IsNullOrWhiteSpace(id))
            {
                ValidateKey(id);
                existing = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + id, idToken);
                if (existing == null || !Same(Text(existing, "companyId"), companyId)) throw new System.UnauthorizedAccessException();
            }
            else id = "OPP" + System.Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();

            string status = Normalize(input.Status);
            if (status == "published") status = "active";
            if (status != "active" && status != "draft" && !(status == "closed" && !string.IsNullOrWhiteSpace(input.Id)))
                throw new System.ArgumentException("Choose a valid opportunity status.");
            string workMode = Normalize(input.WorkMode);
            if (workMode != "onsite" && workMode != "hybrid" && workMode != "remote") throw new System.ArgumentException("Choose a valid work mode.");

            var row = existing == null ? new JObject() : (JObject)existing.DeepClone();
            row["id"] = id;
            row["companyId"] = companyId;
            row["title"] = input.Title.Trim();
            row["description"] = input.Description.Trim();
            row["eligibleMajorIds"] = JArray.FromObject(input.Majors ?? new System.Collections.Generic.List<string>());
            row["availablePositions"] = input.Seats;
            row["isActive"] = status == "active";
            row["status"] = status;
            row["deadline"] = input.Deadline;
            row["location"] = string.IsNullOrWhiteSpace(company.Location) ? Text(existing, "location") : company.Location;
            row["applicationQuestions"] = JArray.FromObject(input.Questions ?? new System.Collections.Generic.List<string>());
            row["requirements"] = input.Requirements ?? string.Empty;
            row["duration"] = input.Duration ?? string.Empty;
            row["requiredTrainingHours"] = input.Hours;
            row["workMode"] = workMode == "onsite" ? "OnSite" : workMode == "hybrid" ? "Hybrid" : "Remote";
            var apps = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Applications, idToken);
            if (existing == null)
                await firebase.SaveAsync(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + id, row, idToken);
            else
                await firebase.TransactAsync(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + id, idToken, current => {
                    if (current == null || !Same(Text(current, "companyId"), companyId)) throw new System.UnauthorizedAccessException();
                    if (input.Seats < OccupiedSeats(id, current, apps))
                        throw new System.ArgumentException("Total seats cannot be fewer than the seats already reserved.");
                    foreach (string field in new[] { "title", "description", "eligibleMajorIds", "availablePositions", "isActive", "status", "deadline", "location", "applicationQuestions", "requirements", "duration", "requiredTrainingHours", "workMode" })
                        current[field] = row[field].DeepClone();
                    return true;
                });
            return ToOpportunity(id, row, apps);
        }

        public async System.Threading.Tasks.Task SetOpportunityStatusAsync(string companyId, string id, string status, string idToken)
        {
            ValidateKey(id);
            var row = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + id, idToken);
            if (row == null || !Same(Text(row, "companyId"), companyId)) throw new System.UnauthorizedAccessException();
            status = Normalize(status);
            if (status != "active" && status != "closed" && status != "draft") throw new System.ArgumentException("Choose a valid opportunity status.");
            await firebase.UpdateAsync(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + id, new { status = status, isActive = status == "active" }, idToken);
        }

        public async System.Threading.Tasks.Task<System.Collections.Generic.List<CompanyApplicationItem>> GetApplicationsAsync(string companyId, string idToken)
        {
            var opportunities = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Opportunities, idToken);
            var applications = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Applications, idToken);
            var users = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Users, idToken);
            var students = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Students, idToken);
            var majors = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Majors, idToken);
            var placements = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Placements, idToken);
            var result = new System.Collections.Generic.List<CompanyApplicationItem>();
            foreach (var entry in applications)
            {
                var app = entry.Value;
                var opportunityId = Text(app, "opportunityId");
                JObject opportunity;
                if (!opportunities.TryGetValue(opportunityId ?? string.Empty, out opportunity) || !Same(Text(opportunity, "companyId"), companyId)) continue;
                string studentUid = Text(app, "studentUid") ?? string.Empty;
                JObject user; users.TryGetValue(studentUid, out user);
                JObject student; students.TryGetValue(studentUid, out student);
                JObject major; majors.TryGetValue(Text(student, "majorId") ?? string.Empty, out major);
                var answers = new System.Collections.Generic.List<string>();
                var questions = Strings(app["applicationQuestions"]);
                if (questions.Count == 0) questions = Strings(opportunity["applicationQuestions"]);
                var answerMap = app["answers"] as JObject;
                var answerList = app["answers"] as JArray;
                for (int i = 0; i < questions.Count; i++)
                    answers.Add(answerList != null && i < answerList.Count
                        ? answerList[i].ToString() : answerMap == null ? string.Empty : Text(answerMap, i.ToString()) ?? string.Empty);
                string status = Normalize(Text(app, "status"));
                status = status == "accepted" ? "accepted"
                    : status == "offered" || status == "offerreceived" ? "offered"
                    : status == "rejected" || status == "declined" ? "rejected"
                    : status == "withdrawn" || status == "cancelled" ? "withdrawn"
                    : status == "inreview" || status == "underreview" ? "in-review"
                    : string.IsNullOrWhiteSpace(status) || status == "pending" || status == "new" || status == "submitted" ? "new"
                    : "unknown";
                string acceptedApplicationId = Text(student, "acceptedApplicationId");
                bool confirmedHere = Same(acceptedApplicationId, Text(app, "id") ?? entry.Key)
                    || placements.Values.Any(placement => placement != null
                        && Same(Text(placement, "applicationId"), Text(app, "id") ?? entry.Key)
                        && IsConfirmedPlacementStatus(Text(placement, "status")));
                // Older records can still say Offered after the student has
                // confirmed the offer. The placement/student relationship is
                // authoritative, so never present a confirmed trainee as pending.
                if (confirmedHere) status = "accepted";
                bool confirmedElsewhere = !string.IsNullOrWhiteSpace(acceptedApplicationId)
                    && !Same(acceptedApplicationId, Text(app, "id") ?? entry.Key);
                confirmedElsewhere = confirmedElsewhere || placements.Values.Any(placement => placement != null
                    && Same(Text(placement, "studentUid"), studentUid)
                    && !Same(Text(placement, "opportunityId"), opportunityId)
                    && IsConfirmedPlacementStatus(Text(placement, "status")));
                result.Add(new CompanyApplicationItem {
                    Id = entry.Key,
                    StudentUid = studentUid,
                    Name = First(Text(user, "fullName"), "Student"),
                    ProfileImageUrl = StudentProfileImages.GetUrl(Text(student, "profileImageFileName")),
                    Number = First(Text(student, "studentNumber"), Text(student, "studentId"), Text(user, "studentId"), "—"),
                    Major = First(Text(major, "name"), Text(student, "majorId"), Text(user, "majorId"), "—"),
                    Email = First(Text(user, "email"), ""),
                    AppliedAtUnixMs = Number(app, "appliedAtUnixMs"),
                    OpportunityId = opportunityId,
                    Opportunity = First(Text(opportunity, "title"), "Internship opportunity"),
                    Status = status,
                    CvUrl = First(Text(user, "cvLink"), Text(student, "cvUrl"), ""),
                    Answers = answers,
                    Questions = questions,
                    RejectionReason = Text(app, "rejectionReason") ?? string.Empty,
                    DecisionNote = status == "offered" ? "Offer sent; waiting for student confirmation."
                        : status == "accepted" ? "The student accepted the offer and confirmed this placement."
                        : status == "withdrawn" ? "This application was closed after the student confirmed another placement."
                        : confirmedElsewhere ? "This student has already confirmed another internship placement." : string.Empty,
                    CanAccept = (status == "new" || status == "in-review") && !confirmedElsewhere,
                    AcceptanceBlockedReason = confirmedElsewhere ? "Already accepted in another opportunity" : string.Empty
                });
            }
            var cvChecks = result.Where(item => !string.IsNullOrWhiteSpace(item.StudentUid)).Select(async item =>
            {
                var cv = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.StudentCvFiles + "/" + item.StudentUid, idToken);
                item.HasUploadedCv = cv != null && !string.IsNullOrWhiteSpace(Text(cv, "dataBase64"));
            });
            await System.Threading.Tasks.Task.WhenAll(cvChecks);
            result.Sort((a, b) => b.AppliedAtUnixMs.CompareTo(a.AppliedAtUnixMs));
            return result;
        }

        public async System.Threading.Tasks.Task SetApplicationStatusAsync(string companyId, string reviewerUid, string id, string status, string reason, string idToken)
        {
            ValidateKey(id);
            reason = (reason ?? string.Empty).Trim();
            if (reason.Length > 500) throw new System.ArgumentException("The rejection reason must be 500 characters or fewer.");
            var app = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.Applications + "/" + id, idToken);
            if (app == null) throw new System.ArgumentException("Application not found.");
            var opportunity = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + Text(app, "opportunityId"), idToken);
            if (opportunity == null || !Same(Text(opportunity, "companyId"), companyId)) throw new System.UnauthorizedAccessException();
            string normalized = Normalize(status);
            string value = normalized == "inreview" ? "InReview" : normalized == "offered" || normalized == "accepted" ? "Offered" : normalized == "rejected" ? "Rejected" : null;
            if (value == null) throw new System.ArgumentException("Choose a valid application decision.");
            string current = Normalize(Text(app, "status"));
            if (current == "accepted" || current == "rejected" || current == "offered" || current == "offerreceived") throw new System.InvalidOperationException("This application already has a final decision.");
            if (value == "Rejected" && string.IsNullOrWhiteSpace(reason)) throw new System.ArgumentException("A rejection reason is required.");
            if (value == "Offered")
            {
                string studentUid = Text(app, "studentUid");
                if (string.IsNullOrWhiteSpace(studentUid))
                    throw new System.InvalidOperationException("This application is not linked to a student account.");
                var student = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.Students + "/" + studentUid, idToken);
                string acceptedApplicationId = Text(student, "acceptedApplicationId");
                if (!string.IsNullOrWhiteSpace(acceptedApplicationId) && !Same(acceptedApplicationId, id))
                    throw new System.InvalidOperationException("This student has already confirmed another internship placement.");
                var placements = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Placements, idToken);
                if (placements.Values.Any(placement => placement != null && Same(Text(placement, "studentUid"), studentUid)
                    && !Same(Text(placement, "opportunityId"), Text(app, "opportunityId"))
                    && IsConfirmedPlacementStatus(Text(placement, "status"))))
                    throw new System.InvalidOperationException("This student has already confirmed another internship placement.");
            }
            await firebase.UpdateAsync(Yarmouk.Helpers.DatabasePaths.Applications + "/" + id, new { companyId = companyId, status = value, rejectionReason = value == "Rejected" ? reason : string.Empty, reviewedByUid = reviewerUid, reviewedAtUnixMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, idToken);
        }

        private async System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, JObject>> GetMapAsync(string path, string token)
        {
            return await firebase.GetAsync<System.Collections.Generic.Dictionary<string, JObject>>(path, token) ?? new System.Collections.Generic.Dictionary<string, JObject>();
        }
        private static CompanyOpportunityItem ToOpportunity(string id, JObject row, System.Collections.Generic.Dictionary<string, JObject> apps)
        {
            string status = Text(row, "status");
            if (string.IsNullOrWhiteSpace(status)) status = Bool(row, "isActive") ? "active" : "closed";
            int count = 0;
            foreach (var app in apps.Values) if (Same(Text(app, "opportunityId"), id)) count++;
            return new CompanyOpportunityItem { Id = id, Title = Text(row, "title") ?? string.Empty, Majors = Strings(row["eligibleMajorIds"]), Seats = Math.Max(0, Integer(row, "availablePositions") - OccupiedSeats(id, row, apps)), TotalSeats = Integer(row, "availablePositions"), Deadline = Text(row, "deadline") ?? string.Empty, Status = Normalize(status), Applications = count, Duration = Text(row, "duration") ?? string.Empty, Hours = Integer(row, "requiredTrainingHours"), WorkMode = Text(row, "workMode") ?? "OnSite", Description = Text(row, "description") ?? string.Empty, Requirements = Text(row, "requirements") ?? string.Empty, Questions = Strings(row["applicationQuestions"]) };
        }
        private static int OccupiedSeats(string id, JObject row, System.Collections.Generic.Dictionary<string, JObject> apps)
        {
            var occupied = new System.Collections.Generic.HashSet<string>(apps.Values
                .Where(a => Same(Text(a, "opportunityId"), id) && Normalize(Text(a, "status")) == "accepted")
                .Select(a => Text(a, "studentUid")).Where(uid => !string.IsNullOrWhiteSpace(uid)), System.StringComparer.Ordinal);
            var reservations = row["seatReservations"] as JObject;
            if (reservations != null) occupied.UnionWith(reservations.Properties().Select(p => p.Name));
            return System.Math.Max(Integer(row, "filledPositions"), occupied.Count);
        }

        private static void Validate(CompanyOpportunityInput input)
        {
            if (input == null) throw new System.ArgumentException("Opportunity details are required.");
            if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Trim().Length > 120) throw new System.ArgumentException("Enter an internship title up to 120 characters.");
            if (string.IsNullOrWhiteSpace(input.Description) || input.Description.Trim().Length > 1500) throw new System.ArgumentException("Enter a description up to 1500 characters.");
            if (input.Seats < 1 || input.Seats > 500) throw new System.ArgumentException("Available seats must be between 1 and 500.");
            if (input.Hours < 0 || input.Hours > 10000) throw new System.ArgumentException("Training hours must be between 0 and 10000.");
            System.DateTime deadline;
            if (!string.IsNullOrWhiteSpace(input.Deadline) && !System.DateTime.TryParseExact(input.Deadline, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out deadline)) throw new System.ArgumentException("Choose a valid application deadline.");
            if (input.Majors == null || input.Majors.Count == 0) throw new System.ArgumentException("Select at least one eligible major.");
            if (input.Questions != null && input.Questions.Count > 20) throw new System.ArgumentException("Use no more than 20 application questions.");
        }
        private static string Text(JObject row, string key) { return row == null || row[key] == null ? null : row[key].Type == JTokenType.String ? (string)row[key] : row[key].ToString(); }
        private static int Integer(JObject row, string key) { int value; return int.TryParse(Text(row, key), out value) ? value : 0; }
        private static long Number(JObject row, string key) { long value; return long.TryParse(Text(row, key), out value) ? value : 0L; }
        private static bool Bool(JObject row, string key) { return row != null && row[key] != null && row[key].Value<bool>(); }
        private static System.Collections.Generic.List<string> Strings(JToken token) { return token == null || token.Type == JTokenType.Null ? new System.Collections.Generic.List<string>() : token.ToObject<System.Collections.Generic.List<string>>() ?? new System.Collections.Generic.List<string>(); }
        private static string First(params string[] values) { return System.Linq.Enumerable.FirstOrDefault(values, value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty; }
        private static string Normalize(string value) { return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).ToLowerInvariant(); }
        private static bool IsConfirmedPlacementStatus(string value) { value = Normalize(value); return value == "confirmed" || value == "accepted" || value == "active" || value == "inprogress" || value == "ongoing" || value == "completed"; }
        private static bool Same(string left, string right) { return !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right) && string.Equals(left, right, System.StringComparison.Ordinal); }
        private static void ValidateKey(string value) { if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?', '&' }) >= 0) throw new System.ArgumentException("Invalid Firebase record id."); }
    }
}
