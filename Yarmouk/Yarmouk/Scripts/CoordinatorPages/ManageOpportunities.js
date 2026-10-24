(() => {
  'use strict';
  const root = document.querySelector('.op-admin');
  if (!root) return;
  const state = JSON.parse(document.querySelector('#backend-data').textContent);
  const records = state.data?.records || [];
  const escape = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const label = value => ({active:'Active',paused:'Paused',closed:'Closed',draft:'Draft',onsite:'On site',remote:'Remote',hybrid:'Hybrid'}[String(value || '').toLowerCase()] || value || 'Not specified');
  const date = value => { if(!value) return 'Not set'; const d = new Date(value.length === 10 ? value + 'T00:00:00' : value); return Number.isNaN(d.getTime()) ? value : d.toLocaleDateString('en-GB',{day:'2-digit',month:'short',year:'numeric'}); };
  const feedback = root.querySelector('[data-op-feedback]');
  if(state.error) { feedback.hidden=false; feedback.textContent=state.error; feedback.setAttribute('role','alert'); }
  root.querySelector('[data-op-total]').textContent = `${records.length} opportunities`;
  root.querySelector('[data-op-metrics]').innerHTML = [
    ['All opportunities',records.length,'Across training providers'],
    ['Active opportunities',records.filter(r=>r.status==='active').length,'Currently available to students'],
    ['Paused opportunities',records.filter(r=>r.status==='paused').length,'Temporarily unavailable'],
    ['Applications',records.reduce((total,r)=>total+r.applications,0),'Across the directory']
  ].map(([title,count,note])=>`<div><span>${escape(title)}</span><strong>${count}</strong><small>${escape(note)}</small></div>`).join('');
  const list = root.querySelector('.op-list');
  list.innerHTML = records.map((r,index)=>`<article class="op-card" data-index="${index}" data-status="${escape(r.status)}" data-company="${escape(r.company)}">
    <div class="op-identity"><span class="op-icon" aria-hidden="true">↗</span><div><h3>${escape(r.title || 'Untitled opportunity')}</h3><p>${escape(r.company)}${r.location ? ' · '+escape(r.location) : ''}</p><div class="op-tags">${r.majors.map(major=>`<span>${escape(major)}</span>`).join('')}</div></div></div>
    <div class="op-fact"><span>Available places</span><strong>${r.seats}</strong><small>${r.applications} applications</small></div>
    <div class="op-fact"><span>Deadline</span><strong>${escape(date(r.deadline))}</strong><small>${escape(label(r.workMode))}</small></div>
    <span class="op-status ${['active','paused','closed','draft'].includes(r.status)?r.status:'other'}">${escape(label(r.status))}</span>
    <div class="op-card-actions"><button type="button" data-op-open="${index}" aria-label="View details for ${escape(r.title || 'opportunity')}">View details <span aria-hidden="true">→</span></button>${r.status==='active'||r.status==='paused'?`<button type="button" class="op-pause-button" data-op-pause="${index}">${r.status==='active'?'Pause opportunity':'Resume opportunity'}</button>`:''}<a class="op-link" href="ApplicationsReview.aspx?opportunityId=${encodeURIComponent(r.id)}" aria-label="View ${r.applications} applications for ${escape(r.title || 'opportunity')}">Applications <span>${r.applications}</span> →</a></div>
  </article>`).join('');
  root.querySelector('[data-op-empty]').hidden = records.length > 0 || !!state.error;
  window.uimsOpportunityExport = () => ({headers:['Opportunity','Provider','Location','Status','Available places','Filled places','Applications','Deadline','Eligible majors','Work mode','Duration','Required hours'],rows:records.map(r=>[r.title,r.company,r.location,label(r.status),r.seats,r.filled,r.applications,r.deadline,r.majors.join(', '),label(r.workMode),r.duration,r.hours])});
  const dialog=root.querySelector('dialog');
  let opener;
  list.addEventListener('click',event=>{
    const pauseButton=event.target.closest('[data-op-pause]');
    if(pauseButton){
      const r=records[Number(pauseButton.dataset.opPause)]; if(!r) return;
      const pause=r.status==='active';
      if(!window.confirm((pause?'Pause ':'Resume ') + (r.title || 'this opportunity') + '?')) return;
      const form=document.querySelector('form');
      const action=form?.querySelector('[name="adminAction"]');
      const payload=form?.querySelector('[name="adminPayload"]');
      if(!form || !action || !payload) { feedback.hidden=false; feedback.textContent='The opportunity action could not be prepared. Reload and try again.'; feedback.setAttribute('role','alert'); return; }
      form.dataset.saving='true'; action.value='set-opportunity-paused'; payload.value=JSON.stringify({id:r.id,paused:pause});
      HTMLFormElement.prototype.submit.call(form); return;
    }
    const button=event.target.closest('[data-op-open]');
    if(!button)return;
    const r=records[Number(button.dataset.opOpen)]; if(!r)return;
    opener=button;
    root.querySelector('#op-detail-title').textContent=r.title || 'Opportunity details';
    const facts=[['Training provider',r.company],['Location',r.location],['Status',label(r.status)],['Deadline',date(r.deadline)],['Available places',r.seats],['Filled places',r.filled],['Applications',r.applications],['Work mode',label(r.workMode)],['Duration',r.duration],['Required hours',r.hours || 'Not specified']];
    root.querySelector('[data-op-details]').innerHTML=`<dl>${facts.map(([key,value])=>`<div><dt>${escape(key)}</dt><dd>${escape(value === 0 ? 0 : value || 'Not specified')}</dd></div>`).join('')}</dl>`+
      [['Description',r.description],['Requirements',r.requirements],['Eligible majors',r.majors.join(', ')],['Skills',r.skills.join(', ')]].map(([key,value])=>`<section><h3>${key}</h3><p>${escape(value || 'Not specified')}</p></section>`).join('')+
      (r.questions.length?`<section><h3>Application questions</h3><ol>${r.questions.map(q=>`<li>${escape(q)}</li>`).join('')}</ol></section>`:'');
    dialog.showModal();
  });
  root.querySelectorAll('[data-op-close]').forEach(button=>button.addEventListener('click',()=>dialog.close()));
  dialog.addEventListener('close',()=>opener?.focus());
})();
