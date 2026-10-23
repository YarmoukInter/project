# Manage pages recovery

The active Manage Students, Manage Companies, and Manage Academic Supervisors files and their shared roster assets were recovered from Git tree 11bb94a0e0ca68d56c5447b1fe900c69cdd7706c. This tree includes file versions absent from surviving commits and stashes. Its date and whether it is the intended final unpushed version are unconfirmed. Files were extracted without rewriting their contents.

Only the three Manage pages and their roster dependencies were restored. Dashboard, Home, About Us, backend services, and configuration were not restored.

## Other candidates

- project-copy/: original page, code-behind, designer, CSS and JavaScript files copied from Downloads/project - Copy. File timestamps previously inspected were October 21, 2026.
- git-candidates/: alternate uncommitted Git blobs; filenames include their object ID prefixes.

These are recovery candidates for comparison, not a confirmed final version. The old roster expects user-supplied temporary passwords, while the current backend uses the email address as the initial password. Review that integration before merging into develop. This branch does not change develop.
