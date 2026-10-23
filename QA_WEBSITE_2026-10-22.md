# Website QA — 2026-10-22

Baseline: develop, commit f77a4a0. git pull --ff-only: already up to date.

## Most urgent findings

1. P0: deployed Firebase permits anonymous reads of private users, students, placements and contact messages.
2. High: offer confirmation does not create a placement (QA-23), blocking the new-student journey.
3. High: uploaded CV is absent from company application review (QA-22).
4. High: company opportunity creation fails; task revision statuses conflict; company evaluation tabs and coordinator completion are unfinished/broken.

Full findings, passing scenarios and explicit limits follow. Evidence screenshot: C:/Users/user/AppData/Local/Temp/qa-confirmed-without-placement.png.

## Verified so far

- Clean working tree before QA; no application source changes made.
- Debug MSBuild passed.
- IIS Express started using the existing project configuration; HTTP 64041 and HTTPS 44335 are listening.
- Code review: student profile reads users/{authenticated UID} and students/{authenticated UID}.
- Code review: coordinator registration creates a Firebase Authentication account first and uses its returned UID for database records.
- Code review: coordinator forms check a per-session CSRF token and redirect after saving.
- Code review: role routing normalizes admin/coordinator, uni_sp/academic and c_hr/company.

These code checks do not prove that live Firebase rules or account records permit the intended operations.

## Browser QA status

Browser access recovered. Visible role-by-role testing is in progress. Dedicated QA student registration and first-login password setup completed. Student application and company offer save/read roundtrips passed. Offer confirmation subsequently persisted. The resulting missing placement record blocks downstream QA-only task and evaluation scenarios (QA-23).

## Coverage checklist (results and remaining limits below)

- Valid and invalid login, empty fields, wrong password, logout, back navigation after logout.
- All four roles: correct dashboard, direct access to other-role routes, unauthenticated access.
- Session expiry, inactive accounts, initial temporary-password handling.
- Desktop 1440px, tablet 768px, phone 390px: navigation, overflow, readable forms, focus, dialogs, reduced motion.
- Refresh after save, cancel, validation failures, duplicate submission, loading and empty states.
- Firebase persistence: save, reload, sign out/in, and confirm reads from a second authorized role.
- Rule enforcement and unauthorized record access; distinguish source rules from deployed rules.
- End-to-end QA-only placement: application, offer, confirmation, task, feedback, hours, evaluations, completion.
- Do not change provided account passwords or delete original records during testing.

## Page inventory (checked means page opened and primary read/UI inspected, not all workflows passed)

### Public

- [x] PublicPages/AboutUs.aspx
- [x] PublicPages/ContactUs.aspx
- [x] PublicPages/Home.aspx
- [x] PublicPages/Login.aspx
- [ ] PublicPages/Team.aspx

### Coordinator

- [x] CoordinatorPages/Account.aspx
- [x] CoordinatorPages/AssignAcademicSupervisors.aspx
- [x] CoordinatorPages/ContactMessages.aspx
- [x] CoordinatorPages/Dashboard.aspx
- [x] CoordinatorPages/FinalCompletionReview.aspx
- [x] CoordinatorPages/ManageAcademicSupervisors.aspx
- [x] CoordinatorPages/ManageCompanies.aspx
- [x] CoordinatorPages/ManageStudents.aspx
- [x] CoordinatorPages/MonitorPlacements.aspx

### Student

- [ ] StudentPages/Apply.aspx
- [ ] StudentPages/ChangePassword.aspx
- [x] StudentPages/Dashboard.aspx
- [x] StudentPages/MyApplications.aspx
- [x] StudentPages/MyTasks.aspx
- [x] StudentPages/Opportunities.aspx
- [x] StudentPages/Profile.aspx
- [x] StudentPages/Progress.aspx

### Company

- [x] CompanyPages/Account.aspx
- [x] CompanyPages/ApplicationsReview.aspx
- [x] CompanyPages/Dashboard.aspx
- [x] CompanyPages/FinalEvaluation.aspx
- [x] CompanyPages/ManageOpportunities.aspx
- [x] CompanyPages/ManageTasks.aspx
- [x] CompanyPages/MyTrainees.aspx

### Academic

- [x] AcademicPages/Account.aspx
- [x] AcademicPages/Dashboard.aspx
- [x] AcademicPages/FinalEvaluation.aspx
- [x] AcademicPages/StudentProgress.aspx

## Data scenarios pending

