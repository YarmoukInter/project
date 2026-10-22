using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.CompanyPages
{
    public partial class MyTrainees : SupervisorPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(LoadTraineesAsync));
        }

        private async System.Threading.Tasks.Task LoadTraineesAsync()
        {
            try
            {
                var auth = GetSupervisorSession("company");
                var trainees = await new SupervisorWorkspaceService().GetCompanyTraineesAsync(auth.Uid, auth.IdToken);
                TraineesList.DataSource = trainees;
                TraineesList.DataBind();
                TraineesList.Visible = trainees.Count > 0;
                TraineeCount.Text = trainees.Count.ToString();
                TraineeListCount.Text = trainees.Count.ToString();
                TraineeApprovedHours.Text = trainees.Sum(x => x.ApprovedHours).ToString("N0");
                TraineeCompletedTasks.Text = trainees.Sum(x => x.CompletedTasks).ToString("N0");
                TraineesEmpty.Visible = trainees.Count == 0;
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("Your company supervisor session could not be verified. Sign in again.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company trainees load failed: " + error.GetType().Name);
                ShowError("Trainee data could not be loaded from Firebase. Refresh the page or contact the coordinator.");
            }
        }

        private void ShowError(string message)
        {
            TraineesErrorText.Text = HttpUtility.HtmlEncode(message);
            TraineesError.Visible = true;
            TraineesList.Visible = false;
            TraineesEmpty.Visible = false;
            TraineeCount.Text = "—";
            TraineeListCount.Text = "—";
            TraineeApprovedHours.Text = "—";
            TraineeCompletedTasks.Text = "—";
        }
    }
}
