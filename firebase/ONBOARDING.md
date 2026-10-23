# Local account onboarding

Accounts created from coordinator pages receive a generated temporary password. The coordinator sees it once in the registration result and must pass it to the user through an appropriate channel. At first sign-in, the application requires the user to replace it before entering any role workspace. The local worker also requests a Firebase password setup email as a fallback; the user may use the reset link and then sign in with the new password. The email is helpful but is not required for the temporary-password flow.

## Run with the local server

1. In Firebase Authentication, enable **Email/Password** and review the **Password reset** template under **Templates**. Firebase sends that template; the local worker does not need SMTP.
   Suggested message: `Your Yarmouk internship account for %EMAIL% is ready. Choose your password using %LINK%. After saving it, return to the sign-in page on the local server provided by your coordinator.` Keep `%LINK%` in the template.
2. Keep the Realtime Database URL accessible to the local server. The current project rules permit reads and writes; private rules would require a server credential and a worker update.
3. Start the ASP.NET site, then run `npm --prefix onboarding-worker start` in another terminal. Node.js 20 or later is required. Keep the worker running while accounts are added.
4. On its **first** run the worker records existing users without emailing them, except coordinator registrations marked as pending onboarding. After that, newly added profiles are processed every 30 seconds. Its local `state.json` file remembers sent invitations across restarts; keep that file on the local server.

To add a user in Firebase Console, create the Authentication account and create `/users/<Authentication UID>` in Realtime Database with matching `email`, a valid `role` (`student`, `academic`, `company`, or `coordinator`), `isActive: true`, and `mustChangePassword: true`. Create the complete record in one write. Since Firebase Console does not reveal a generated temporary password to the coordinator, the worker's password setup email is the first-login method for these accounts. The worker also supports a database-first record: it creates an Authentication account, moves that new user profile to the real Authentication UID, and sends the link. Add any role-specific `students` or `companies` data as needed; an existing `students/<old UID>` record is moved with the user profile.

Adding an Authentication account **alone** does not provide a role and cannot grant access to the application. The matching `/users/<UID>` record is required. If an email was already sent or an old account needs another link, use **Forgot password?** on the login page. Firebase accepting an email request does not guarantee inbox delivery; check spam and the Firebase Authentication email quota if it does not arrive.

The worker relies on the project's current public Realtime Database rules and Firebase Web API key. Run only one worker instance, and do not reset `state.json` unless you intentionally want to treat all current users as a fresh baseline. No Cloud Functions or Blaze plan is needed.