- Coordinator: register/edit/activate student, academic supervisor, company; assignment; placement monitoring; completion; contact-message read/status; notifications.
- Student: exact academic profile, editable fields, skills, valid/invalid CV upload and download; opportunity major/capacity filters; duplicate application; accepted-offer confirmation; task submission/revision; hours; progress.
- Company: own-company opportunity create/edit/close, custom questions, application accept/reject, trainees, tasks and feedback, evaluations, account.
- Academic: assigned-only students, progress, hours approval/return, final evaluation, account.
- Public: homepage gate/book/filters/major links/FAQ/video, About emblem/tour, Contact validation and coordinator receipt.

## Code review follow-ups (not yet reproduced defects)

- Registration sets mustChangePassword for every role, while AuthPolicy redirects only students for password setup. Verify intended onboarding for newly created academic/company accounts.
- Student model has CompletedCreditHours and CompletedCredits, plus RequiredTrainingHours and RequiredHours. Verify all create/read/progress views use the intended fields consistently.
- ApplicationService falls back from an indexed student query to a collection read on Firebase HTTP 400. Verify deployed indexes and whether restrictive rules block the fallback.
- Contact submission uses an extension method in the coordinator code; verify service authentication and deployed rules through a QA-marked message.

No credentials or tokens are recorded in this report.
## Browser run — confirmed results (continued)

- Browser control recovered after settings restart. Fresh visible preview tab created because old tab retained connection-error state.
- PASS: coordinator empty login shows validation; wrong password shows generic authentication error; correct credentials route to coordinator dashboard.
- PASS: dashboard reads 17 active users, 7 active students, 3 partner companies, 4 academic supervisors, 3 opportunities, 5 placements.
- PASS: notifications panel opens with zero unread and a messages link.
- PASS: Manage Students, Manage Companies, Manage Academic Supervisors and Monitor Placements load Firebase-backed records.
- PASS: student registration empty submit focuses required Arabic name and shows validation; cancel closes editor.
- PASS: placement search returns clear no-match state.
- FAIL QA-01 (high): Final Completion Review > Review record causes a form POST/reload and clears all 5 visible records to 0 instead of opening usable details. Generated .fr-open button has no type="button" inside WebForms form; Page_Load only loads data on !IsPostBack. Close/filter buttons also need audit for default-submit behavior.
- FAIL QA-02 (high, code-confirmed unfinished feature): FinalCompletionReview.js hard-disables Approve completion and Generate/send official PDF. Current screen is read-only; full completion journey cannot succeed through this page.
- DATA QA-03: Wessam is Unassigned in Manage Students, while Monitor Placements displays omar for his placement. Verify student/placement supervisor synchronization.
- DATA QA-04: one Test Student has two placement rows; one references an unavailable training provider. This is existing data; no records changed by QA.
- UI QA-05: final completion review shows generic Student for Wessam and Test Student while Monitor Placements resolves their actual names.
- PASS: coordinator logout redirects to login; direct ManageStudents access after logout also redirects to login.
- PASS: provided student logs in and sees his own name, number, confirmed Orange placement, tasks, hours and profile.
- PASS: skill QA marker saved, survived full reload, then removed and original lists saved back successfully.
- PASS: confirmed internship prevents further applications; Applications Pending filter shows a clear no-match state.
- PASS: student attempt to open CoordinatorPages/ManageStudents redirects to student dashboard without exposing admin data.
- FAIL QA-06 (high): Write Database Queries is Revision requested with grade 5 and feedback in Progress/Dashboard, but My Tasks places it in Complete, says No supervisor feedback yet, and prevents resubmission. Conflicting task/submission status sources break the revision loop.
- DATA QA-08: Wessam academic record fields English name, national ID, gender, nationality, birthplace, DOB, GPA/status/graduation are blank; student number/major/hours load. Determine whether record lacks them or mapping is wrong by checking admin edit.
- UI QA-09: sidebar expands on pointer hover and shifts Profile/theme controls enough that a mouse click aimed at Profile toggled dark mode. Keyboard activation reaches the correct link. Audit stable sidebar hit targets; theme restored to light.
- LIMIT: coordinator account and security settings are explicitly read-only/disabled; cannot test profile/password writes through that page.
- FAIL QA-10 (high): valid company Add opportunity QA record fails with "Could not save the opportunity: Unknown request." No QA opportunity persisted. JS posts JSON to data-api-url (ASPX URL); FriendlyUrls permanent redirect likely converts POST to GET. Confirm through redirect-safe endpoint fix; GET list works.
- FAIL QA-12 (high): Company FinalEvaluation tabs do not switch. General information click leaves Reports selected and no fields appear. Scripts/CompanyPages/FinalEvaluation.js contains only a placeholder comment; no tab handlers. Draft/final fields are hidden and inaccessible through normal UI.
- DATA QA-13: company FinalEvaluation lists Wessam and Wessam Ali as two rows with same student number and same placementId=DEMO_PLC_003; My Trainees lists only one Wessam. Duplicate or legacy student mapping needs normalization.
- UI QA-14: company Applications Review says "Offer sent; waiting for student confirmation" for Wessam although student Applications says final choice confirmed and My Trainees lists him.
- PASS: company application details and answers load; existing accepted status visible; My Trainees reads four placement cards with hours and tasks.
- PASS: company task editor enforces required task number; selecting Wessam loads six tasks and review dialog.
- PASS: company Account Save changes (existing values) confirms Firebase save and reload; logout works.
- DATA follow-up: company Tasks selector lists Ali, Demo Student One, Test Student and Wessam while My Trainees lists Demo Student One, Demo Student Two, Test Student and Wessam.
- PASS: academic login succeeds; Student Progress Monitor lists 3 placements and approved hours; academic account save confirms success.
- PASS: academic attempt to open company opportunities redirects to academic dashboard.
- PASS: Wessam academic evaluation modal opens and Submit result is disabled until company evaluation is received; no actual final grades changed.
- LIMIT QA-16: neither student Progress nor academic StudentProgress exposes training-hour submission/approval controls in the inspected interfaces. They display totals only. Find intended entry/review route before claiming full hours workflow works.

