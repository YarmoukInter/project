'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');

test('first evaluation load posts to the canonical response URL and preserves one form state', async () => {
  const events = {};
  let originalRemoved = false;
  const oldFields = [{ name: '__VIEWSTATE', remove() { this.removed = true; } },
    { name: '__VIEWSTATE9', remove() { this.removed = true; } }];
  const copied = [];
  const currentForm = {
    action: '/CompanyPages/FinalEvaluation',
    querySelectorAll(selector) { return selector === 'input[type="hidden"]' ? oldFields : []; },
    appendChild(field) { copied.push(field); }
  };
  const newFields = ['__VIEWSTATE', '__VIEWSTATE1', '__VIEWSTATEFIELDCOUNT', '__EVENTVALIDATION']
    .map(name => ({ name, value: 'new-' + name, cloneNode() { return { name: this.name, value: this.value }; } }));
  const workspace = { querySelector(selector) { return selector === '.evaluation-workspace' ? {} : null; } };
  const parsed = { forms: [{ querySelectorAll() { return newFields; } }], querySelector() { return workspace; } };
  let inserted;
  const host = { innerHTML: '', replaceChildren(node) { inserted = node; } };
  const dialog = {
    open: false, showModal() { this.open = true; }, close() { this.open = false; },
    addEventListener() {}, querySelector() { return null; }
  };
  const document = {
    forms: [currentForm], documentElement: { classList: { add() {} } },
    getElementById(id) { return id === 'evaluationDialog' ? dialog : host; },
    querySelectorAll() { return []; },
    querySelector(selector) { return selector === '.evaluation-layout > .evaluation-main' ? { remove() { originalRemoved = true; } } : null; },
    addEventListener(name, handler) { (events[name] ||= []).push(handler); }
  };
  const canonical = 'http://localhost/CompanyPages/FinalEvaluation?placementId=placement';
  const context = {
    document, URL, URLSearchParams, Set, location: { href: 'http://localhost/CompanyPages/FinalEvaluation', search: '' },
    window: {}, DOMParser: class { parseFromString() { return parsed; } },
    fetch: async () => ({ ok: true, url: canonical, text: async () => '<evaluation form>' })
  };
  vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../Yarmouk/Yarmouk/Scripts/CompanyPages/FinalEvaluation.js'), 'utf8'), context);
  const link = { href: 'http://localhost/CompanyPages/FinalEvaluation.aspx?placementId=placement' };
  events.click[0]({ target: { closest(selector) { return selector === '[data-evaluation-open]' ? link : null; } }, preventDefault() {} });
  await new Promise(resolve => setImmediate(resolve));
  assert.equal(currentForm.action, canonical);
  assert.equal(inserted, workspace);
  assert.equal(originalRemoved, true, 'old controls must not duplicate imported IDs or labels');
  assert.ok(oldFields.every(field => field.removed), 'old/chunked ViewState is removed');
  assert.equal(copied.length, 4);
  assert.equal(copied.find(field => field.name === '__VIEWSTATEFIELDCOUNT').value, 'new-__VIEWSTATEFIELDCOUNT');
});
