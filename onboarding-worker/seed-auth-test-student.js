'use strict';

// A repeatable student account for testing login and permanent deletion from
// CoordinatorPages/ManageStudents.
const path = require('node:path');
const { initializeApp, cert, deleteApp } = require('firebase-admin/app');
const { getAuth } = require('firebase-admin/auth');
const { getDatabase } = require('firebase-admin/database');

const email = 'support@divbux.com';
const password = 'DivbuxAuth!2026';
const app = initializeApp({
  credential: cert(require(process.env.YARMOUK_FIREBASE_SERVICE_ACCOUNT || path.resolve(__dirname, '..', 'yarmouk-uims-firebase-adminsdk-fbsvc-a3b29f5bc6.json'))),
  databaseURL: process.env.YARMOUK_FIREBASE_DATABASE_URL || 'https://yarmouk-uims-default-rtdb.firebaseio.com'
});
const auth = getAuth(app);
const db = getDatabase(app);

async function main() {
  let user;
  try {
    user = await auth.getUserByEmail(email);
    await auth.updateUser(user.uid, { displayName: 'Auth Test', password, disabled: false });
  } catch (error) {
    if (error.code !== 'auth/user-not-found') throw error;
    user = await auth.createUser({ email, password, displayName: 'Auth Test', disabled: false });
  }
  const now = Date.now();
  await db.ref().update({
    [`users/${user.uid}`]: {
      uid: user.uid, email, fullName: 'Auth Test', englishName: 'Auth Test', role: 'student',
      isActive: true, mustChangePassword: true, createdAtUnixMs: now
    },
    [`students/${user.uid}`]: {
      uid: user.uid, studentId: 'AUTH-TEST-2026', studentNumber: 'AUTH-TEST-2026',
      englishName: 'Auth Test', majorId: 'CIS', gpa: '75.00', completedCredits: 0,
      academicSupervisorId: '', academicSupervisorName: ''
    }
  });
  process.stdout.write(JSON.stringify({ ok: true, email, uid: user.uid }));
}

main().catch(error => { process.stderr.write(error.stack || error.message); process.exitCode = 1; })
  .finally(() => deleteApp(app));
