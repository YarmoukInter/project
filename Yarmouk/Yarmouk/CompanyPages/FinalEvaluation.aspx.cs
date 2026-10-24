using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.CompanyPages
{
    public partial class FinalEvaluation : SupervisorPageBase
    {
        private readonly CompanySupervisorService service = new CompanySupervisorService();

        private static readonly string[] Criteria = CompanyEvaluationPdf.Criteria;

        private static readonly string[] Levels = { "Excellent", "Very Good", "Good", "Satisfactory", "Poor", "Fail" };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string requestedPlacementId = Request.QueryString["placementId"];
                if (!string.IsNullOrWhiteSpace(requestedPlacementId))
                    SelectedPlacementId.Value = requestedPlacementId;
                RegisterAsyncTask(new PageAsyncTask(BindPageAsync));
            }
        }

        protected void SubmitEvaluation_Click(object sender, EventArgs e) { RegisterAsyncTask(new PageAsyncTask(() => SaveEvaluationAsync(true))); }

        private async System.Threading.Tasks.Task SaveEvaluationAsync(bool submit)
        {
            try
            {
                var auth = GetSupervisorSession("company");
                string companyId = await EnsureCompanyIdAsync(auth);
                int score;
                int? scoreValue = int.TryParse(CompanyScore.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out score)
                    && score >= 0 && score <= 100 ? (int?)score : null;
                var ratings = ReadRatings();
                await service.SaveCompanyEvaluationAsync(auth.Uid, companyId, auth.IdToken, SelectedPlacementId.Value,
                    scoreValue, CompanyFeedback.Text, ratings, submit);
                ShowMessage(submit
                    ? "The final company evaluation was submitted to the academic supervisor. The assigned academic supervisor can now download its PDF copy."
                    : "Your evaluation draft was saved to Firebase.", true);
                await BindPageAsync();
                ActiveEvaluationTab.Value = "review";
                RegisterEvaluationScript("showReviewTab", "window.uimsSelectEvaluationTab && window.uimsSelectEvaluationTab('review');");
            }
            catch (ArgumentException error)
            {
                ShowMessage(error.Message, false);
            }
            catch (UnauthorizedAccessException)
            {
                ShowMessage("Your company supervisor session or placement could not be verified. Sign in again or contact the coordinator.", false);
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company evaluation save failed: " + error.GetType().Name + ": " + error.Message);
                ShowMessage("The evaluation could not be saved to Firebase. Check your connection and Firebase rules, then try again.", false);
            }
        }

        private Dictionary<string, string> ReadRatings()
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 1; i <= Criteria.Length; i++)
            {
                string value = Request.Form["companyCriterion_" + i];
                if (!string.IsNullOrWhiteSpace(value)) values[i.ToString(CultureInfo.InvariantCulture)] = value;
            }
            return values;
        }

        private async System.Threading.Tasks.Task BindPageAsync()
        {
            try
            {
                var auth = GetSupervisorSession("company");
                string companyId = await EnsureCompanyIdAsync(auth);
                var queue = await service.GetEvaluationQueueAsync(auth.Uid, companyId, auth.IdToken);
                var evaluableQueue = queue.Where(x => !string.IsNullOrWhiteSpace(x.PlacementId)).ToList();
                if (evaluableQueue.Count > 0 && !evaluableQueue.Any(x => string.Equals(x.PlacementId, SelectedPlacementId.Value, StringComparison.Ordinal)))
                    SelectedPlacementId.Value = evaluableQueue[0].PlacementId;
                var traineeCards = queue.Select(x => new TraineeCard
                {
                    PlacementId = x.PlacementId,
                    StudentName = string.IsNullOrWhiteSpace(x.StudentName) ? "Student" : x.StudentName,
                    StudentNumber = string.IsNullOrWhiteSpace(x.StudentNumber) ? "Student number not recorded" : x.StudentNumber,
                    Initials = GetInitials(x.StudentName),
                    ProfileImageUrl = x.ProfileImageUrl,
                    CanEvaluate = !string.IsNullOrWhiteSpace(x.PlacementId) && !x.Locked,
                    ToolTip = string.IsNullOrWhiteSpace(x.PlacementId) ? "A placement record must be linked before reports or evaluations can be opened." : string.Empty,
                    CssClass = string.IsNullOrWhiteSpace(x.PlacementId) ? "trainee-row is-unlinked"
                        : (string.Equals(x.PlacementId, SelectedPlacementId.Value, StringComparison.Ordinal)
                            ? "trainee-row is-selected" : "trainee-row"),
                    SelectUrl = string.IsNullOrWhiteSpace(x.PlacementId) ? string.Empty
                        : ResolveUrl("~/CompanyPages/FinalEvaluation.aspx?placementId=" + HttpUtility.UrlEncode(x.PlacementId)),
                    StatusLabel = string.IsNullOrWhiteSpace(x.PlacementId) ? "Placement not linked" : (x.Locked ? "Submitted" : "In progress"),
                    MajorId = string.IsNullOrWhiteSpace(x.MajorId) ? "—" : x.MajorId,
                    PlacementStatus = string.IsNullOrWhiteSpace(x.PlacementStatus) ? "—" : x.PlacementStatus,
                    StartDate = string.IsNullOrWhiteSpace(x.StartDate) ? "—" : x.StartDate,
                    EndDate = string.IsNullOrWhiteSpace(x.EndDate) ? "—" : x.EndDate
                }).ToList();
                TraineeList.DataSource = traineeCards;
                TraineeList.DataBind();
                TraineeCount.Text = queue.Count.ToString(CultureInfo.InvariantCulture);
                TraineesEmpty.Visible = queue.Count == 0;

                if (evaluableQueue.Count == 0)
                {
                    EvaluationWorkspace.Visible = false;
                    EvaluationEmpty.Visible = true;
                    return;
                }

                var selected = evaluableQueue.FirstOrDefault(x => string.Equals(x.PlacementId, SelectedPlacementId.Value, StringComparison.Ordinal))
                    ?? evaluableQueue[0];
                SelectedPlacementId.Value = selected.PlacementId;
                EvaluationWorkspace.Visible = true;
                EvaluationEmpty.Visible = false;
                StudentName.Text = EncodeOrDash(selected.StudentName);
                DetailsStudentName.Text = EncodeOrDash(selected.StudentName);
                StudentNumber.Text = EncodeOrDash(selected.StudentNumber);
                DetailsStudentNumber.Text = EncodeOrDash(selected.StudentNumber);
                StudentInitials.Text = HttpUtility.HtmlEncode(GetInitials(selected.StudentName));
                StudentProfileImageUrl = HttpUtility.HtmlAttributeEncode(selected.ProfileImageUrl ?? string.Empty);
                PlacementLabel.Text = EncodeOrDash(selected.CompanyName);
                DetailsCompany.Text = EncodeOrDash(selected.CompanyName);
                DetailsStatus.Text = EncodeOrDash(selected.PlacementStatus);
                DetailsStart.Text = EncodeOrDash(selected.StartDate);
                DetailsEnd.Text = EncodeOrDash(selected.EndDate);
                EvaluationStatus.Text = selected.Locked ? (selected.Submitted ? "Submitted to academic supervisor" : "Locked") : "Draft · editable";
                CompanyScore.Text = selected.Score ?? string.Empty;
                CompanyFeedback.Text = selected.Feedback ?? string.Empty;
                CriteriaMarkup.Text = BuildCriteriaMarkup(selected.Criteria, selected.Locked);
                CompanyScore.Enabled = !selected.Locked;
                CompanyFeedback.Enabled = !selected.Locked;
                bool hasAcademicSupervisor = !string.IsNullOrWhiteSpace(selected.AcademicSupervisorUid);
                SubmitEvaluation.Enabled = !selected.Locked && hasAcademicSupervisor;
                SubmitEvaluation.Attributes["data-academic-supervisor-assigned"] = hasAcademicSupervisor ? "true" : "false";
                SubmitEvaluation.Attributes["data-evaluation-locked"] = selected.Locked ? "true" : "false";
                SubmitEvaluation.ToolTip = selected.Locked
                    ? "This evaluation is locked after submission."
                    : (hasAcademicSupervisor ? string.Empty : "Assign an academic supervisor before sending the final evaluation.");
                EvaluationGuidance.Text = selected.Locked
                    ? "This evaluation is locked after submission."
                    : (hasAcademicSupervisor
                        ? "Complete all 18 criteria and enter an overall score from 0 to 100 to enable submission. Sending the evaluation locks it and shares it with the assigned academic supervisor."
                        : "Assign an academic supervisor to enable final submission. Changes are not saved until the evaluation is submitted.");

            }
            catch (UnauthorizedAccessException error)
            {
                EvaluationWorkspace.Visible = false;
                EvaluationEmpty.Visible = true;
                ShowMessage(error.Message, false);
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company reports and evaluation load failed: " + error.GetType().Name + ": " + error.Message);
                EvaluationWorkspace.Visible = false;
                EvaluationEmpty.Visible = true;
                ShowMessage("Firebase records could not be loaded. Check read access to students, placements, evaluations, tasks, and taskSubmissions.", false);
            }
        }

        private static string BuildCriteriaMarkup(IDictionary<string, string> saved, bool disabled)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < Criteria.Length; i++)
            {
                int number = i + 1;
                string key = number.ToString(CultureInfo.InvariantCulture);
                string current = saved != null && saved.ContainsKey(key) ? saved[key] : string.Empty;
                builder.Append("<fieldset class=\"criteria-row\"><legend><span class=\"criteria-number\">")
                    .Append(number.ToString("00", CultureInfo.InvariantCulture)).Append("</span><span class=\"criteria-label\">")
                    .Append(HttpUtility.HtmlEncode(Criteria[i])).Append("</span></legend><div class=\"criteria-options\">");
                foreach (string level in Levels)
                    AppendRating(builder, number, level, current, disabled);
                if (number >= 11 && number <= 13) AppendRating(builder, number, "N/A", current, disabled);
                builder.Append("</div></fieldset>");
            }
            return builder.ToString();
        }

        private static void AppendRating(StringBuilder builder, int number, string level, string selected, bool disabled)
        {
            string id = "companyCriterion_" + number.ToString(CultureInfo.InvariantCulture) + "_" + level.Replace(" ", string.Empty).Replace("/", string.Empty);
            builder.Append("<label class=\"rating-option").Append(string.Equals(selected, level, StringComparison.Ordinal) ? " is-selected" : "")
                .Append("\" for=\"").Append(id).Append("\"><input type=\"radio\" id=\"").Append(id)
                .Append("\" name=\"companyCriterion_").Append(number.ToString(CultureInfo.InvariantCulture)).Append("\" value=\"")
                .Append(HttpUtility.HtmlAttributeEncode(level)).Append("\"")
                .Append(string.Equals(selected, level, StringComparison.Ordinal) ? " checked=\"checked\"" : "")
                .Append(disabled ? " disabled=\"disabled\"" : "").Append(" /><span class=\"rating-option__name\">")
                .Append(HttpUtility.HtmlEncode(level)).Append("</span><span class=\"rating-option__range\">").Append(RatingRange(level)).Append("</span></label>");
        }

        private static string RatingRange(string level)
        {
            switch (level)
            {
                case "Excellent": return "90–100%";
                case "Very Good": return "80–90%";
                case "Good": return "70–80%";
                case "Satisfactory": return "60–70%";
                case "Poor": return "50–60%";
                case "Fail": return "Below 50%";
                default: return "Not applicable";
            }
        }

        private static string GetInitials(string name)
        {
            var words = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "YU";
            return string.Concat(words.Take(2).Select(x => x.Substring(0, 1).ToUpperInvariant()));
        }

        protected string StudentProfileImageUrl { get; private set; }

        private static string EncodeOrDash(string value)
        { return string.IsNullOrWhiteSpace(value) ? "—" : HttpUtility.HtmlEncode(value); }

        private string resolvedCompanyId;

        private async System.Threading.Tasks.Task<string> EnsureCompanyIdAsync(AuthSession auth)
        {
            if (auth == null) throw new UnauthorizedAccessException("A verified company supervisor session is required.");
            if (string.IsNullOrWhiteSpace(resolvedCompanyId))
                resolvedCompanyId = await service.GetCompanyIdForSupervisorAsync(auth.Uid, auth.IdToken);
            return resolvedCompanyId;
        }

        private void ShowMessage(string message, bool success)
        {
            EvaluationMessage.CssClass = success ? "evaluation-alert is-success" : "evaluation-alert is-error";
            EvaluationMessageText.Text = HttpUtility.HtmlEncode(message);
            EvaluationMessage.Visible = true;
        }

        private void RegisterEvaluationScript(string key, string script)
        { ClientScript.RegisterStartupScript(GetType(), key, script, true); }

        private sealed class TraineeCard
        {
            public string PlacementId { get; set; }
            public string StudentName { get; set; }
            public string StudentNumber { get; set; }
            public string Initials { get; set; }
            public string ProfileImageUrl { get; set; }
            public string StatusLabel { get; set; }
            public bool CanEvaluate { get; set; }
            public string ToolTip { get; set; }
            public string CssClass { get; set; }
            public string SelectUrl { get; set; }
            public string MajorId { get; set; }
            public string PlacementStatus { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
        }
    }
}
