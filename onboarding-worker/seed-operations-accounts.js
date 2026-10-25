'use strict';

// Idempotent seed for a clean UIMS demonstration environment.
const path = require('node:path');
const { initializeApp, cert, deleteApp } = require('firebase-admin/app');
const { getAuth } = require('firebase-admin/auth');
const { getDatabase } = require('firebase-admin/database');

const serviceAccountPath = process.env.YARMOUK_FIREBASE_SERVICE_ACCOUNT
  || path.resolve(__dirname, '..', 'yarmouk-uims-firebase-adminsdk-fbsvc-a3b29f5bc6.json');
const app = initializeApp({
  credential: cert(require(serviceAccountPath)),
  databaseURL: process.env.YARMOUK_FIREBASE_DATABASE_URL || 'https://yarmouk-uims-default-rtdb.firebaseio.com'
});
const auth = getAuth(app);
const db = getDatabase(app);

const accounts = [
  { key: 'petra', name: 'Petra Digital Solutions', email: 'hr@petra-digital.uims.yarmouk.edu.jo', password: 'PetraTalent!2026', role: 'company', companyId: 'PETRA_DIGITAL_SOLUTIONS' },
  { key: 'techworks', name: 'Jordan TechWorks', email: 'careers@jordan-techworks.uims.yarmouk.edu.jo', password: 'TechWorks!2026', role: 'company', companyId: 'JORDAN_TECHWORKS' },
  { key: 'northstar', name: 'North Star Logistics', email: 'talent@northstar-logistics.uims.yarmouk.edu.jo', password: 'Logistics!2026', role: 'company', companyId: 'NORTH_STAR_LOGISTICS' },
  { key: 'rania', name: 'Dr. Rania Al-Hadidi', email: 'rania.alhadidi@uims.yarmouk.edu.jo', password: 'RaniaFaculty!2026', role: 'academic' },
  { key: 'omar', name: 'Dr. Omar Al-Khatib', email: 'omar.alkhatib@uims.yarmouk.edu.jo', password: 'OmarFaculty!2026', role: 'academic' }
];

const opportunitySpecs = [
  { company: 'petra', id: 'PETRA_BACKEND_2026', title: 'Backend Development Intern', description: 'Build and document REST APIs, assist with data modelling, and contribute to automated tests within an agile product team.', positions: 3, duration: '16 weeks', mode: 'Hybrid', requirements: 'C# or Java fundamentals, SQL, Git, and clear technical communication.' },
  { company: 'petra', id: 'PETRA_QA_2026', title: 'Quality Assurance Intern', description: 'Create test cases, run regression checks, record defects, and support the release-readiness process.', positions: 2, duration: '12 weeks', mode: 'OnSite', requirements: 'Attention to detail, test-case writing, and familiarity with web applications.' },
  { company: 'techworks', id: 'TECHWORKS_FRONTEND_2026', title: 'Frontend Development Intern', description: 'Implement responsive interface components and work with designers and backend engineers on customer-facing web products.', positions: 3, duration: '16 weeks', mode: 'Hybrid', requirements: 'HTML, CSS, JavaScript, Git, and a portfolio or coursework project.' },
  { company: 'techworks', id: 'TECHWORKS_DATA_2026', title: 'Data Analytics Intern', description: 'Prepare operational reports, clean source data, and build dashboard views that help teams make decisions.', positions: 2, duration: '14 weeks', mode: 'OnSite', requirements: 'Excel, SQL basics, analytical thinking, and clear written reporting.' },
  { company: 'northstar', id: 'NORTHSTAR_SYSTEMS_2026', title: 'Business Systems Intern', description: 'Support logistics system configuration, analyse process gaps, and prepare user documentation for operations teams.', positions: 2, duration: '16 weeks', mode: 'OnSite', requirements: 'Information systems fundamentals, process mapping, and communication skills.' },
  { company: 'northstar', id: 'NORTHSTAR_SUPPORT_2026', title: 'IT Support Intern', description: 'Assist with first-line support, asset inventory, account setup, and routine troubleshooting for office systems.', positions: 3, duration: '12 weeks', mode: 'OnSite', requirements: 'Networking basics, Windows support, customer service, and problem solving.' }
];

const faculty = 'Faculty of Information Technology and Computer Science';
const majors = [
  ['CS', 'Computer Science', 132],
  ['SE', 'Software Engineering', 132],
  ['CY', 'Cybersecurity', 132],
  ['CIS', 'Computer Information Systems', 132],
  ['CIT', 'Computer Information Technology', 132],
  ['DSAI', 'Data Science and Artificial Intelligence', 132]
];

async function ensureAccount(spec) {
  let user;
  try {
    user = await auth.getUserByEmail(spec.email);
    await auth.updateUser(user.uid, { displayName: spec.name, password: spec.password, disabled: false });
  } catch (error) {
    if (error.code !== 'auth/user-not-found') throw error;
    user = await auth.createUser({ email: spec.email, password: spec.password, displayName: spec.name, disabled: false });
  }
  return { ...spec, uid: user.uid };
}

async function main() {
  const resolved = {};
  for (const account of accounts) resolved[account.key] = await ensureAccount(account);

  const now = Date.now();
  const updates = {};
  for (const [id, name, requiredCreditHours] of majors) {
    updates[`majors/${id}`] = { id, name, faculty, requiredCreditHours };
  }
  for (const account of Object.values(resolved)) {
    updates[`users/${account.uid}`] = {
      uid: account.uid, email: account.email, fullName: account.name, englishName: account.name,
      role: account.role, isActive: true, mustChangePassword: true, createdAtUnixMs: now,
      ...(account.companyId ? { companyId: account.companyId } : {})
    };
  }
  for (const account of Object.values(resolved).filter(account => account.role === 'company')) {
    updates[`companies/${account.companyId}`] = {
      id: account.companyId, name: account.name, companyName: account.name, supervisorUid: account.uid,
      supervisorUids: { [account.uid]: true }, contactEmail: account.email, industry: account.key === 'northstar' ? 'Logistics and Supply Chain' : 'Information Technology',
      location: 'Irbid, Jordan', isApproved: true, isActive: true, createdAtUnixMs: now
    };
  }
  for (const opportunity of opportunitySpecs) {
    const company = resolved[opportunity.company];
    updates[`opportunities/${opportunity.id}`] = {
      id: opportunity.id, companyId: company.companyId, companyName: company.name, title: opportunity.title,
      description: opportunity.description, requirements: opportunity.requirements, eligibleMajorIds: ['CIS', 'CS'],
      availablePositions: opportunity.positions, filledPositions: 0, isActive: true, status: 'active',
      deadline: '2026-11-30', duration: opportunity.duration, requiredTrainingHours: 160,
      workMode: opportunity.mode, createdAtUnixMs: now, updatedAtUnixMs: now
    };
  }
  await db.ref().update(updates);
  process.stdout.write(JSON.stringify({
    ok: true,
    companies: accounts.filter(account => account.role === 'company').map(account => ({ name: account.name, email: account.email })),
    academics: accounts.filter(account => account.role === 'academic').map(account => ({ name: account.name, email: account.email })),
    opportunities: opportunitySpecs.map(opportunity => opportunity.title)
  }));
}

main().catch(error => { process.stderr.write(error.stack || error.message); process.exitCode = 1; })
  .finally(() => deleteApp(app));
