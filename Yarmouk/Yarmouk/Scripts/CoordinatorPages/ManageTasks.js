(() => {
  'use strict';
  const page = document.querySelector('[data-coordinator-tasks]');
  if (!page) return;
  const state = window.CoordinatorBackend.state();
  const rows = state.data?.tasks || [];
  const escape = value => String(value ?? '').replace(/[&<>"']/g, ch => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[ch]));
  const list = page.querySelector('#coordinator-task-rows');
  const toolbar = page.querySelector('[data-task-tools]');
  const search = document.createElement('input');
  search.type = 'search'; search.placeholder = 'Search tasks or students'; search.setAttribute('aria-label','Search tasks or students'); search.maxLength = 200;
  const sort = document.createElement('select'); sort.setAttribute('aria-label','Sort tasks');
  [['newest','Newest first'],['student','Student name'],['title','Task title'],['deadline','Deadline']].forEach(([value,label]) => sort.add(new Option(label,value)));
  const size = document.createElement('select'); size.setAttribute('aria-label','Tasks per page');
  [6,10,20].forEach(value => size.add(new Option(String(value),String(value))));
  const exportButton = document.createElement('button'); exportButton.type = 'button'; exportButton.className = 'task-admin-export'; exportButton.textContent = 'Export Excel';
  toolbar.append(search,sort,size,exportButton);
  const summary = document.createElement('p'); summary.className='task-admin-summary'; summary.setAttribute('role','status'); summary.setAttribute('aria-live','polite');
  const pagination = document.createElement('nav'); pagination.className='task-admin-pagination'; pagination.setAttribute('aria-label','Task pages');
  const pageControls = document.createElement('div'); pageControls.className='task-admin-page-controls'; pagination.append(summary,pageControls);
  list.after(pagination);
  const dialog = page.querySelector('[data-task-dialog]');
  const detailContent = page.querySelector('[data-task-detail-content]');
  let currentPage = 1, opener = null;
  page.querySelector('[data-task-count]').textContent = `${rows.length} ${rows.length === 1 ? 'task' : 'tasks'}`;
  page.querySelector('[data-task-feedback]').textContent = state.error || state.notice || '';
  page.querySelector('[data-task-empty]').hidden = rows.length > 0;

  const taskStatusClass = value => /complete|done/i.test(value) ? 'is-complete' : /progress/i.test(value) ? 'is-progress' : 'is-pending';
  function openDetails(row, button) {
    opener = button;
    page.querySelector('#task-detail-title').textContent = row.title || 'Task details';
    const values = [['Student',row.studentName],['Student number',row.studentNumber],['Training provider',row.company],['Deadline',row.deadline],['Task status',row.taskStatus],['Submission',row.submission],['Submitted',row.submittedAt],['Review status',row.reviewStatus],['Description',row.description || 'No description provided']];
    detailContent.innerHTML = values.map(([label,value]) => `<div class="task-detail-item"><span>${escape(label)}</span><strong>${escape(value || '—')}</strong></div>`).join('');
    if (typeof dialog.showModal === 'function') dialog.showModal(); else dialog.setAttribute('open','');
  }
  function render() {
    const query = search.value.trim().toLocaleLowerCase();
    const filtered = rows.filter(row => `${row.title} ${row.studentName} ${row.studentNumber} ${row.company} ${row.taskStatus} ${row.reviewStatus}`.toLocaleLowerCase().includes(query));
    const key = sort.value;
    const ordered = [...filtered].sort((a,b) => key === 'student' ? a.studentName.localeCompare(b.studentName) : key === 'title' ? a.title.localeCompare(b.title) : key === 'deadline' ? a.deadline.localeCompare(b.deadline) : (Number(b.createdAt)||0)-(Number(a.createdAt)||0));
    const perPage = Number(size.value) || 6;
    const pages = Math.max(1,Math.ceil(ordered.length/perPage)); currentPage = Math.min(currentPage,pages);
    const start = (currentPage-1)*perPage; const visible = ordered.slice(start,start+perPage);
    list.innerHTML = visible.map((row,index) => {
      const photo = row.profileImageUrl ? `<img src="${escape(row.profileImageUrl)}" alt="" loading="lazy" data-student-photo data-initials="${escape(row.studentInitials || 'S')}" />` : `<span aria-hidden="true">${escape(row.studentInitials || 'S')}</span>`;
      const inlinePhoto = row.profileImageUrl ? `<img src="${escape(row.profileImageUrl)}" alt="" loading="lazy" data-student-photo data-inline-student-photo data-initials="${escape(row.studentInitials || 'S')}" />` : '';
      const tooltipId = `task-student-tooltip-${start+index}`;
      const studentFields = [['Student number',row.studentNumber],['Training provider',row.company]];
      return `<article class="task-admin-card">
      <div class="task-admin-card-main"><span class="task-admin-task-mark" aria-hidden="true">${String(start+index+1).padStart(2,'0')}</span><div class="task-admin-task-copy"><h3>${escape(row.title)}</h3><div class="task-admin-student-line"><span class="task-admin-student-preview" tabindex="0" role="group" aria-label="Student profile preview for ${escape(row.studentName)}" aria-describedby="${tooltipId}">${inlinePhoto}<span class="task-admin-student-text">${escape(row.studentName)} <span aria-hidden="true">·</span> ${escape(row.company)}</span><span class="task-admin-student-tooltip" id="${tooltipId}" role="tooltip"><span class="task-admin-tooltip-photo">${photo}</span><span class="task-admin-tooltip-details"><strong>${escape(row.studentName)}</strong><dl>${studentFields.map(([label,value]) => `<div><dt>${escape(label)}</dt><dd>${escape(value || 'Not provided')}</dd></div>`).join('')}</dl></span></span></span></div></div></div>
      <div class="task-admin-card-meta"><div><span>Deadline</span><strong>${escape(row.deadline)}</strong></div><div><span>Task status</span><strong><i class="task-admin-badge ${taskStatusClass(row.taskStatus)}">${escape(row.taskStatus)}</i></strong></div><div><span>Review</span><strong><i class="task-admin-badge ${/submitted|complete|accepted/i.test(row.reviewStatus) ? 'is-complete' : 'is-neutral'}">${escape(row.reviewStatus)}</i></strong></div></div>
      <button class="task-admin-details" type="button" data-task-details="${start+index}">View details</button>
    </article>`;
    }).join('');
    list.querySelectorAll('[data-student-photo]').forEach(image => image.addEventListener('error',() => {
      const fallback = document.createElement('span');
      fallback.setAttribute('aria-hidden','true');
      fallback.textContent = image.dataset.initials || 'S';
      if (image.hasAttribute('data-inline-student-photo')) image.remove();
      else image.replaceWith(fallback);
    },{once:true}));
    list.querySelectorAll('[data-task-details]').forEach(button => button.addEventListener('click',() => openDetails(visible[Number(button.dataset.taskDetails)-start],button)));
    list.querySelectorAll('.task-admin-student-preview').forEach(preview => {
      const placeTooltip = () => {
        const bounds = preview.getBoundingClientRect();
        preview.dataset.tooltipPlacement = bounds.top < 250 ? 'below' : 'above';
        preview.dataset.tooltipAlign = bounds.right > window.innerWidth - 420 ? 'end' : 'start';
      };
      preview.addEventListener('mouseenter',placeTooltip);
      preview.addEventListener('focus',placeTooltip);
    });
    summary.textContent = ordered.length ? `${start+1}–${Math.min(start+perPage,ordered.length)} of ${ordered.length} tasks` : '0 tasks';
    pageControls.replaceChildren();
    const addPage = (label,target,disabled,current) => { const button=document.createElement('button'); button.type='button'; button.textContent=label; button.disabled=disabled; if(current)button.setAttribute('aria-current','page'); button.addEventListener('click',()=>{currentPage=target;render();}); pageControls.append(button); };
    addPage('Previous',currentPage-1,currentPage===1,false);
    const pageNumbers = new Set();
    if (pages <= 7) {
      for (let number=1;number<=pages;number++) pageNumbers.add(number);
    } else {
      pageNumbers.add(1); pageNumbers.add(pages);
      for (let number=Math.max(2,currentPage-1);number<=Math.min(pages-1,currentPage+1);number++) pageNumbers.add(number);
      if (currentPage <= 2) pageNumbers.add(3);
      if (currentPage >= pages-1) pageNumbers.add(pages-2);
    }
    let previousPage = 0;
    [...pageNumbers].sort((a,b)=>a-b).forEach(number=>{
      if (previousPage && number-previousPage>1) {
        const ellipsis=document.createElement('span');
        ellipsis.className='task-admin-ellipsis';
        ellipsis.setAttribute('aria-hidden','true');
        ellipsis.textContent='…';
        pageControls.append(ellipsis);
      }
      addPage(String(number),number,false,number===currentPage);
      previousPage=number;
    });
    addPage('Next',currentPage+1,currentPage===pages,false);
    pagination.hidden = ordered.length === 0;
    page.querySelector('[data-task-empty]').hidden = rows.length > 0;
  }
  [search,sort,size].forEach(control => control.addEventListener(control===search?'input':'change',()=>{currentPage=1;render();}));
  exportButton.addEventListener('click',()=>window.UimsExcelExport?.download(`training-tasks-${new Date().toISOString().slice(0,10)}.csv`,['Student','Student number','Training provider','Task','Description','Deadline','Task status','Submission','Submitted','Review status'],rows.map(row=>[row.studentName,row.studentNumber,row.company,row.title,row.description,row.deadline,row.taskStatus,row.submission,row.submittedAt,row.reviewStatus])));
  page.querySelectorAll('[data-task-close]').forEach(button=>button.addEventListener('click',()=>dialog.close()));
  dialog.addEventListener('click',event=>{if(event.target===dialog)dialog.close();});
  dialog.addEventListener('close',()=>opener?.focus());
  render();
})();
