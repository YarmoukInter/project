using System;
using System.Linq;
using System.Globalization;
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
            if (Request.QueryString["noteSaved"] == "1") ShowSuccess("Your progress note was saved and is visible to the student.");
            if (Request.QueryString["hoursSaved"] == "1") ShowSuccess("The verified training hours were approved and recorded.");
            RegisterAsyncTask(new PageAsyncTask(LoadProgressAsync));
        }

        protected void SaveProgressNote_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(SaveProgressNoteAsync));
        }

        protected void SaveApprovedHours_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(SaveApprovedHoursAsync));
        }

        private async System.Threading.Tasks.Task SaveProgressNoteAsync()
        {
            try
            {
                var auth = GetSupervisorSession("academic");
                await new SupervisorWorkspaceService().SaveStudentProgressNoteAsync(
                    NotePlacementId.Value, auth.Uid, auth.IdToken, NoteBody.Text);
                Response.Redirect(ResolveUrl("~/AcademicPages/StudentProgress.aspx?noteSaved=1"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SupervisorWorkflowException error)
            {
                ShowSaveError(error.Message);
            }
            catch (UnauthorizedAccessException)
            {
                ShowSaveError("Your academic supervisor session could not be verified. Sign in again.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Academic progress note save failed: " + error.GetType().Name);
                ShowSaveError("The progress note could not be saved. Please try again.");
            }
        }

        private async System.Threading.Tasks.Task SaveApprovedHoursAsync()
        {
            try
            {
                int hours;
                if (!int.TryParse(ApprovedHoursInput.Text, NumberStyles.None, CultureInfo.InvariantCulture, out hours))
                {
                    ShowSaveError("Enter a whole number of hours to approve.");
                    return;
                }
                var auth = GetSupervisorSession("academic");
                await new SupervisorWorkspaceService().RecordApprovedHoursAsync(
                    HoursPlacementId.Value, auth.Uid, auth.IdToken, hours, ApprovedHoursNote.Text);
                Response.Redirect(ResolveUrl("~/AcademicPages/StudentProgress.aspx?hoursSaved=1"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SupervisorWorkflowException error)
            {
                ShowSaveError(error.Message);
            }
            catch (UnauthorizedAccessException)
            {
                ShowSaveError("Your academic supervisor session could not be verified. Sign in again.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Approved training hours save failed: " + error.GetType().Name);
                ShowSaveError("The approved hours could not be saved. Please try again.");
            }
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

        private void ShowSaveError(string message)
        {
            ProgressErrorText.Text = HttpUtility.HtmlEncode(message);
            ProgressError.Visible = true;
        }

        private void ShowSuccess(string message)
        {
            ProgressSuccessText.Text = HttpUtility.HtmlEncode(message);
            ProgressSuccess.Visible = true;
        }
    }
}
