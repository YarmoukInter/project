'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const api = require('./firebase-api');

test('requests Firebase to send a password reset link', async () => {
  let request;
  await api.sendPasswordSetupEmail('new@example.com', async (url, options) => {
    request = { url, options };
    return { ok: true, json: async () => ({ email: 'new@example.com' }) };
  });
  assert.match(request.url, /accounts:sendOobCode\?key=/);
  assert.deepEqual(JSON.parse(request.options.body), { requestType: 'PASSWORD_RESET', email: 'new@example.com' });
});

test('a database-only profile creates an Authentication account', async () => {
  const uid = await api.createAuthAccount('new@example.com', 'random-pass', async (url, options) => {
    assert.match(url, /accounts:signUp\?key=/);
    assert.equal(JSON.parse(options.body).email, 'new@example.com');
    return { ok: true, json: async () => ({ localId: 'firebase-uid' }) };
  });
  assert.equal(uid, 'firebase-uid');
});

test('an existing Authentication account is reused', async () => {
  const uid = await api.createAuthAccount('existing@example.com', 'random-pass', async () => ({
    ok: false, status: 400, json: async () => ({ error: { message: 'EMAIL_EXISTS' } })
  }));
  assert.equal(uid, null);
});
