# Login setup

The provided Firebase Web API key and Realtime Database URL are set in Web.config. Enable Authentication > Email/Password in Firebase Console.

The supplied uims_firebase_realtime_database_seed.json is initial database data, not authentication accounts. Each profile key and its uid field must equal the matching Firebase Authentication UID. Profiles use uid, fullName, email, role, isActive and mustChangePassword. The seed role values student, c_hr, uni_sp, and admin map to student, company, academic supervisor, and coordinator in the app.

Merge the users rule from ../../firebase/auth-profile.rules.json with existing database rules. It lets a signed-in user read their own profile and lets a student change only their own mustChangePassword value from true to false. Do not replace the complete team rules with this example.

If password change succeeds but setup cannot finish, deploy that users rule in Firebase Console. While the current browser session remains open, the student can then click Continue to workspace again; the app retries only the profile flag update and does not change the password a second time. If the session has expired, the coordinator can set that student's users/{uid}/mustChangePassword value to false after confirming the password change.

The login page uses the repository's stone gateway design and its own CSS file. The password setup page also has its own CSS file. Login checks credentials with Firebase Authentication, gets the role from the matching user profile and directs first-login students to password setup. Password setup verifies the temporary password, changes it in Firebase and clears the profile flag. Password recovery uses Firebase reset email.

To run, open Yarmouk.slnx in Visual Studio, build and start IIS Express. Use a Firebase test account with a matching users/{uid} profile. No Firebase records or rules have been changed by this code update.

A read-only check on 2026-10-20 found that the live users index and a profile could be read without authentication. Review the deployed rules in Firebase Console. The other internship pages remain team templates and require their own workflow implementation.