- CORRECTION: early dashboard count snapshots were captured during 1.25-second count-up animations, not final values. Removed premature metric mismatch findings QA-07/11/15. Settled academic dashboard shows 3 assigned students, 3 placements, 1 evaluation ready, consistent with its detail pages. Recheck student/company settled values before concluding.
## Critical deployed Firebase finding

- FAIL QA-17 / P0: unauthenticated REST GETs to users.json, students.json, placements.json, and contactMessages.json all return HTTP 200. No auth token supplied. Deployed Firebase rules allow access to private application records directly. Website route redirects do not protect these endpoints. Only status codes/counts recorded; record contents/tokens were not printed or saved.
- FAIL QA-18 / P0: authenticated student can also GET the entire users collection (19 records), entire students collection (10 records), another demo student's record, and contactMessages collection (7 records), all HTTP 200.
- Write access was deliberately not probed on original records; public write protection remains unverified. No rules deployed by QA.
- DATA QA-08 resolved: direct read of authenticated Wessam students/{UID} has none of the expected academic metadata fields populated. The empty profile reflects the current UID record; a legacy Wessam Ali row appears elsewhere. Do not copy national identifiers between accounts without confirming ownership.

## Public and notification tests

- PASS: Contact Us empty validation; valid QA message saves with success response.
- PASS: second QA contact message sent while admin dashboard remained open; badge changed from 1 to 2 without reload. Both subject lines appear in the panel.
- PASS: notification deep link reaches inbox and marks message read; badge decreases. Archive persists through reload. Both QA messages retained under Archived (2); original inbox remains 5 messages. No emails sent and nothing permanently deleted.
- FAIL QA-19 (medium): notification deep link is removed from address bar but remains in form action (./ContactMessages?message=<old ID>). Subsequent POSTs re-consume the old deep link: Mark unread immediately becomes read again, and opening another unread message initially returns reader to the old message. A second click on the now-read message selects it correctly.
- PASS: homepage mobile navigation and opportunity dialog open; no broken images or page-level horizontal overflow at 390px. About Us mobile and official embedded tour load.
- DATA/PRODUCT QA-20: homepage public opportunity summaries read assets/home-opportunities.json (11 static examples), not live Firebase (3 authenticated opportunities). Public majors list six fields; database currently offers CIS, CS and SE. Needs explicit sample labeling or backend connection.
- NOTE: official faculty tour has its own Arabic controls; surrounding About Us content is English.
- FAIL QA-21 (medium): coordinator dashboard footer "Review assignments" still opens AssignAcademicSupervisors, which displays only "Page layout placeholder. Add controls and events here." The pending Supervisor assignments link correctly opens Manage Students.

## Additional coverage and state

