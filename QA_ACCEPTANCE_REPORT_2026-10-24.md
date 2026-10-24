# QA acceptance report — 24 October 2026

## Test fixture

The Firebase fixture `full-qa-journey-20261024` contains ten isolated QA accounts, two active opportunities, seven application scenarios, placements, task submissions, final evaluations, and a certificate-ready completion record.

| Scenario | Result |
| --- | --- |
| Account/profile identity match | 10 of 10 accounts verified |
| Company and HR linkage | Verified against the approved QA company record |
| Opportunity creation/listing | 2 active opportunities verified |
| Application decisions | New, offered, rejected, and accepted states verified |
| Task-gated company evaluation | Student 4 appears after 10 of 10 completed tasks; Student 7 is blocked at 9 of 10 |
| Company and academic evaluations | Submitted for Student 4 |
| Coordinator completion | Approved with a final result of 91 / 100 |
| Certificate availability | Enabled for Student 4 |

## Browser checks

Using the QA HR account, the company workspace showed both opportunities, seven applications with the expected decision counts, and exactly one eligible final-evaluation trainee: **Wessam Khraisat Student 4**. The incomplete placement did not appear in the final-evaluation queue.

## Automated checks

- `tests/Run-WorkflowRegressionTests.ps1` — passed
- `tests/Run-CompanyEvaluationPdfTests.ps1` — passed
- `npm --prefix onboarding-worker test` — 4 tests passed
- `node onboarding-worker/verify-full-qa-journey.js` — passed
- Debug MSBuild build — passed

## Test accounts

Students: `wessam.khraisat.student1@yarmouk.com` through `wessam.khraisat.student7@yarmouk.com`.

Company: `wessam.khraisat.company@yarmouk.com`.

HR: `wessam.khraisat.hr@yarmouk.com`.

Academic QA: `wessam.khraisat.academic.qa@yarmouk.com`.

All QA accounts use the shared temporary test password established by the fixture. The fixture script intentionally refuses to overwrite accounts that are not marked as QA data.
