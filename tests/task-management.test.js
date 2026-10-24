'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');

function fixture(confirmResult = false) {
  const events = {}, formEvents = {}, nodes = {};
  const form = { addEventListener(name, fn) { formEvents[name] = fn; } };
  const fields = ['id', 'number', 'title', 'deadline', 'description', 'url'].map(name => {
    const field = { value: '', setCustomValidity() {}, focus() {}, reportValidity() { return true; } };
    nodes['edit-task-' + name] = field;
    return field;
  });
  nodes['edit-task-fields'] = { disabled: true };
  nodes['edit-task-modal'] = {
    hidden: true, closest() { return form; }, addEventListener() {}, querySelectorAll() { return fields.slice(1); }
  };
  const document = {
    getElementById(id) { return nodes[id] || null; },
    addEventListener(name, fn) { (events[name] ||= []).push(fn); },
    querySelectorAll() { return []; }, contains() { return true; },
    body: { classList: { add() {}, remove() {} } }
  };
  vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../Yarmouk/Yarmouk/Scripts/CompanyPages/ManageTasks.js'), 'utf8'), {
    document, window: { confirm() { return confirmResult; } }, Date
  });
  function click(matches) {
    let prevented = false;
    const event = { target: { closest(selector) { return matches[selector] || null; } }, preventDefault() { prevented = true; } };
    events.click.forEach(fn => fn(event));
    return prevented;
  }
  return { nodes, click, formEvents };
}

test('delete requires confirmation before allowing submission', () => {
  assert.equal(fixture(false).click({ '[data-task-delete]': {} }), true);
  assert.equal(fixture(true).click({ '[data-task-delete]': {} }), false);
});

test('editor populates the selected task and disables hidden fields again on cancel', () => {
  const f = fixture();
  f.click({ '[data-task-edit]': { focus() {}, dataset: { taskId: 'database-key', taskNumber: 'TASK008', taskTitle: 'Restaurant system',
    taskDeadline: '2099-11-26', taskDescription: 'Assignment instructions', taskUrl: 'https://example.com/task.pdf' } } });
  assert.equal(f.nodes['edit-task-modal'].hidden, false);
  assert.equal(f.nodes['edit-task-fields'].disabled, false);
  assert.equal(f.nodes['edit-task-id'].value, 'database-key');
  assert.equal(f.nodes['edit-task-title'].value, 'Restaurant system');
  f.click({ '[data-close-task-edit]': {} });
  assert.equal(f.nodes['edit-task-modal'].hidden, true);
  assert.equal(f.nodes['edit-task-fields'].disabled, true, 'hidden required controls cannot block task creation');
});

test('invalid edit stops submission without losing the entered values', () => {
  const f = fixture();
  f.nodes['edit-task-title'].value = 'Keep this title';
  f.nodes['edit-task-number'].reportValidity = () => false;
  let prevented = false;
  f.formEvents.submit({ submitter: { name: 'taskManagementAction', value: 'edit' }, preventDefault() { prevented = true; } });
  assert.equal(prevented, true);
  assert.equal(f.nodes['edit-task-title'].value, 'Keep this title');
});
