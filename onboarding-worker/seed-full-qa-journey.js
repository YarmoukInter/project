'use strict';

// Creates an idempotent, clearly labelled end-to-end QA fixture.  It is kept
// separate from production records by `demoSeedKey` and refuses to overwrite
// an account that does not belong to this fixture.
const path = require('node:path');
const { initializeApp, cert, deleteApp } = require('firebase-admin/app');
const { getAuth } = require('firebase-admin/auth');
const { getDatabase } = require('firebase-admin/database');

const marker = 'full-qa-journey-20261024';
const password = 'QaJourney!2026';
const companyId = 'QA_WESSAM_KHRAISAT_COMPANY_20261024';
const academicEmail = 'wessam.khraisat.academic.qa@yarmouk.com';
const accounts = [
  ...Array.from({ length: 7 }, (_, index) => ({
    key: `student${index + 1}`,
    name: `Wessam Khraisat Student ${index + 1}`,
    email: `wessam.khraisat.student${index + 1}@yarmouk.com`,
    role: 'student'
  })),
  { key: 'company', name: 'Wessam Khraisat Company', email: 'wessam.khraisat.company@yarmouk.com', role: 'company' },
  { key: 'hr', name: 'Wessam Khraisat HR', email: 'wessam.khraisat.hr@yarmouk.com', role: 'company' },
  // A dedicated academic account is needed to verify the final-evaluation and
  // meeting/certificate stages without relying on a real staff account.
  { key: 'academic', name: 'Wessam Khraisat Academic QA', email: academicEmail, role: 'academic' }
];

const app = initializeApp({
  credential: cert(require(path.resolve(__dirname, '..', 'yarmouk-uims-firebase-adminsdk-fbsvc-a3b29f5bc6.json'))),
  databaseURL: 'https://yarmouk-uims-default-rtdb.firebaseio.com'
});
const auth = getAuth(app);
const db = getDatabase(app);

async function ensureAccount(spec) {
  let user;
  try {
    user = await auth.getUserByEmail(spec.email);
    const profile = (await db.ref(`users/${user.uid}`).get()).val();
    if (profile && profile.demoSeedKey && profile.demoSeedKey !== marker)
      throw new Error(`Refusing to reuse ${spec.email}: it belongs to another test fixture.`);
    if (profile && !profile.demoSeedKey)
      throw new Error(`Refusing to reuse ${spec.email}: it is not a QA fixture account.`);
    await auth.updateUser(user.uid, { displayName: spec.name, password, disabled: false });
  } catch (error) {
    if (error.code !== 'auth/user-not-found') throw error;
    user = await auth.createUser({ email: spec.email, password, displayName: spec.name });
  }
  return { ...spec, uid: user.uid };
}

function studentRecord(account, index, academicUid) {
  return {
    uid: account.uid,
    studentId: `QA-WK-2026-${String(index).padStart(2, '0')}`,
    studentNumber: `QA-WK-2026-${String(index).padStart(2, '0')}`,
    englishName: account.name,
    fullName: account.name,
    majorId: 'CIS',
    academicSupervisorId: academicUid,
    academicSupervisorName: 'Wessam Khraisat Academic QA',
    requiredTrainingHours: 160,
    approvedTrainingHours: index === 4 ? 160 : 0,
    internshipStatus: index === 4 ? 'Completed' : index === 7 ? 'InProgress' : 'Application stage',
    demoSeedKey: marker
  };
}

function applicationStatus(index) {
  return ({ 1: 'New', 2: 'Offered', 3: 'Rejected', 4: 'Accepted', 5: 'Rejected', 6: 'New', 7: 'Accepted' })[index];
}

