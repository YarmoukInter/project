'use strict';

const databaseUrl = 'https://yarmouk-uims-default-rtdb.firebaseio.com';
const apiKey = 'AIzaSyC-rMThE_iWhQbmvuuRFL2FqhHgTXuAR9U';

async function request(url, options = {}, fetchRequest = fetch) {
  const response = await fetchRequest(url, { ...options, signal: AbortSignal.timeout(20000) });
  const body = await response.json().catch(() => ({}));
  if (!response.ok) {
    const error = new Error(body.error?.message || body.error || `HTTP ${response.status}`);
    error.code = body.error?.message || String(response.status);
    throw error;
  }
  return body;
}

const authUrl = action => `https://identitytoolkit.googleapis.com/v1/accounts:${action}?key=${encodeURIComponent(apiKey)}`;
const dataUrl = path => `${databaseUrl}/${path.replace(/^\/+/, '')}.json`;
const jsonPost = value => ({ method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(value) });

async function createAuthAccount(email, password, fetchRequest = fetch) {
  try {
    const response = await request(authUrl('signUp'), jsonPost({ email, password, returnSecureToken: true }), fetchRequest);
    if (!response.localId) throw new Error('Firebase did not return the new Authentication UID.');
    return response.localId;
  } catch (error) {
    if (error.code === 'EMAIL_EXISTS') return null;
    throw error;
  }
}

async function sendPasswordSetupEmail(email, fetchRequest = fetch) {
  return request(authUrl('sendOobCode'), jsonPost({ requestType: 'PASSWORD_RESET', email }), fetchRequest);
}

async function read(path, fetchRequest = fetch) {
  return request(dataUrl(path), {}, fetchRequest);
}

async function patchRoot(updates, fetchRequest = fetch) {
  return request(dataUrl(''), { method: 'PATCH', headers: { 'content-type': 'application/json' }, body: JSON.stringify(updates) }, fetchRequest);
}

module.exports = { createAuthAccount, sendPasswordSetupEmail, read, patchRoot };