- All four supplied accounts logged in and logged out successfully; no password was changed.
- Settled company dashboard: 1 active opportunity, 4 trainees, 5 open tasks, 2 evaluations due. Earlier count mismatch was animation, not a defect. Applications metric settles at 3 while Applications Review lists 1 for the company; investigate different company-versus-opportunity filtering.
- Academic dashboard phone layout and navigation drawer tested; public Home/About phone layouts have no page horizontal overflow. Existing preview widths also included ~878px and full desktop. Not every page/breakpoint combination is yet verified.
- QA student CV: invalid PDF and oversized PDF rejected; valid synthetic PDF uploaded and remained linked after reload. Downloaded PDF SHA-256 exactly matches the uploaded fixture. No original student CV replaced.
- QA student created: qa.student.20261022@example.com, student number 9900221022, synthetic national ID 0000000000, GPA 80.25, credits 90, assigned omar. User completed both password creation and first-login password change. No password recorded in report.
- No application source fixes, commits, pushes, Firebase rule deployments, original placement decisions, final grades, or original-account password changes made during audit. User changed only the new QA account password.
- Initial QA changes: temporary skill marker added then removed (original list restored); two QA contact messages saved and archived. Later QA student/profile/CV/application/offer changes are listed below. Company QA opportunity save failed.
- Registration, first-password setup, academic metadata, application, offer and confirmation subsequently passed as documented below. Rejection/duplicate-race cases remain unverified. New-task lifecycle is blocked by missing placement creation; hours/final evaluation/completion have separate blockers.
## Dedicated QA student — registration verified

- PASS: user completed password entry and Register student; success notice and QA row appeared in admin roster.
- QA account email: qa.student.20261022@example.com. Password intentionally omitted from report.
- PASS: direct authenticated Firebase read of this account confirms englishName, studentNumber (including string storage), synthetic nationalId with leading zeros, gender, nationality, birthplace, DOB, academic status, expected graduation, GPA=80.25, completedCredits=90, completedCreditHours=90, assigned supervisor, user role=student and matching Authentication UID.
- PASS: first login with temporary password forces ChangePassword. Direct request to Profile cannot bypass it and redirects back to ChangePassword.
- PASS: user completed first-login password change; subsequent agent login with the new existing credential reaches the student dashboard. No password change submitted by agent.

## New-student application roundtrip

- PASS: all synthetic academic registration values display in the QA student's Academic record, including leading-zero national ID, birth date, GPA, credits and supervisor.
- PASS: QA-only phone and portfolio edits save, survive reload, and remain associated with the QA account.
- PASS: fake PDF and oversized PDF rejected; valid QA PDF saved under the student's profile and remains linked after reload.
- PASS: empty company-question answers prevent application submission and focus the first required answer.
- PASS: completed QA application saves, redirects to My Applications, survives reload, and appears in Orange's dashboard and Applications Review with both answers intact.
- PASS: Orange accepts the QA application; success message appears, company status becomes Accepted, and the student's application reads Offer Received after logging back in.
- FAIL QA-22 (high): company review says No CV attached although a valid CV was uploaded before applying. Profile uploads write studentCvFiles/{UID}, while opportunity context/application review uses the separate users.cvLink field; uploaded file is not integrated into application review.
- PASS: student offer confirmation subsequently persisted; My Applications shows Accepted and opportunities block additional applications. Native confirmation initially stalled automation; this is not classified as a website failure.
- Additional QA database changes: one QA student account/profile/CV, one QA application and one offer. Original placements/tasks/evaluations were not changed.

## Placement blocker and remaining limits

- FAIL QA-23 (high): a new student confirms an offer, but no placement is created. QA My Applications shows Accepted; student Dashboard still says No active training / No confirmed placement. Orange My Trainees remains four original trainees, and task selector omits the QA student.
- Direct authenticated Firebase verification: acceptedApplicationId exists, zero placements reference QA UID, and student has neither placementId nor companyId. No real record contents or token logged.
- Source confirmation: MyApplications.aspx.cs AcceptApplication_Click writes acceptedApplicationId and application status only. StudentService.SetAcceptedApplicationAsync updates only acceptedApplicationId; no placement creation is called in this path.
- Consequently, new QA task assignment/submission/revision/approval, placement hour progression and final evaluations cannot be honestly passed end-to-end. No manual database placement was inserted to bypass this failure.
- Other uncovered cases: network interruption/retry races, simultaneous acceptance, expired sessions/tokens, inactive-account transitions, social-login providers, actual password-recovery email delivery, every responsive breakpoint/browser combination and destructive operations. These remain unverified.
- No application source fixes or production-rule changes were made. Report is local and untracked. No commits/pushes made.

