(() => {
 'use strict';
 const root=document.querySelector('.app-admin'); if(!root)return;
 const state=JSON.parse(document.querySelector('#backend-data').textContent);
 const rows=state.data?.records || [];
 const esc=v=>String(v??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
 const label=v=>({new:'New',pending:'Pending',inreview:'In review',in_review:'In review',accepted:'Accepted',rejected:'Rejected',approved:'Accepted',declined:'Rejected'}[String(v||'new').toLowerCase()] || String(v||'New').replace(/([a-z])([A-Z])/g,'$1 $2').replace(/[_-]/g,' ').replace(/^./,c=>c.toUpperCase()));
 const date=v=>{if(!v)return 'Not recorded';const d=new Date(v);return Number.isNaN(d.getTime())?'Not recorded':d.toLocaleDateString('en-GB',{day:'2-digit',month:'short',year:'numeric'});};
 const avatar=r=>`<span class="op-applicant-photo uims-student-photo-avatar" data-student-photo-url="${esc(r.photo)}"><span>${esc((r.name||'S').trim().split(/\s+/).slice(0,2).map(part=>part[0]||'').join('').toUpperCase())}</span></span>`;
 const feedback=root.querySelector('[data-application-feedback]');
 if(state.error){feedback.hidden=false;feedback.textContent=state.error;feedback.setAttribute('role','alert');}
 root.querySelector('[data-application-scope]').textContent=state.data?.opportunityTitle || 'Application directory';
 root.querySelector('[data-application-count]').textContent=`(${rows.length})`;
 root.querySelector('[data-all-applications]').hidden=!state.data?.opportunityId;
 root.querySelector('[data-application-empty]').hidden=rows.length>0 || !!state.error;
 if(!state.data?.opportunityId)root.querySelector('[data-application-empty]').textContent='No applications have been received yet.';
 const list=root.querySelector('.op-list');
 list.innerHTML=rows.map((r,index)=>{const status=label(r.status);return `<article class="op-card" data-status="${esc(status)}" data-applied="${Number(r.appliedAt)||0}"><div class="op-identity">${avatar(r)}<div><h3>${esc(r.name)}</h3><p>${esc(r.number||'Student number not recorded')}</p><p>${esc(r.major)}</p></div></div><div class="op-fact app-opportunity"><span>Opportunity</span><strong>${esc(r.opportunity)}</strong><small>${esc(r.company)}</small></div><div class="op-fact"><span>Applied</span><strong>${esc(date(r.appliedAt))}</strong></div><span class="op-status" data-status="${esc(status)}">${esc(status)}</span><button type="button" data-application-open="${index}" aria-label="View application for ${esc(r.name)}">View application →</button></article>`;}).join('');
 window.UimsStudentPhotos?.enhance(list);
 window.uimsApplicationExport=()=>({headers:['Student','Student number','Email','Major','Opportunity','Provider','Status','Applied'],rows:rows.map(r=>[r.name,r.number,r.email,r.major,r.opportunity,r.company,label(r.status),date(r.appliedAt)])});
 const dialog=root.querySelector('dialog');let opener;
 list.addEventListener('click',event=>{
   const button=event.target.closest('[data-application-open]');if(!button)return;
   const r=rows[Number(button.dataset.applicationOpen)];if(!r)return;opener=button;
   root.querySelector('#application-detail-title').textContent=r.name;
   root.querySelector('[data-application-details]').innerHTML=avatar(r)+`<dl>${[['Student number',r.number],['Email',r.email],['Major',r.major],['Opportunity',r.opportunity],['Training provider',r.company],['Status',label(r.status)],['Applied',date(r.appliedAt)]].map(([key,value])=>`<div><dt>${key}</dt><dd>${esc(value||'Not provided')}</dd></div>`).join('')}</dl><section><h3>Application answers</h3>${Math.max(r.questions.length,r.answers.length)?Array.from({length:Math.max(r.questions.length,r.answers.length)},(_,i)=>`<section><h3>${esc(r.questions[i]||`Question ${i+1}`)}</h3><p>${esc(r.answers[i]||'No answer provided')}</p></section>`).join(''):'<p>No application questions or answers were recorded.</p>'}</section>`+(r.rejectionReason?`<section><h3>Rejection reason</h3><p>${esc(r.rejectionReason)}</p></section>`:'');
   window.UimsStudentPhotos?.enhance(dialog);dialog.showModal();
 });
 root.querySelectorAll('[data-application-close]').forEach(button=>button.addEventListener('click',()=>dialog.close()));
 dialog.addEventListener('close',()=>opener?.focus());
})();
