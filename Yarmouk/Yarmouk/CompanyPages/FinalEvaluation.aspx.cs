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

        private static readonly string[] Criteria =
        {
            "His/Her attendance and punctuality",
            "His/Her involvement in social events of the company/establishment",
            "Ability to adapt to the work environment and understand the nature of activities in the company/establishment",
            "His/Her appreciation of security and safety rules and level of commitment to them",
            "Ability to comprehend and respond to the directions of the training supervisor",
            "Ability to understand others’ point of view and communicate his/her own",
            "Ability to fit into the team",
            "His/Her self-initiative and autonomy",
            "Ability to apply gained theoretical knowledge and willingness to learn more",
            "Ability to accomplish given tasks within the specified timetable",
            "His/Her manual skills (assembling, maintenance, installation)",
            "His/Her skills in preparing sketches and engineering plans for a given idea",
            "His/Her computer and programming skills (software and hardware)",
            "His/Her creativity, originality and imagination",
            "Ability to conduct research and efficiently use the available resources",
            "Interest and usefulness of the work for the establishment",
            "His/Her overall quality of work",
            "General quality of the student’s report (contents, syntax, presentation, literature survey)"
        };

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

        protected void ReportList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "review") return;
            var decision = e.Item.FindControl("ReviewDecision") as DropDownList;
            var feedback = e.Item.FindControl("ReviewFeedback") as TextBox;
            string status = decision == null ? string.Empty : decision.SelectedValue;
            string notes = feedback == null ? string.Empty : feedback.Text;
            string submissionId = Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture);
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    var auth = GetSupervisorSession("company");
                    string companyId = await EnsureCompanyIdAsync(auth);
                    await service.SaveReportReviewAsync(auth.Uid, companyId, auth.IdToken, SelectedPlacementId.Value,
                        submissionId, status, notes);
                    ShowMessage("The report review was saved to Firebase.", true);
                    await BindPageAsync();
                }
                catch (ArgumentException error) { ShowMessage(error.Message, false); await BindPageAsync(); }
                catch (Exception error)
                {
                    System.Diagnostics.Trace.TraceWarning("Company report review failed: " + error.GetType().Name + ": " + error.Message);
                    ShowMessage("The report review could not be saved. Check your Firebase rules and try again.", false);
                    await BindPageAsync();
                }
            }));
        }

        protected void ReportList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
            var row = e.Item.DataItem as CompanyReportRow;
            if (row == null) return;
            var fileLink = e.Item.FindControl("ReportFileLink") as HyperLink;
            Uri link;
            if (fileLink != null && Uri.TryCreate(row.SubmissionUrl, UriKind.Absolute, out link)
                && (link.Scheme == Uri.UriSchemeHttps || link.Scheme == Uri.UriSchemeHttp))
            {
                fileLink.NavigateUrl = link.AbsoluteUri;
                fileLink.Visible = true;
            }
            var decision = e.Item.FindControl("ReviewDecision") as DropDownList;
            if (decision != null)
            {
                var existing = decision.Items.FindByValue(row.Status);
                if (existing != null) decision.SelectedValue = row.Status;
                bool reviewed = string.Equals(row.Status, "Approved", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(row.Status, "RevisionRequested", StringComparison.OrdinalIgnoreCase);
                decision.Enabled = !reviewed;
                var button = e.Item.FindControl("SaveReportReview") as Button;
                if (button != null) button.Enabled = !reviewed;
            }
        }

        protected void SaveDraft_Click(object sender, EventArgs e) { RegisterAsyncTask(new PageAsyncTask(() => SaveEvaluationAsync(false))); }
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
                    ? "The final company evaluation was submitted to the academic supervisor. You can print or save a PDF copy."
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
                    CanEvaluate = !string.IsNullOrWhiteSpace(x.PlacementId),
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
                PlacementLabel.Text = EncodeOrDash(selected.CompanyName);
                DetailsCompany.Text = EncodeOrDash(selected.CompanyName);
                DetailsStatus.Text = EncodeOrDash(selected.PlacementStatus);
                DetailsStart.Text = EncodeOrDash(selected.StartDate);
                DetailsEnd.Text = EncodeOrDash(selected.EndDate);
                EvaluationStatus.Text = selected.Locked ? (selected.Submitted ? "Submitted to academic supervisor" : "Locked") : "Draft · editable";
                CompanyScore.Text = selected.Score ?? string.Empty;
                CompanyFeedback.Text = selected.Feedback ?? string.Empty;
                CompanyScoreSummary.Text = string.IsNullOrWhiteSpace(selected.Score) ? "—" : HttpUtility.HtmlEncode(selected.Score + " / 100");
                CriteriaMarkup.Text = BuildCriteriaMarkup(selected.Criteria, selected.Locked);
                CompanyScore.Enabled = !selected.Locked;
                CompanyFeedback.Enabled = !selected.Locked;
                SaveDraft.Enabled = !selected.Locked;
                SubmitEvaluation.Enabled = !selected.Locked;
                if (selected.Locked) { SaveDraft.ToolTip = "This evaluation is locked after submission."; }

                var reports = await service.GetReportsAsync(auth.Uid, companyId, auth.IdToken, selected.PlacementId);
                ReportList.DataSource = reports;
                ReportList.DataBind();
                ReportCount.Text = reports.Count.ToString(CultureInfo.InvariantCulture);
                ReportsEmpty.Visible = reports.Count == 0;
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