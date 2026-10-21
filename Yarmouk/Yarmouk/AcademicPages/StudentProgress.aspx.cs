using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.AcademicPages
{
    public partial class StudentProgress : SupervisorPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(LoadProgressAsync));
        }

        private async System.Threading.Tasks.Task LoadProgressAsync()
        {
            try
            {
                var auth = GetSupervisorSession("academic");
                var students = await new SupervisorWorkspaceService().GetAcademicStudentsAsync(auth.Uid, auth.IdToken);
                AssignedStudents.DataSource = students;
                AssignedStudents.DataBind();
                AssignedStudentsCount.Text = students.Count.ToString();
                CaseloadCount.Text = students.Count.ToString();
                ApprovedHoursTotal.Text = students.Sum(x => x.ApprovedHours).ToString("N0");
                CompletedTasksTotal.Text = students.Sum(x => x.CompletedTasks).ToString("N0");
                ProgressEmpty.Visible = students.Count == 0;
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("Your academic supervisor session could not be verified. Sign in again.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Academic progress load failed: " + error.GetType().Name);
                ShowError("Progress data could not be loaded from Firebase. Refresh the page or contact the coordinator.");
            }
        }

        private void ShowError(string message)
        {
            ProgressErrorText.Text = HttpUtility.HtmlEncode(message);
            ProgressError.Visible = true;
            ProgressEmpty.Visible = false;
            AssignedStudentsCount.Text = "—";
            CaseloadCount.Text = "—";
            ApprovedHoursTotal.Text = "—";
            CompletedTasksTotal.Text = "—";
        }
    }
}
