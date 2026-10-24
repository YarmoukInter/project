'use strict';

const path = require('node:path');
const { initializeApp, cert, applicationDefault, deleteApp } = require('firebase-admin/app');
const { getAuth } = require('firebase-admin/auth');
const { getDatabase } = require('firebase-admin/database');

const credentialPath = process.env.YARMOUK_FIREBASE_SERVICE_ACCOUNT || process.env.GOOGLE_APPLICATION_CREDENTIALS;

initializeApp({
  credential: credentialPath ? cert(require(path.resolve(credentialPath))) : applicationDefault(),
  databaseURL: process.env.YARMOUK_FIREBASE_DATABASE_URL || 'https://yarmouk-uims-default-rtdb.firebaseio.com'
});

const auth = getAuth();
const db = getDatabase();
const studentCollections = [
  'applications', 'placements', 'tasks', 'taskSubmissions',
  'trainingLogs', 'evaluations'
];
const normalizeRole = value => ({ c_hr: 'company', uni_sp: 'academic' }[String(value || '').toLowerCase()]
  || String(value || '').toLowerCase());

async function readInput() {
  let input = '';
  for await (const chunk of process.stdin) input += chunk;
  return JSON.parse(input || '{}');
}

async function requireRoleUser(uid, expectedRole) {
  if (!/^[A-Za-z0-9_-]{1,128}$/.test(uid || '')) throw new Error('Invalid account UID.');
  const role = normalizeRole(expectedRole);
  if (!['student', 'academic', 'company'].includes(role)) throw new Error('Unsupported account role.');
  const snapshot = await db.ref(`users/${uid}`).get();
  const profile = snapshot.val();
  if (!profile || normalizeRole(profile.role) !== role)
    throw new Error(`The selected account is not a ${role}.`);
  return profile;
}

async function updateCredentials(request) {
  const role = normalizeRole(request.role || 'student');
  const profile = await requireRoleUser(request.uid, role);
  const email = String(request.email || '').trim();
  const password = String(request.password || '');
  if (!email && !password) throw new Error('Enter a new email or password.');
  const changes = {};
  if (email) changes.email = email;
  if (password) {
    if (password.length < 8 || password.length > 128) throw new Error('Password must be between 8 and 128 characters.');
    changes.password = password;
  }
  await auth.updateUser(request.uid, changes);
  const databaseChanges = {};
  if (email && email !== profile.email) databaseChanges[`users/${request.uid}/email`] = email;
  if (password) databaseChanges[`users/${request.uid}/mustChangePassword`] = true;
  if (Object.keys(databaseChanges).length) await db.ref().update(databaseChanges);
  return { message: password
    ? 'Student sign-in details updated. The student must change the new temporary password at next sign-in.'
    : 'Student sign-in email updated.' };
}

async function deleteStudent(request) {
  await requireRoleUser(request.uid, 'student');
  const root = (await db.ref().get()).val() || {};
  const updates = { [`users/${request.uid}`]: null, [`students/${request.uid}`]: null,
    [`studentCvFiles/${request.uid}`]: null, [`studentProgressNotes/${request.uid}`]: null };
  for (const collection of studentCollections) {
    for (const [key, value] of Object.entries(root[collection] || {})) {
      if (key === request.uid || (value && String(value.studentUid || '') === request.uid))
        updates[`${collection}/${key}`] = null;
    }
  }
  // Remove the database profile first. If the Authentication deletion then fails,
  // restore the profile so the coordinator can retry without leaving a ghost record.
  const studentProfile = root.users?.[request.uid];
  const studentData = root.students?.[request.uid];
  const cvData = root.studentCvFiles?.[request.uid];
  const notesData = root.studentProgressNotes?.[request.uid];
  await db.ref().update(updates);
  try {
    await auth.deleteUser(request.uid);
  } catch (error) {
    const restore = { [`users/${request.uid}`]: studentProfile };
    if (studentData !== undefined) restore[`students/${request.uid}`] = studentData;
    if (cvData !== undefined) restore[`studentCvFiles/${request.uid}`] = cvData;
    if (notesData !== undefined) restore[`studentProgressNotes/${request.uid}`] = notesData;
    await db.ref().update(restore);
    throw error;
  }
  return { message: 'Student Authentication account and related database records deleted.' };
}

(async () => {
  const request = await readInput();
  const result = request.action === 'health' ? { message: `Firebase Admin connected; users readable: ${(await db.ref('users').limitToFirst(1).get()).exists()}` }
    : request.action === 'credentials' ? await updateCredentials(request)
    : request.action === 'deleteStudent' ? await deleteStudent(request)
    : (() => { throw new Error('Unknown admin operation.'); })();
  process.stdout.write(JSON.stringify({ ok: true, ...result }));
})().catch(error => {
  process.stdout.write(JSON.stringify({ ok: false, error: error.message || 'Admin operation failed.' }));
  process.exitCode = 1;
}).finally(() => deleteApp(db.app));
