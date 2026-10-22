using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.StudentPages
{
    public partial class Opportunities : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var auth = Session == null ? null : Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null || string.IsNullOrWhiteSpace(auth.Uid) || string.IsNullOrWhiteSpace(auth.IdToken))
                {
                    GoToLogin();
                    return;
                }

                try
                {
                    var student = await new StudentService().GetStudentAsync(auth.Uid, auth.IdToken);
                    if (student == null)
                    {
                        OpportunityData.Text = "[]";
                        SetFeedback("Your student profile could not be loaded.");
                        return;
                    }

                    var firebase = new FirebaseHelper();
                    var opportunityRows = await firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Opportunities, auth.IdToken)
                        ?? new Dictionary<string, JObject>();
                    var companyRows = await firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Companies, auth.IdToken)
                        ?? new Dictionary<string, JObject>();

                    var items = opportunityRows
                        .Where(entry => entry.Value != null && IsAvailable(entry.Value))
                        .Select(entry =>
                        {
                            var row = entry.Value;
                            string companyId = ReadText(row, "companyId");
                            JObject company;
                            companyRows.TryGetValue(companyId ?? string.Empty, out company);
                            return new
                            {
                                id = First(ReadText(row, "id"), entry.Key),
                                title = ReadText(row, "title") ?? "Internship opportunity",
                                description = ReadText(row, "description") ?? string.Empty,
                                requirements = ReadText(row, "requirements") ?? string.Empty,
                                duration = ReadText(row, "duration") ?? string.Empty,
                                requiredTrainingHours = ReadInt(row, "requiredTrainingHours"),
                                workMode = ReadText(row, "workMode") ?? string.Empty,
                                skills = ReadStrings(row, "skills", "requiredSkills"),
                                deadline = ReadText(row, "deadline") ?? string.Empty,
                                availablePositions = ReadInt(row, "availablePositions", "seats"),
                                majorIds = ReadStrings(row, "eligibleMajorIds", "majorIds", "majors", "majorId"),
                                studentMajorId = student.MajorId ?? string.Empty,
                                questionsCount = ReadStrings(row, "applicationQuestions", "questions").Count,
                                companyName = ReadText(company, "name") ?? "Company",
                                companyIndustry = ReadText(company, "industry") ?? string.Empty,
                                location = First(ReadText(row, "location"), ReadText(company, "location")),
                                companyLogoUrl = ReadText(company, "logoUrl") ?? string.Empty
                            };
                        })
                        .ToList();

                    OpportunityData.Text = SafeJson(items);
                    if (items.Count == 0)
                    {
                        SetFeedback(opportunityRows.Count == 0
                            ? "No opportunity records were returned from the Firebase opportunities database path."
                            : string.Format("Firebase returned {0} opportunity record(s), but none are active, within deadline, and have available seats.", opportunityRows.Count));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Could not load student opportunities: " + ex.GetType().Name);
                    OpportunityData.Text = "[]";
                    SetFeedback("Opportunities could not be loaded. Please refresh and try again.");
                }
            }));
        }

        private static bool IsAvailable(JObject row)
        {
            // Older Firebase records use status, while newer ones use isActive.
            // Treat only an explicit inactive value as closed so records without
            // either field are not silently hidden from students.
            var activeToken = row["isActive"];
            if (activeToken != null && activeToken.Type != JTokenType.Null)
            {
                string activeValue = Normalize(activeToken.ToString());
                if (activeValue == "false" || activeValue == "0" || activeValue == "no") return false;
            }
            string status = Normalize(ReadText(row, "status"));
            if (status == "inactive" || status == "closed" || status == "expired" || status == "draft" || status == "disabled") return false;

            string deadline = ReadText(row, "deadline");
            DateTime parsedDeadline;
            if (!string.IsNullOrWhiteSpace(deadline)
                && (!DateTime.TryParse(deadline, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsedDeadline)
                    || parsedDeadline.Date < DateTime.Today)) return false;

            int seats = ReadInt(row, "availablePositions", "seats");
            int filled = ReadInt(row, "filledPositions");
            if (seats > 0 && filled >= seats) return false;
            return true;
        }

        private static List<string> ReadStrings(JObject row, params string[] keys)
        {
            foreach (string key in keys)
            {
                var token = row == null ? null : row[key];
                var array = token as JArray;
                if (array != null)
                    return array.Where(value => value != null && value.Type != JTokenType.Null)
                        .Select(value => value.ToString().Trim()).Where(value => value.Length > 0).ToList();
                var objectValues = token as JObject;
                if (objectValues != null)
                    return objectValues.Properties()
                        .Where(property => property.Value != null && property.Value.Type != JTokenType.Null)
                        .Select(property => property.Value.Type == JTokenType.Boolean && property.Value.Value<bool>()
                            ? property.Name : property.Value.ToString().Trim())
                        .Where(value => value.Length > 0).ToList();
                if (token != null && token.Type == JTokenType.String && !string.IsNullOrWhiteSpace((string)token))
                    return ((string)token).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(value => value.Trim()).Where(value => value.Length > 0).ToList();
            }
            return new List<string>();
        }

        private static int ReadInt(JObject row, params string[] keys)
        {
            foreach (string key in keys)
            {
                int value;
                if (row != null && row[key] != null && int.TryParse(row[key].ToString(), out value)) return value;
            }
            return 0;
        }

        private static string ReadText(JObject row, string key)
        {
            var token = row == null ? null : row[key];
            return token == null || token.Type == JTokenType.Null ? null : token.ToString();
        }

        private static string First(params string[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
        }

        private void SetFeedback(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            string safeMessage = JsonConvert.SerializeObject(message);
            ClientScript.RegisterStartupScript(GetType(), "opportunityLoadFeedback",
                "var feedback=document.getElementById('opportunityFeedback');if(feedback)feedback.textContent=" + safeMessage + ";", true);
        }

        private static string SafeJson(object value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })
                .Replace("<", "\\u003c").Replace(">", "\\u003e").Replace("&", "\\u0026");
        }

        private void GoToLogin()
        {
            Response.Redirect(ResolveUrl("~/PublicPages/Login.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
