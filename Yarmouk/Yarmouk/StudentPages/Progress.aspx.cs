using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.StudentPages
{
    public partial class Progress : Page
    {
        private const int TotalTrainingHours = 240;
        private const int WeeklyTrainingHours = 30;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            RegisterAsyncTask(new PageAsyncTask(LoadProgressAsync));
        }

        private async System.Threading.Tasks.Task LoadProgressAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null)
            {
                CompletionPercent.Text = "--";
                ApprovedHours.Text = "--";
                ShowNotice("Your training progress is not available. Please sign in again.");
                return;
            }

            try
            {
                var student = await new StudentService().GetStudentAsync(auth.Uid, auth.IdToken);
                if (student == null)
                {
                    CompletionPercent.Text = "--";
                    ApprovedHours.Text = "--";
                    ShowNotice("Your progress will appear here once your student training record is available.");
                    return;
                }

                int approvedHours = Math.Max(0, student.ApprovedTrainingHours);
                int displayedHours = Math.Min(approvedHours, TotalTrainingHours);
                int percent = (int)Math.Floor(displayedHours * 100.0 / TotalTrainingHours);

                CompletionPercent.Text = percent.ToString(CultureInfo.InvariantCulture);
                ApprovedHours.Text = approvedHours.ToString("N0", CultureInfo.InvariantCulture);
                progressRing.Style["--progress"] = percent.ToString(CultureInfo.InvariantCulture) + "%";
                progressRing.Attributes["aria-label"] = "Internship progress: " + percent.ToString(CultureInfo.InvariantCulture) + " percent complete";
                progressRing.Attributes["aria-valuenow"] = percent.ToString(CultureInfo.InvariantCulture);
                hoursTrack.Style["width"] = percent.ToString(CultureInfo.InvariantCulture) + "%";

                var milestones = new List<ProgressMilestone>();
                for (int week = 1; week <= 8; week++)
                {
                    int weekHours = Math.Min(WeeklyTrainingHours, Math.Max(0, approvedHours - ((week - 1) * WeeklyTrainingHours)));
                    bool complete = weekHours == WeeklyTrainingHours;
                    bool current = weekHours > 0 && !complete;
                    milestones.Add(new ProgressMilestone
                    {
                        Week = week.ToString("00", CultureInfo.InvariantCulture),
                        HoursLabel = weekHours.ToString(CultureInfo.InvariantCulture),
                        Fill = (weekHours * 100 / WeeklyTrainingHours).ToString(CultureInfo.InvariantCulture),
                        CssClass = complete ? "is-complete" : (current ? "is-current" : "is-upcoming"),
                        Marker = complete ? "✓" : week.ToString(CultureInfo.InvariantCulture)
                    });
                }

                MilestoneRepeater.DataSource = milestones;
                MilestoneRepeater.DataBind();
            }
            catch (Exception)
            {
                CompletionPercent.Text = "--";
                ApprovedHours.Text = "--";
                ShowNotice("We could not load your training progress right now. Please try again later.");
            }
        }

        private void ShowNotice(string message)
        {
            NoticeText.Text = Server.HtmlEncode(message);
            ProgressNotice.Visible = true;
        }

        public sealed class ProgressMilestone
        {
            public string Week { get; set; }
            public string HoursLabel { get; set; }
            public string Fill { get; set; }
            public string CssClass { get; set; }
            public string Marker { get; set; }
        }
    }
}
