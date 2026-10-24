# UIMS — University Internship Management System

UIMS is a field-training and academic-assessment platform for the Faculty of Information Technology and Computer Science at Yarmouk University. It brings students, training providers, academic supervisors, and university coordinators into one guided workflow for internships.

The application is built with ASP.NET Web Forms and C#, with Firebase Authentication and Firebase Realtime Database for identity and data services.

## Product links

- [Trello board — UIMS Group 5](https://trello.com/b/6pw4IcSu/uims-group-5-yarmouk-university)
- [Figma design — demo link](https://www.figma.com/design/UIMS-Group-5-Yarmouk-UIMS-Demo) *(placeholder link; replace it with the team’s published Figma file when available)*

## What the site does

UIMS supports the complete field-training journey:

- Students explore opportunities, apply, confirm an accepted placement, submit tasks, and monitor their training progress.
- Training providers manage opportunities, review applications, assign and approve tasks, and submit the company’s final evaluation.
- Academic supervisors follow assigned students, review academic progress and training hours, and complete the academic assessment.
- Coordinators manage accounts, companies, supervisors, placements, opportunities, and final completion decisions.

## Training lifecycle

| Cycle | Main activities | Main roles |
| --- | --- | --- |
| 1. Opportunity and application | A company publishes an opportunity. Students browse available roles and submit an application. | Student, training provider |
| 2. Selection and placement | The company accepts or rejects applications. The student confirms one accepted offer, creating the training placement. | Student, training provider, coordinator |
| 3. Training and task delivery | The provider assigns work. The student submits task evidence, then the provider reviews and marks the task completed. | Student, training provider |
| 4. Academic follow-up | The academic supervisor monitors the student’s progress and approves training hours independently. | Academic supervisor, student |
| 5. Final evaluation | After all assigned tasks are completed, the provider fills in the company evaluation. The academic supervisor completes the academic evaluation. | Training provider, academic supervisor |
| 6. Completion review | The coordinator reviews the evidence, evaluations, approved hours, and placement status before recording the completion decision. | Coordinator |

## Technical overview

```text
Browser
  → ASPX page and JavaScript
  → ASPX code-behind and domain services
  → Firebase helper
  → Firebase Authentication + Realtime Database
```

The role-based workspaces are organized under these folders:

- `StudentPages/` — student workflow
- `CompanyPages/` — training-provider workflow
- `AcademicPages/` — academic-supervisor workflow
- `CoordinatorPages/` — coordinator administration
- `PublicPages/` — public university pages and sign-in

## Run locally

1. Open `Yarmouk/Yarmouk.sln` in Visual Studio.
2. Restore NuGet packages if Visual Studio requests it.
3. Configure Firebase using the project’s existing setup notes.
4. Start the ASP.NET project with IIS Express.
5. Open the local site, normally at `http://localhost:51001/`.

For account-onboarding setup, see [firebase/ONBOARDING.md](firebase/ONBOARDING.md). For application architecture and developer conventions, see [Yarmouk/Yarmouk/TEAM_DEVELOPER_GUIDE.md](Yarmouk/Yarmouk/TEAM_DEVELOPER_GUIDE.md).

## Development team

| Team member | Role | Focus |
| --- | --- | --- |
| Wissam Khreisat | Scrum Master | Sprint coordination, integration, and blocker removal |
| Amneh Al-Hazaimeh | Product Owner | Requirements, product backlog, and user journeys |
| Ghaith Ammourah | Quality Assurance Engineer | Functional testing, permissions review, and data verification |
| Sief Alkhatatbeh | Full-Stack Developer | Interfaces, ASP.NET C# features, Firebase integration |
| Dana Al-Khatib | Full-Stack Developer | Role-based interfaces, applications, placements, and progress tracking |
| Haya Ahmad | Full-Stack Developer | Opportunities, task management, and evaluation workflows |
| Amani Alazzam | Full-Stack Developer | Authentication, permissions, responsive Arabic interfaces, and cloud data modeling |

## Project workflow

The team uses iterative development cycles:

1. Plan the cycle and refine its backlog items in Trello.
2. Design the workflow and interface in Figma.
3. Implement the feature in a role-specific workspace.
4. Test business rules, permissions, and data flow.
5. Review the result as a team before merging it into the shared codebase.

## Contribution notes

- Keep page structure in `.aspx`, server logic in `.aspx.cs`, and client styling/behavior in the related `Content/` and `Scripts/` files.
- Do not expose Firebase identity tokens in browser code or logs.
- Verify builds and role permissions before merging changes.
- Keep changes focused on the relevant workspace and workflow cycle.