async function main() {
  const resolved = {};
  for (const account of accounts) resolved[account.key] = await ensureAccount(account);
  const now = Date.now();
  const students = Array.from({ length: 7 }, (_, index) => resolved[`student${index + 1}`]);
  const company = resolved.company;
  const hr = resolved.hr;
  const academic = resolved.academic;
  const opportunityId = 'QA_WK_BACKEND_INTERNSHIP_20261024';
  const secondOpportunityId = 'QA_WK_FRONTEND_INTERNSHIP_20261024';
  const updates = {};

  for (const account of Object.values(resolved)) {
    updates[`users/${account.uid}`] = {
      uid: account.uid, email: account.email, fullName: account.name, role: account.role,
      isActive: true, mustChangePassword: false, companyId: account.role === 'company' ? companyId : '',
      demoSeedKey: marker, qaFixture: true, createdAtUnixMs: now
    };
  }
  updates[`companies/${companyId}`] = {
    id: companyId, name: 'Wessam Khraisat Company', companyName: 'Wessam Khraisat Company',
    isApproved: true, supervisorUid: company.uid, supervisorUids: { [company.uid]: true, [hr.uid]: true },
    industry: 'Information Technology', location: 'Irbid, Jordan', contactEmail: company.email,
    Phone: '+962 7 9000 0000', demoSeedKey: marker
  };
  updates[`opportunities/${opportunityId}`] = {
    id: opportunityId, companyId, title: 'QA Backend Development Internship',
    description: 'End-to-end QA opportunity used to validate applications, tasks, evaluations, and certificates.',
    eligibleMajorIds: ['CIS'], availablePositions: 4, filledPositions: 2,
    isActive: true, status: 'active', deadline: '2026-12-31',
    requirements: 'QA fixture; CIS students only.', duration: '16 weeks',
    requiredTrainingHours: 160, workMode: 'OnSite',
    applicationQuestions: ['Why are you interested in this internship?', 'Describe a technical project you completed.'],
    demoSeedKey: marker
  };
  updates[`opportunities/${secondOpportunityId}`] = {
    id: secondOpportunityId, companyId, title: 'QA Frontend Development Internship',
    description: 'Second active opportunity to validate company opportunity listing and application filtering.',
    eligibleMajorIds: ['CIS'], availablePositions: 3, filledPositions: 0,
    isActive: true, status: 'active', deadline: '2026-12-31',
    requirements: 'QA fixture; CIS students only.', duration: '12 weeks',
    requiredTrainingHours: 160, workMode: 'Hybrid', demoSeedKey: marker
  };

  students.forEach((account, zeroIndex) => {
    const index = zeroIndex + 1;
    const applicationId = `QA_WK_APPLICATION_${index}_20261024`;
    const placementId = `QA_WK_PLACEMENT_${index}_20261024`;
    const status = applicationStatus(index);
    const student = studentRecord(account, index, academic.uid);
    updates[`students/${account.uid}`] = student;
    updates[`applications/${applicationId}`] = {
      id: applicationId, studentUid: account.uid, opportunityId: index === 6 ? secondOpportunityId : opportunityId,
      companyId, status, appliedAtUnixMs: now - index * 86400000,
      applicationQuestions: ['Why are you interested in this internship?', 'Describe a technical project you completed.'],
      answers: [`I want practical experience in the QA workflow (${index}).`, `QA project ${index} validates the complete flow.`],
      reviewedByUid: (status === 'Rejected' || status === 'Offered' || status === 'Accepted') ? hr.uid : '',
      reviewedAtUnixMs: status === 'New' ? null : now - index * 3600000,
      rejectionReason: status === 'Rejected' ? 'QA scenario: another applicant was selected for this test opportunity.' : '',
      placementId: status === 'Accepted' ? placementId : '', isConfirmedPlacement: status === 'Accepted', demoSeedKey: marker
    };
    if (status === 'Accepted') {
      student.acceptedApplicationId = applicationId;
      student.companyId = companyId;
      updates[`placements/${placementId}`] = {
        id: placementId, studentUid: account.uid, studentId: `QA-WK-2026-${String(index).padStart(2, '0')}`,
        companyId, opportunityId, applicationId, academicSupervisorUid: academic.uid, companySupervisorUid: hr.uid,
        status: index === 4 ? 'completed' : 'InProgress', startDate: '2026-09-01', endDate: '2026-12-15',
        requiredHours: 160, createdAtUnixMs: now - 30 * 86400000, demoSeedKey: marker
      };
      const completeTaskCount = index === 4 ? 10 : 9;
      for (let taskNumber = 1; taskNumber <= 10; taskNumber += 1) {
        const taskId = `QA_WK_TASK_${index}_${String(taskNumber).padStart(2, '0')}_20261024`;
        const complete = taskNumber <= completeTaskCount;
        updates[`tasks/${taskId}`] = {
          id: taskId, taskNumber: `QA-${index}-${taskNumber}`, title: `QA task ${taskNumber} for ${account.name}`,
          description: 'Controlled task used to validate task completion and final-evaluation eligibility.',
          studentUid: account.uid, studentName: account.name, companyId, placementId, opportunityId,
          supervisorUid: hr.uid, dueAtUnixMs: now - (12 - taskNumber) * 86400000,
          createdAtUnixMs: now - (25 - taskNumber) * 86400000, status: complete ? 'Completed' : 'InProgress', demoSeedKey: marker
        };
        if (complete) updates[`taskSubmissions/${taskId}_SUB_${account.uid}`] = {
          id: `${taskId}_SUB_${account.uid}`, taskId, studentUid: account.uid, placementId,
          notes: `QA completion evidence for task ${taskNumber}.`, status: 'Submitted', reviewStatus: 'Approved',
          submittedAtUnixMs: now - (11 - taskNumber) * 86400000, demoSeedKey: marker
        };
      }
    }
  });

  const finalStudent = students[3];
  const finalPlacementId = 'QA_WK_PLACEMENT_4_20261024';
  const evaluationId = `EVAL_${finalPlacementId}`;
  const criteria = Object.fromEntries(Array.from({ length: 18 }, (_, index) => [String(index + 1), 'Excellent']));
  updates[`evaluations/${evaluationId}`] = {
    id: evaluationId, placementId: finalPlacementId, studentUid: finalStudent.uid,
    companyId, companySupervisorUid: hr.uid, academicSupervisorUid: academic.uid,
    companyScore: 90, companyFeedback: 'Completed all assigned QA tasks to the required standard.',
    companyCriteria: criteria, companySubmitted: true, companyUpdatedAtUnixMs: now - 3 * 86400000,
    companySubmittedAtUnixMs: now - 3 * 86400000, academicScore: 92,
    academicFeedback: 'Academic review completed successfully.', academicSubmitted: true,
    academicUpdatedAtUnixMs: now - 2 * 86400000, academicSubmittedAtUnixMs: now - 2 * 86400000,
    zoomMeetingDate: '2026-12-20T10:00', zoomMeetingUrl: 'https://zoom.us/j/1234567890',
    zoomMeetingAssessment: 'Final meeting completed; student met all programme requirements.',
    coordinatorApproved: true, coordinatorUid: 'QA_FIXTURE_COORDINATOR', coordinatorApprovedAtUnixMs: now - 86400000,
    finalScore: 91, officialCertificateGeneratedAtUnixMs: now - 3600000, demoSeedKey: marker
  };
  updates[`students/${finalStudent.uid}`].completionCertificateAvailable = true;
  updates[`students/${finalStudent.uid}`].completionCertificateEvaluationId = evaluationId;
  updates[`trainingLogs/QA_WK_LOG_4_20261024`] = {
    id: 'QA_WK_LOG_4_20261024', studentUid: finalStudent.uid, placementId: finalPlacementId,
    companyId, hours: 160, status: 'approved', isApproved: true, submittedAtUnixMs: now - 5 * 86400000,
    demoSeedKey: marker
  };

  await db.ref().update(updates);
  const verification = await Promise.all([
    db.ref(`companies/${companyId}`).get(), db.ref(`opportunities/${opportunityId}`).get(),
    db.ref(`evaluations/${evaluationId}`).get(), db.ref(`students/${finalStudent.uid}`).get(),
    ...accounts.map(account => auth.getUserByEmail(account.email))
  ]);
  const evaluation = verification[2].val() || {};
  const finalStudentRecord = verification[3].val() || {};
  if (!verification[0].exists() || !verification[1].exists() || !evaluation.companySubmitted || !evaluation.academicSubmitted ||
      !evaluation.coordinatorApproved || !finalStudentRecord.completionCertificateAvailable)
    throw new Error('Post-seed verification failed. The fixture was not left in a complete state.');
  process.stdout.write(JSON.stringify({
    ok: true, marker, password, companyId, opportunityIds: [opportunityId, secondOpportunityId],
    accounts: accounts.map(account => ({ name: account.name, email: account.email, role: account.role })),
    finalPath: { student: finalStudent.name, placementId: finalPlacementId, evaluationId, finalScore: evaluation.finalScore },
    scenarios: { newApplications: [1, 6], offered: [2], rejected: [3, 5], accepted: [4, 7],
      completedForEvaluation: 4, blockedByIncompleteTask: 7 }
  }, null, 2));
}

main().catch(error => { process.stderr.write((error.stack || error.message) + '\n'); process.exitCode = 1; })
  .finally(() => deleteApp(app));
