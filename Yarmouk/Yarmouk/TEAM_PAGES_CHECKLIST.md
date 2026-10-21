# Required 32 pages

| Area | Pages |
|---|---|
| Public (4) | Home, AboutUs, ContactUs, Login |
| Student (8) | ChangePassword, Dashboard, Profile, Opportunities, Apply, MyApplications, MyTasks, Progress |
| Company (7) | Dashboard, ManageOpportunities, ApplicationsReview, MyTrainees, ManageTasks, FinalEvaluation, Account |
| Academic (4) | Dashboard, StudentProgress, FinalEvaluation, Account |
| Coordinator (9) | Dashboard, ManageStudents, ManageCompanies, ManageAcademicSupervisors, AssignAcademicSupervisors, MonitorPlacements, FinalCompletionReview, ContactMessages, Account |

Each page has .aspx, .aspx.cs, .aspx.designer.cs, and separate CSS/JS. Public Home uses Home.Master, other sections use Section.Master.

## Before running
- Review database fields against the approved JSON before connecting live data.
- FirebaseDatabaseUrl must be configured in Web.config locally.
- Login, server role guards, and first-time password change are implemented; see AUTH_SETUP.md for the required Firebase configuration. Approvals and other feature write workflows remain unimplemented.
- Only the student Profile has a simple hardcoded Model example.
- No live Firebase write is enabled by these page templates.
