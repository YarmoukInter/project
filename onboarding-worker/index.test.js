'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const { moveProfile } = require('./index');

test('moves a database-first student profile to the Authentication UID', async () => {
  let updates;
  await moveProfile('draft-key', 'auth-uid', { email: 'student@example.com', role: 'student' }, {
    read: async key => key === 'students/draft-key' ? { majorId: 'CS' } : null,
    patchRoot: async value => { updates = value; }
  });
  assert.equal(updates['users/auth-uid'].uid, 'auth-uid');
  assert.equal(updates['users/draft-key'], null);
  assert.equal(updates['students/auth-uid'].uid, 'auth-uid');
  assert.equal(updates['students/draft-key'], null);
});
