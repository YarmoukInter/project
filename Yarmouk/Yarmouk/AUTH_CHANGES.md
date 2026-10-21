# Authentication changes

The login page now uses Firebase Authentication and keeps the university gateway design. Its CSS and presentation JavaScript are separate team-owned files: Content/PublicPages/Login.css and Scripts/PublicPages/Login.js.

StudentPages/ChangePassword.aspx has a dedicated CSS file and password visibility script. The server verifies the temporary password, updates Firebase Authentication, then clears mustChangePassword for the signed-in student. Seed roles c_hr, uni_sp, and admin are mapped to the app roles company, academic, and coordinator.

Helpers/AuthGuard.cs and AuthPolicy.cs route users from their database role and require password setup on the student's first login. Services/FirebaseAuthService.cs connects to Firebase; Services/PasswordSetupService.cs updates the setup flag. Web.config contains the supplied Firebase web key and database URL. The university logo and ASP.NET project registrations were updated. Team setup docs and Firebase rules example were added.

The supplied seed fields match the account model but do not create Firebase Authentication accounts. A live read check found public access to the users index/profile. No Firebase rules or records were changed. Auth pages compiled and layouts were reviewed at desktop and mobile widths; see AUTH_SETUP.md for setup steps.

Login troubleshooting found that a valid student profile was keyed by its seed UID while the Firebase Authentication account had a different UID. The login page now explains this mismatch. Roles c_hr, uni_sp and admin are also mapped to the app's company, academic and coordinator roles.
