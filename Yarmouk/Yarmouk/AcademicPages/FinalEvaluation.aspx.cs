using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.AcademicPages
{
    public partial class FinalEvaluation : SupervisorPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(BindEvaluationQueueAsync));
        }

        protected void EvaluationQueue_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "draft" && e.CommandName != "submit") return;
            var scoreInput = e.Item.FindControl("AcademicScoreInput") as TextBox;
            var feedbackInput = e.Item.FindControl("AcademicFeedbackInput") as TextBox;
            var meetingDateInput = e.Item.FindControl("ZoomMeetingDateInput") as TextBox;
            var meetingUrlInput = e.Item.FindControl("ZoomMeetingUrlInput") as TextBox;
            var meetingAssessmentInput = e.Item.FindControl("ZoomMeetingAssessmentInput") as TextBox;
            string placementId = Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture);
            int score;
            if (scoreInput == null || !int.TryParse(scoreInput.Text, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out score))
            {
                ShowMessage("Enter a whole-number academic score between 0 and 100.", false);
                ReopenEvaluationDialog(placementId);
                return;
            }

            string feedback = feedbackInput == null ? string.Empty : feedbackInput.Text;
            bool submit = e.CommandName == "submit";
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    var auth = GetSupervisorSession("academic");
                    await new SupervisorWorkspaceService().SaveAcademicEvaluationAsync(
                        placementId, auth.Uid, auth.IdToken, score, feedback, submit,
                        meetingDateInput == null ? string.Empty : meetingDateInput.Text,
                        meetingUrlInput == null ? string.Empty : meetingUrlInput.Text,
                        meetingAssessmentInput == null ? string.Empty : meetingAssessmentInput.Text);
                    ShowMessage(submit
                        ? "Academic evaluation submitted. It is now waiting for university approval."
                        : "Draft saved. You can continue editing until it is submitted.", true);
                    await BindEvaluationQueueAsync();
                }
                catch (SupervisorWorkflowException error)
                {
                    ShowMessage(error.Message, false);
                    ReopenEvaluationDialog(placementId);
                }
                catch (UnauthorizedAccessException)
                {
                    ShowMessage("Your academic supervisor session could not be verified. Sign in again.", false);
                    ReopenEvaluationDialog(placementId);
                }
                catch (Exception error)
                {
                    System.Diagnostics.Trace.TraceWarning("Academic evaluation save failed: " + error.GetType().Name);
                    ShowMessage("The evaluation could not be saved to Firebase. Refresh the page or contact the coordinator.", false);
                    ReopenEvaluationDialog(placementId);
                }
            }));
        }

        private async System.Threading.Tasks.Task BindEvaluationQueueAsync()
        {
            try
            {
                var auth = GetSupervisorSession("academic");
                var queue = await new SupervisorWorkspaceService().GetAcademicEvaluationQueueAsync(auth.Uid, auth.IdToken);
                EvaluationQueue.DataSource = queue;
                EvaluationQueue.DataBind();
                EvaluationQueueCount.Text = queue.Count.ToString();
                EvaluationEmpty.Visible = queue.Count == 0;
            }
            catch (UnauthorizedAccessException)
            {
                ShowMessage("Your academic supervisor session could not be verified. Sign in again.", false);
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Academic evaluation queue load failed: " + error.GetType().Name);
                ShowMessage("The evaluation queue could not be loaded from Firebase. Refresh the page or contact the coordinator.", false);
            }
        }

        private void ShowMessage(string message, bool success)
        {
            EvaluationMessage.CssClass = success ? "evaluation-alert is-success" : "evaluation-alert is-error";
            EvaluationMessageText.Text = HttpUtility.HtmlEncode(message);
            EvaluationMessage.Visible = true;
        }

        private void ReopenEvaluationDialog(string placementId)
        {
            ClientScript.RegisterStartupScript(GetType(), "reopenEvaluationDialog",
                "window.__reopenEvaluationPlacementId = \"" + HttpUtility.JavaScriptStringEncode(placementId) + "\";", true);
        }
    }
}
