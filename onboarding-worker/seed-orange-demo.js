'use strict';

// Creates one clearly marked, repeatable Orange evaluation demo in Firebase.
// Run from this directory with: node seed-orange-demo.js
const { initializeApp, cert, deleteApp } = require('firebase-admin/app');
const { getAuth } = require('firebase-admin/auth');
const { getDatabase } = require('firebase-admin/database');
const path = require('node:path');

const marker = 'orange-evaluation-demo-20261024';
const email = 'orange.evaluation.demo@yarmouk.com';
const ids = {
  opportunity: 'ORANGE_EVAL_DEMO_20261024',
  application: 'ORANGE_EVAL_DEMO_20261024',
  placement: 'ORANGE_EVAL_DEMO_20261024',
  tasks: [
    'ORANGE_EVAL_DEMO_ANALYSIS_20261024',
    'ORANGE_EVAL_DEMO_IMPLEMENTATION_20261024',
    'ORANGE_EVAL_DEMO_TESTING_20261024',
    'ORANGE_EVAL_DEMO_FINAL_REPORT_20261024'
  ]
};

const app = initializeApp({
  credential: cert(require(path.resolve(__dirname, '..', 'yarmouk-uims-firebase-adminsdk-fbsvc-a3b29f5bc6.json'))),
  databaseURL: 'https://yarmouk-uims-default-rtdb.firebaseio.com'
});

const auth = getAuth(app);
const db = getDatabase(app);
const normalizeRole = value => ({ c_hr: 'company', uni_sp: 'academic', admin: 'coordinator' }[String(value || '').toLowerCase()]
  || String(value || '').toLowerCase());

async function getOrangeCompany() {
  let supervisor;
  try { supervisor = await auth.getUserByEmail('orange@yarmouk.com'); }
  catch { throw new Error('Orange supervisor account orange@yarmouk.com was not found.'); }
  const profile = (await db.ref(`users/${supervisor.uid}`).get()).val();
  if (!profile || normalizeRole(profile.role) !== 'company' || profile.isActive === false)
    throw new Error('Orange supervisor profile is missing, inactive, or has the wrong role.');

  const companyId = String(profile.companyId || process.env.ORANGE_COMPANY_ID || '');
  if (!companyId) throw new Error('Orange supervisor profile has no companyId. Set ORANGE_COMPANY_ID to the exact Orange company record ID.');
  const company = (await db.ref(`companies/${companyId}`).get()).val();
  const linked = company && (company.supervisorUid === supervisor.uid
    || profile.companyId === companyId
    || company.supervisorUids?.[supervisor.uid] === true);
  if (!companyId || !company || !linked) throw new Error('Orange company record is not linked to this supervisor account.');
  return { supervisor, profile, company, companyId };
}

async function getAcademicAssignment(companyId) {
  let trainees;
  try {
    trainees = (await db.ref('students').orderByChild('companyId').equalTo(companyId).get()).val() || {};
  } catch (error) {
    // The production database may not index companyId. The demo can still be
    // evaluated as a draft; final submission requires an assigned academic UID.
    if (!String(error.message || '').includes('Index not defined')) throw error;
    return null;
  }
  for (const student of Object.values(trainees)) {
    const uid = student?.academicSupervisorId;
    if (!uid) continue;
    const user = (await db.ref(`users/${uid}`).get()).val();
    if (user && normalizeRole(user.role) === 'academic' && user.isActive !== false)
      return { uid, name: user.fullName || 'Academic supervisor', majorId: student.majorId || 'CIS' };
  }
  return null;
}

async function existingAccountOrCreate() {
  try {
    const user = await auth.getUserByEmail(email);
    const profile = (await db.ref(`users/${user.uid}`).get()).val();
    if (profile?.demoSeedKey !== marker) throw new Error('The reserved demo email already belongs to another account.');
    return { uid: user.uid, created: false };
  } catch (error) {
    if (error.message === 'The reserved demo email already belongs to another account.') throw error;
    if (error.code && error.code !== 'auth/user-not-found') throw error;
    const user = await auth.createUser({ email, password: email, displayName: 'Lina Haddad (Orange Evaluation Demo)' });
    return { uid: user.uid, created: true };
  }
}

