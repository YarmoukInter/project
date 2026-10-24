'use strict';

// Read-only validation for the full QA fixture created by seed-full-qa-journey.js.
const path = require('node:path');
const { initializeApp, cert, deleteApp } = require('firebase-admin/app');
const { getAuth } = require('firebase-admin/auth');
const { getDatabase } = require('firebase-admin/database');

const marker = 'full-qa-journey-20261024';
const companyId = 'QA_WESSAM_KHRAISAT_COMPANY_20261024';
const finalPlacementId = 'QA_WK_PLACEMENT_4_20261024';
const evaluationId = `EVAL_${finalPlacementId}`;
const app = initializeApp({
  credential: cert(require(path.resolve(__dirname, '..', 'yarmouk-uims-firebase-adminsdk-fbsvc-a3b29f5bc6.json'))),
  databaseURL: 'https://yarmouk-uims-default-rtdb.firebaseio.com'
});
const db = getDatabase(app);
const auth = getAuth(app);
const fail = message => { throw new Error(message); };

async function main() {
  const root = (await db.ref().get()).val() || {};
  const fixtureUsers = Object.entries(root.users || {}).filter(([, value]) => value?.demoSeedKey === marker);
  if (fixtureUsers.length !== 10) fail(`Expected 10 fixture accounts; found ${fixtureUsers.length}.`);
  for (const [uid, profile] of fixtureUsers) {
    const identity = await auth.getUser(uid);
    if (identity.email !== profile.email || identity.disabled || identity.displayName !== profile.fullName)
      fail(`Authentication/profile mismatch for ${profile.email}.`);
  }
  const company = root.companies?.[companyId];
  if (!company?.isApproved || !company?.supervisorUid || !company.supervisorUids?.[company.supervisorUid])
    fail('Company supervisor linkage is not valid.');
  const opportunities = Object.values(root.opportunities || {}).filter(value => value?.demoSeedKey === marker);
  if (opportunities.length !== 2 || opportunities.some(value => value.status !== 'active' || value.companyId !== companyId))
    fail('QA opportunities are not active and linked to the QA company.');
  const applications = Object.values(root.applications || {}).filter(value => value?.demoSeedKey === marker);
  const statuses = applications.map(value => String(value.status).toLowerCase());
  for (const status of ['new', 'offered', 'rejected', 'accepted']) if (!statuses.includes(status)) fail(`Missing ${status} application scenario.`);
  const finalEvaluation = root.evaluations?.[evaluationId];
  if (!finalEvaluation?.companySubmitted || !finalEvaluation?.academicSubmitted || !finalEvaluation?.coordinatorApproved)
    fail('The completed evaluation has not passed all three approval stages.');
  if (finalEvaluation.companyScore !== 90 || finalEvaluation.academicScore !== 92 || finalEvaluation.finalScore !== 91)
    fail('The final evaluation scores are incorrect.');
  const finalStudentUid = finalEvaluation.studentUid;
  const finalStudent = root.students?.[finalStudentUid];
  if (!finalStudent?.completionCertificateAvailable || finalStudent.completionCertificateEvaluationId !== evaluationId)
    fail('The completed student cannot access the official certificate.');
  const completedTasks = Object.values(root.tasks || {}).filter(value => value?.placementId === finalPlacementId && String(value.status).toLowerCase() === 'completed');
  if (completedTasks.length !== 10) fail(`Expected 10 completed tasks for the final path; found ${completedTasks.length}.`);
  const blockedPlacementId = 'QA_WK_PLACEMENT_7_20261024';
  const blockedTasks = Object.values(root.tasks || {}).filter(value => value?.placementId === blockedPlacementId);
  if (blockedTasks.length !== 10 || blockedTasks.filter(value => String(value.status).toLowerCase() === 'completed').length !== 9)
    fail('The incomplete-task blocking scenario is not configured correctly.');
  if (Object.values(root.evaluations || {}).some(value => value?.placementId === blockedPlacementId))
    fail('An evaluation exists for a student with an incomplete task.');
  process.stdout.write(JSON.stringify({ ok: true, accounts: fixtureUsers.length, opportunities: opportunities.length,
    applicationStatuses: [...new Set(statuses)].sort(), completedTasks: completedTasks.length,
    incompleteScenario: '9 of 10 tasks completed; no evaluation exists', certificateAvailable: true }, null, 2));
}

main().catch(error => { process.stderr.write((error.stack || error.message) + '\n'); process.exitCode = 1; })
  .finally(() => deleteApp(app));
