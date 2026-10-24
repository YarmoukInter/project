# Server configuration and migration

## Firebase Admin account maintenance

Install Node.js 20+ and run `npm ci --prefix onboarding-worker --ignore-scripts`.
Deploy `onboarding-worker` beside the web application (the server searches parent directories).

Configure the environment **of the IIS/IIS Express process**, then restart it:

- `YARMOUK_NODE_PATH`: absolute path to `node.exe` if Node is not on the server PATH. Default: `node`.
- `YARMOUK_FIREBASE_SERVICE_ACCOUNT` or `GOOGLE_APPLICATION_CREDENTIALS`: absolute path to the authorized Firebase Admin JSON credential, stored outside the repository. The server identity must have read access. Alternatively use Application Default Credentials on the hosting platform.
- Optional `FirebaseAdminCredentialPath` in server-only appSettings supplies the same path.

The application passes its configured Firebase database URL to the worker. No checkout-specific Node path or fixed service-account filename is required. Credentials must have the project's existing Authentication and Realtime Database administration permissions. Do not commit credential JSON, print it, or put it in browser code.

No Admin credential was supplied in this checkout. Installing the runtime does not grant access; password/email changes and deletion require the hosting administrator to configure the credential. Verify with a disposable account after configuration, not an existing student's password.

## Persistent profile photos

Set `YARMOUK_PROFILE_IMAGE_DIRECTORY` to an absolute, durable directory outside the release checkout. The web-server identity needs read/write access; multiple instances must mount the same storage. New uploads use it and authenticated image requests are served through `StudentProfileImage.ashx`. During migration the application also reads legacy files under `Uploads/StudentProfileImages`. Missing files return an empty photo URL so the existing initials fallback remains visible.

Before switching releases, copy existing uploads from each old checkout:

```powershell
./firebase/Migrate-ProfileImages.ps1 -SourceDirectory 'C:\old-release\Yarmouk\Yarmouk\Uploads\StudentProfileImages' -DestinationDirectory 'D:\UIMS-data\StudentProfileImages'
```

The script preserves originals, skips identical existing images and stops on checksum conflicts. Back up the persistent directory separately from the source repository. A database filename alone cannot reconstruct a missing photo; restore it from the old checkout/backup or ask its owner to upload it again.

## Seat reservations

`availablePositions` is the total capacity, `filledPositions` is occupied capacity, and `seatReservations/{studentUid}` records the offer consuming a seat. The company list shows remaining capacity and also accounts for legacy accepted applications when the stored count is absent; the editor changes total capacity. Conditional writes are scoped to one opportunity and retry after ETag conflicts, using the [Firebase conditional-write protocol](https://firebase.google.com/docs/database/rest/save-data#section-conditional-requests). Editing an opportunity preserves its current reservations.

The subsequent placement/application/student save is atomic via a multi-path PATCH. If connectivity interrupts that save, retry confirmation using the same student and offer: it reuses the reservation. An uncertain reservation is deliberately retained rather than risking overbooking. Cancellation is not currently exposed by this workflow; any future cancellation must release its reservation with the same conditional-write discipline. This checkout does not automatically migrate existing confirmed placements; confirmation accounts for existing placement occupants when reserving a seat.

## Academic hours and progress

Academic → Student Progress → **Record approved hours** is the existing authorized hours workflow. Enter verified hours to add (not a replacement total). It creates an approved `trainingLogs` record with supervisor identity/time and updates student/placement totals, bounded by the required hours. Completion remains gated by the approved records and other completion requirements.

Task progress uses distinct submitted tasks out of ten. The meeting/completion workflow still checks approved task completion separately. Final scores use 40% company and 60% academic across the academic page, coordinator review and certificate.

## Arabic PDFs

The installed MigraDoc 1.50 renderer does not shape Arabic. Arabic runs now use Windows GDI+ shaping and bidirectional layout, embedded as high-resolution PNG runs in the PDF. English remains PDF text. Arabic is visually portable but these runs are not selectable/searchable text. Both evaluation and certificate fixtures were rendered and visually checked.