async function main() {
  const { supervisor, company, companyId } = await getOrangeCompany();
  const academic = await getAcademicAssignment(companyId);
  const { uid: studentUid, created } = await existingAccountOrCreate();

  const protectedPaths = [
    `students/${studentUid}`, `placements/${ids.placement}`, `applications/${ids.application}`,
    `opportunities/${ids.opportunity}`, ...ids.tasks.map(id => `tasks/${id}`),
    ...ids.tasks.map(id => `taskSubmissions/${id}_SUB_${studentUid}`)
  ];
  for (const recordPath of protectedPaths) {
    const value = (await db.ref(recordPath).get()).val();
    if (value && value.demoSeedKey !== marker) {
      if (created) await auth.deleteUser(studentUid);
      throw new Error(`Refusing to overwrite a record outside this demo: ${recordPath}`);
    }
  }

  const now = Date.now();
  const startDate = '2026-09-01';
  const endDate = '2026-12-15';
  const taskSpecs = [
    ['Analysis', 'Analyze Orange’s trainee onboarding workflow', 'Document the current steps, identify three friction points, and recommend measurable improvements.'],
    ['Implementation', 'Build an internship tracking dashboard prototype', 'Create a clear dashboard flow for trainee progress, deadlines, and supervisor feedback.'],
    ['Testing', 'Test the internship tracking workflow', 'Prepare test cases for onboarding, task submission, review, and final evaluation; record the results.'],
    ['Final report', 'Prepare the final internship report', 'Summarize the work completed, skills developed, outcomes, and recommendations for Orange.']
  ];
  const updates = {};
  const userRecord = {
    uid: studentUid, email, fullName: 'Lina Haddad (Orange Evaluation Demo)', role: 'student', isActive: true,
    mustChangePassword: true, demoSeedKey: marker, onboardingRequestedAtUnixMs: now
  };
  const student = {
    uid: studentUid, studentId: 'DEMO-ORANGE-20261024', studentNumber: 'DEMO-ORANGE-20261024',
    englishName: 'Lina Haddad (Orange Evaluation Demo)', majorId: academic?.majorId || 'CIS',
    academicSupervisorId: academic?.uid || '', academicSupervisorName: academic?.name || '',
    companyId, internshipStatus: 'InProgress', acceptedApplicationId: ids.application,
    requiredTrainingHours: 160, approvedTrainingHours: 120, trainingStartDate: startDate,
    completedCreditHours: 90, gpa: '89.00', demoSeedKey: marker
  };
  const opportunity = {
    id: ids.opportunity, companyId, title: 'Orange software engineering internship (demo)',
    description: 'Test placement created to try company reports and final evaluation.',
    eligibleMajorIds: [student.majorId], availablePositions: 1, filledPositions: 1,
    isActive: true, status: 'Open', deadline: endDate, requirements: 'Demo record for evaluation testing.',
    duration: 'September–December 2026', requiredTrainingHours: 160, workMode: 'On-site', demoSeedKey: marker
  };
  const application = {
    id: ids.application, studentUid, opportunityId: ids.opportunity, companyId,
    status: 'accepted', appliedAtUnixMs: now - 7 * 86400000, answers: [],
    isConfirmedPlacement: true, placementId: ids.placement, demoSeedKey: marker
  };
  const placement = {
    id: ids.placement, studentUid, studentId: student.studentId, companyId,
    opportunityId: ids.opportunity, applicationId: ids.application,
    academicSupervisorUid: academic?.uid || '', companySupervisorUid: supervisor.uid,
    status: 'InProgress', startDate, endDate, requiredHours: 160, createdAtUnixMs: now, demoSeedKey: marker
  };

  updates[`users/${studentUid}`] = userRecord;
  updates[`students/${studentUid}`] = student;
  updates[`opportunities/${ids.opportunity}`] = opportunity;
  updates[`applications/${ids.application}`] = application;
  updates[`placements/${ids.placement}`] = placement;
  taskSpecs.forEach(([number, title, description], index) => {
    const taskId = ids.tasks[index];
    updates[`tasks/${taskId}`] = {
      id: taskId, taskNumber: `DEMO-${number.toUpperCase()}`, title, description,
      studentUid, studentName: student.englishName, companyId, placementId: ids.placement,
      opportunityId: ids.opportunity, supervisorUid: supervisor.uid,
      dueAtUnixMs: now - (index + 1) * 86400000, createdAtUnixMs: now - (index + 5) * 86400000,
      status: 'completed', demoSeedKey: marker
    };
    updates[`taskSubmissions/${taskId}_SUB_${studentUid}`] = {
      id: `${taskId}_SUB_${studentUid}`, taskId, studentUid, placementId: ids.placement,
      notes: `Demo submission: completed the ${number.toLowerCase()} task. This text is sample content for reviewing the trainee’s final evaluation.`,
      status: 'Submitted', reviewStatus: 'Pending', submittedAtUnixMs: now - (index + 1) * 86400000,
      demoSeedKey: marker
    };
  });
  await db.ref().update(updates);
  process.stdout.write(JSON.stringify({
    ok: true, student: student.englishName, email, studentNumber: student.studentNumber,
    company: company.name || company.companyName || 'Orange', taskCount: taskSpecs.length,
    placementId: ids.placement, academicSupervisorAssigned: Boolean(academic?.uid), created
  }));
}

main().catch(error => {
  process.stdout.write(JSON.stringify({ ok: false, error: error.message || 'Demo setup failed.' }));
  process.exitCode = 1;
}).finally(() => deleteApp(app));
