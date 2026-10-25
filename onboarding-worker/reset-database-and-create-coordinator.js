'use strict';

// Explicit maintenance utility: clear the UIMS Firebase project and leave one
// operational coordinator account. Run only when resetting a non-production
// project is intentional.
const path = require('node:path');
const { initializeApp, cert, deleteApp } = require('firebase-admin/app');
const { getAuth } = require('firebase-admin/auth');
const { getDatabase } = require('firebase-admin/database');

const email = 'operations.coordinator@yarmouk.edu.jo';
const temporaryPassword = 'YarmoukOps!2026';
const serviceAccountPath = process.env.YARMOUK_FIREBASE_SERVICE_ACCOUNT
  || path.resolve(__dirname, '..', 'yarmouk-uims-firebase-adminsdk-fbsvc-a3b29f5bc6.json');

const app = initializeApp({
  credential: cert(require(serviceAccountPath)),
  databaseURL: process.env.YARMOUK_FIREBASE_DATABASE_URL || 'https://yarmouk-uims-default-rtdb.firebaseio.com'
});
const auth = getAuth(app);
const db = getDatabase(app);

async function deleteAllAuthUsers() {
  let pageToken;
  do {
    const page = await auth.listUsers(1000, pageToken);
    if (page.users.length) await auth.deleteUsers(page.users.map(user => user.uid));
    pageToken = page.pageToken;
  } while (pageToken);
}

async function main() {
  await db.ref().remove();
  await deleteAllAuthUsers();

  const user = await auth.createUser({
    email,
    password: temporaryPassword,
    displayName: 'Operations Coordinator',
    disabled: false
  });
  const createdAtUnixMs = Date.now();
  await db.ref(`users/${user.uid}`).set({
    uid: user.uid,
    email,
    fullName: 'Operations Coordinator',
    englishName: 'Operations Coordinator',
    role: 'admin',
    isActive: true,
    mustChangePassword: true,
    createdAtUnixMs
  });

  process.stdout.write(JSON.stringify({ ok: true, email, uid: user.uid }));
}

main()
  .catch(error => { process.stderr.write(error.stack || error.message); process.exitCode = 1; })
  .finally(() => deleteApp(app));
