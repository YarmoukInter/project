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
            int score;
            if (scoreInput == null || !int.TryParse(scoreInput.Text, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out score))
            {
                ShowMessage("Enter a whole-number academic score between 0 and 100.", false);
                return;
            }

            string placementId = Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture);
            string feedback = feedbackInput == null ? string.Empty : feedbackInput.Text;
            bool submit = e.CommandName == "submit";
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    var auth = GetSupervisorSession("academic");
                    await new SupervisorWorkspaceService().SaveAcademicEvaluationAsync(
                        placementId, auth.Uid, auth.IdToken, score, feedback, submit);
                    ShowMessage(submit
                        ? "Academic evaluation submitted. It is now waiting for university approval."
                        : "Draft saved. You can continue editing until it is submitted.", true);
                    await BindEvaluationQueueAsync();
                }
                catch (SupervisorWorkflowException error)
                {
                    ShowMessage(error.Message, false);
                }
                catch (UnauthorizedAccessException)
                {
                    ShowMessage("Your academic supervisor session could not be verified. Sign in again.", false);
                }
                catch (Exception error)
                {
                    System.Diagnostics.Trace.TraceWarning("Academic evaluation save failed: " + error.GetType().Name);
                    ShowMessage("The evaluation could not be saved to Firebase. Refresh the page or contact the coordinator.", false);
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
    }
}
