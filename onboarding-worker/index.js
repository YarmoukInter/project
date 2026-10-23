'use strict';

const fs = require('node:fs/promises');
const path = require('node:path');
const { randomBytes } = require('node:crypto');
const firebase = require('./firebase-api');

const stateFile = path.join(__dirname, 'state.json');
const intervalMs = 30000;
const validRoles = new Set(['student', 'academic', 'company', 'coordinator', 'uni_sp', 'c_hr', 'admin']);
let state = null;
let busy = false;

async function saveState() {
  const temporary = `${stateFile}.tmp`;
  await fs.writeFile(temporary, JSON.stringify(state, null, 2), 'utf8');
  await fs.rename(temporary, stateFile);
}

async function moveProfile(oldUid, newUid, profile, api = firebase) {
  if (oldUid === newUid) return;
  const existing = await api.read(`users/${newUid}`);
  if (existing) throw new Error(`Cannot move user ${oldUid}: target UID already has a profile.`);
  const updates = {
    [`users/${newUid}`]: { ...profile, uid: newUid, mustChangePassword: true, isActive: profile.isActive ?? true },
    [`users/${oldUid}`]: null
  };
  if (profile.role === 'student') {
    const student = await api.read(`students/${oldUid}`);
    if (student) {
      updates[`students/${newUid}`] = { ...student, uid: newUid };
      updates[`students/${oldUid}`] = null;
    }
  }
  if (profile.role === 'company' || profile.role === 'c_hr') {
    const companies = await api.read('companies') || {};
    for (const [companyId, company] of Object.entries(companies)) {
      if (company?.supervisorUid === oldUid) {
        updates[`companies/${companyId}/supervisorUid`] = newUid;
      }
    }
  }
  await api.patchRoot(updates);
}

async function invite(uid, profile) {
  const email = String(profile?.email || '').trim();
  const role = String(profile?.role || '').trim().toLowerCase();
  if (!email || !validRoles.has(role)) {
    console.warn(`Skipping ${uid}: add a valid email and role to /users/${uid}.`);
    return;
  }

  let authUid = state.pending[uid];
  if (!authUid) {
    const randomPassword = randomBytes(32).toString('base64url') + 'aA1!';
    authUid = await firebase.createAuthAccount(email, randomPassword) || uid;
    state.pending[uid] = authUid;
    await saveState();
  }
  if (authUid !== uid) await moveProfile(uid, authUid, profile);
  else await firebase.patchRoot({ [`users/${uid}/uid`]: uid, [`users/${uid}/mustChangePassword`]: true,
    ...(profile.isActive == null ? { [`users/${uid}/isActive`]: true } : {}) });

  await firebase.sendPasswordSetupEmail(email);
  state.seen[uid] = { status: 'sent', authUid, at: new Date().toISOString() };
  state.seen[authUid] = { status: 'sent', authUid, at: new Date().toISOString() };
  delete state.pending[uid];
  await saveState();
  console.log(`Firebase accepted a password setup email request for UID ${authUid}.`);
}

async function poll() {
  if (busy) return;
  busy = true;
  try {
    const users = await firebase.read('users') || {};
    if (!state) {
      try { state = JSON.parse(await fs.readFile(stateFile, 'utf8')); }
      catch (error) {
        if (error.code !== 'ENOENT') throw error;
        state = { seen: {}, pending: {} };
        for (const [uid, profile] of Object.entries(users)) {
          if (!profile?.onboardingRequestedAtUnixMs) state.seen[uid] = { status: 'existing' };
        }
        await saveState();
        console.log(`First run: recorded existing users. Pending coordinator registrations will receive setup emails.`);
      }
    }
    state.seen ||= {};
    state.pending ||= {};
    for (const [uid, profile] of Object.entries(users)) {
      if (state.seen[uid]) continue;
      try { await invite(uid, profile); }
      catch (error) { console.error(`Onboarding failed for UID ${uid}: ${error.message}`); }
    }
  } catch (error) {
    console.error(`Cannot check Firebase users: ${error.message}`);
  } finally { busy = false; }
}

if (require.main === module) {
  poll();
  setInterval(poll, intervalMs);
}

module.exports = { moveProfile };
