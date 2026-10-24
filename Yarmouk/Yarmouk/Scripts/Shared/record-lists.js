/* Progressive, client-only organization. Existing data loading and record actions remain owned by each page. */
(() => {
  'use strict';
  const text = (el, selector) => (selector ? el.querySelector(selector)?.textContent : el.textContent)?.trim() || '';
  const compare = new Intl.Collator(undefined, {numeric: true, sensitivity: 'base'}).compare;
  const normalize = value => String(value || '').normalize('NFKC').toLocaleLowerCase().replace(/\s+/g, ' ').trim();
  const route = location.pathname.replace(/\.aspx$/i, '').replace(/\/$/, '').toLowerCase();
  const nameKey = selector => el => text(el, selector);
  const dateKey = selector => el => {
    const value = Date.parse(text(el, selector).replace(/^Due:\s*/i, ''));
    return Number.isFinite(value) ? value : null;
  };
  const defaultKeys = {name: nameKey('h3, h2, strong'), status: nameKey('[class*="status"], [class*="badge"]')};
  const configs = {
    '/coordinatorpages/manageopportunities': [{list:'#coordinator-opportunity-list',item:'.op-card',label:'opportunities',search:true,export:true,pageSizes:[6,12,24],filters:[{label:'Training provider',allLabel:'All providers',key:el=>el.dataset.company},{label:'Status',allLabel:'All statuses',key:el=>text(el,'.op-status')}],keys:{name:nameKey('h3')},exportData:()=>window.uimsOpportunityExport()}],
    '/coordinatorpages/applicationsreview': [{list:'#coordinator-application-list',item:'.op-card',label:'applications',search:true,export:true,pageSizes:[6,12,24],filters:[{label:'Training provider',allLabel:'All providers',key:el=>text(el,'.app-opportunity small')},{label:'Status',allLabel:'All statuses',key:el=>el.dataset.status}],sorts:[['-date','Newest applications first'],['date','Oldest applications first'],['name','Student · A–Z']],initial:'-date',keys:{name:nameKey('h3'),date:el=>Number(el.dataset.applied)||null},exportData:()=>window.uimsApplicationExport()}],
    '/academicpages/tasks': [{list:'#academic-task-rows', item:'tr', label:'tasks', search:true, student:true,
      statuses:[['done','Submitted'],['inprogress','In progress'],['todo','To do'],['completed','Completed']],
      keys:{name:el=>el.dataset.student, title:nameKey('.task-register-title'), date:dateKey('.task-due'), status:el=>el.dataset.status},
      initial:'priority', sorts:[['priority','Needs review first'],['date','Due date · soonest'],['-date','Due date · latest'],['name','Student · A–Z'],['title','Task title · A–Z']]}],
    '/academicpages/studentprogress': [{list:'.progress-list',item:'.student-progress-card',label:'students',search:'#progressSearch',keys:defaultKeys}],
    '/academicpages/finalevaluation': [{list:'.evaluation-cards',item:'.evaluation-card',label:'evaluations',search:true,
      group:{label:'Training provider',key:el=>el.dataset.company},initial:'priority',priority:{ready:0,waiting:1,submitted:2},
      sorts:[['priority','Ready for review first'],['name','Student · A–Z'],['-name','Student · Z–A']],
      emptyWhenFiltered:true,emptyMessage:'No evaluations in this view. Choose another review status or adjust the search.',
      keys:{name:nameKey('.evaluation-student-name'),status:el=>el.dataset.status}}],
    '/companypages/mytrainees': [{list:'.trainees-grid',item:'.trainee-card',label:'trainees',search:'#traineeSearch',keys:defaultKeys}],
    '/companypages/manageopportunities': [{list:'#opportunityRows',item:'tr',label:'opportunities',search:'#opportunitySearch',keys:{name:nameKey('.co-opportunity-title'),date:dateKey('td:nth-child(4)'),status:nameKey('.co-status')}}],
    '/companypages/applicationsreview': [{list:'.co-candidate-list',item:'.co-candidate-card',label:'applications',search:false,keys:{name:nameKey('.co-student-name'),date:dateKey('.co-candidate-applied'),status:el=>el.dataset.status}}],
    '/companypages/managetasks': [{list:'.task-card-list',item:'.task-review-card',label:'tasks',search:true,keys:{name:nameKey('.task-card-student'),title:nameKey('.task-card-title'),date:dateKey('.task-card-deadline'),status:el=>el.dataset.status}}],
    '/companypages/finalevaluation': [{list:'.evaluation-trainee-list',item:'.evaluation-trainee-card',label:'trainees',search:true,keys:{name:nameKey('.evaluation-trainee-name'),status:el=>el.dataset.status}},
      {list:'#company-report-list',item:'.report-card',label:'reports',search:true,pageSizes:[3,6,12],keys:{name:nameKey('h4'),status:nameKey('.report-status')}}],
    '/coordinatorpages/managestudents': [{list:'#roster-body',item:'tr:not(:has([colspan]))',label:'students',search:true,export:true,statuses:[['active','Active'],['inactive','Inactive']],filters:[{label:'Department',allLabel:'All departments',key:el=>text(el,'.yr-column-department')},{label:'Supervisor',allLabel:'All supervisors',key:el=>el.querySelector('[data-assignment]')?.selectedOptions?.[0]?.textContent?.trim()}],keys:{name:nameKey('.yr-person-name'),status:nameKey('.yr-status')}}],
    '/coordinatorpages/managecompanies': [{list:'#roster-body',item:'tr:not(:has([colspan]))',label:'companies',search:true,export:true,keys:{name:nameKey('.yr-person-name'),status:nameKey('.yr-status')}}],
    '/coordinatorpages/manageacademicsupervisors': [{list:'#roster-body',item:'tr:not(:has([colspan]))',label:'supervisors',search:true,export:true,pageSizes:[6,12,24],statuses:[['active','Active'],['inactive','Inactive']],filters:[{label:'Department',allLabel:'All departments',key:el=>text(el,'.yr-column-department')},{label:'College',allLabel:'All colleges',key:el=>text(el,'.yr-column-college')},{label:'Assignments',allLabel:'All assignments',key:el=>Number(text(el,'.yr-number'))>0?'With assigned students':'No assigned students'}],sorts:[['name','Name · A–Z'],['-name','Name · Z–A'],['assigned','Fewest assigned students'],['-assigned','Most assigned students']],keys:{name:nameKey('.yr-person-name'),assigned:el=>Number(text(el,'.yr-number'))}}],
    '/coordinatorpages/monitorplacements': [{list:'#mp-rows',item:'.mp-placement-card',label:'placements',search:'#mp-search',export:true,keys:{name:nameKey('.mp-person-copy strong'),status:nameKey('.mp-badge'),progress:el=>Number(text(el,'.mp-progress-label strong').replace('%',''))},exportData:()=>({headers:['Student','Student number','Training role','Training provider','Location','Academic supervisor','Progress','Tasks','Evaluations','Approved hours','Status','Last update'],rows:(window.uimsMonitorData||[]).map(row=>[row.name,row.id,row.role,row.company,row.city,row.supervisor,row.progress+'%',row.tasks,row.evaluations,row.hours,row.status,row.updated])})}],
    '/coordinatorpages/finalcompletionreview': [{list:'#fr-rows',item:'tr',label:'evaluations',search:'#fr-search',export:true,keys:{name:nameKey('td:first-child strong'),status:nameKey('.fr-badge')},exportData:()=>({headers:['Student','Student number','Training provider','Opportunity','Company evaluation','Academic evaluation','Approved hours','Required hours','Status','Coordinator approved'],rows:(window.uimsFinalReviewExport||[]).map(record=>[record.name,record.studentId,record.company,record.opportunity,record.companySubmitted?'Received':'Pending',record.academicSubmitted?'Received':'Pending',record.approvedHours,record.hoursConfigured?record.requiredHours:'Not configured',record.status,record.coordinatorApproved?'Yes':'No'])})}],
    '/coordinatorpages/contactmessages': [{list:'#message-list',item:'.cm-message',label:'messages',search:true,keepSort:true,compact:true,externalControl:'#message-sort',export:true,exportIcon:true,exportData:()=>({headers:['Sender','Email','Subject','Message','Received','Read','Archived'],rows:(window.uimsCoordinatorMessagesForExport?.()||[]).map(message=>[message.fullName,message.email,message.subject,message.message,message.createdAtUnixMs?new Date(message.createdAtUnixMs).toISOString():'',message.isRead?'Yes':'No',message.isArchived?'Yes':'No'])})}],
    '/coordinatorpages/managetasks': [{list:'#coordinator-task-rows',item:'tr',label:'tasks',search:true,export:true,keys:{name:nameKey('.task-student-name'),title:nameKey('.task-title'),date:dateKey('.task-deadline'),status:nameKey('.task-status')}}],
    '/studentpages/opportunities': [{list:'#opportunityList',item:'.opportunity-card',label:'opportunities',search:false,keys:{name:nameKey('h3')},compact:true}],
    '/studentpages/myapplications': [{list:'#applicationList',item:'.application-card',label:'applications',search:false,keepSort:true}],
    '/studentpages/progress': [{list:'#academic-notes-list',item:'.academic-note',label:'notes',search:true,initial:'-date',keys:{name:nameKey('.academic-note-header strong'),date:el=>{const value=Date.parse(el.querySelector('time')?.dateTime||'');return Number.isFinite(value)?value:null;}}}]
  };
  const definitions = configs[route];
  if (!definitions) return;
  let nextId = 0;
  function mount(config, list) {
    if (list.dataset.recordListReady) return;
    list.dataset.recordListReady = 'true';
    const id = 'record-list-' + (++nextId);
    if (!list.id) list.id = id;
    let currentPage = 1;
    const smallScreen = matchMedia('(max-width: 700px)').matches;
    const pageSizes = config.pageSizes || (smallScreen ? [5,10,20] : list.tagName === 'TBODY' ? [10,20,50] : [6,12,24]);
    let pageSize = pageSizes[0];
    const originalOrder = new WeakMap();
    let order = 0;
    let scheduled = false;
    let lastTotal = -1;
    const target = list.closest('.evaluation-queue-region, .task-register-region, .yr-table-region, .co-table-wrap, .table-scroll, .trainee-table-wrap, .mp-table-wrap, .fr-table-wrap') || list;
    const toolbar = document.createElement('div');
    toolbar.className = 'record-tools' + (config.compact ? ' record-tools--compact' : '');
    toolbar.setAttribute('aria-label', `Organize ${config.label}`);
    const footer = document.createElement('nav');
    footer.className = 'record-pagination';
    footer.setAttribute('aria-label', `${config.label} pages`);
    const summary = document.createElement('p');
    summary.className = 'record-result-summary';
    summary.setAttribute('role','status');
    summary.setAttribute('aria-live','polite');
    const controls = document.createElement('div');
    controls.className = 'record-page-controls';
    footer.append(summary,controls);
    target.before(toolbar);
    target.after(footer);
    const fields = [];
    function field(label, type, options) {
      const wrap = document.createElement('label');
      wrap.className = 'record-field';
      if(label === 'Sort by') wrap.classList.add('record-field--sort');
      if(label === 'Per page') wrap.classList.add('record-field--size');
      const name = document.createElement('span');
      name.textContent = label;
      name.id = id + '-field-' + fields.length;
      const control = document.createElement(type === 'search' ? 'input' : 'select');
      control.setAttribute('aria-controls',list.id);
      control.setAttribute('aria-labelledby',name.id);
      if(type === 'search') { control.type = 'search'; control.placeholder = `Search ${config.label}`; control.maxLength = 200; }
      else (options || []).forEach(([value,title]) => control.add(new Option(title,value)));
      wrap.append(name,control);
      toolbar.append(wrap);
      fields.push(control);
      return control;
    }
    const search = config.search === true ? field('Search','search') : null;
    const student = config.student ? field('Student','select',[['','All students']]) : null;
    const group = config.group ? field(config.group.label,'select',[['','All providers']]) : null;
    const status = config.statuses ? field('Status','select',[['','All statuses'],...config.statuses]) : null;
    const customFilters = (config.filters || []).map(filter => ({filter, control:field(filter.label,'select',[[ '', filter.allLabel || `All ${filter.label.toLowerCase()}` ]])}));
    let sorts = config.sorts || [['source','Default order'],['name','Name · A–Z'],['-name','Name · Z–A']];
    if (!config.sorts && config.keys?.date) sorts = [...sorts,['date','Date · earliest'],['-date','Date · latest']];
    if (!config.sorts && config.keys?.status) sorts.push(['status','Group by status']);
    if (config.keys?.progress) sorts.push(['progress','Progress · lowest'],['-progress','Progress · highest']);
    const sort = !config.keepSort ? field('Sort by','select',sorts) : null;
    if(sort && config.initial) sort.value = config.initial;
    if (config.externalControl) {
      const extra = document.querySelector(config.externalControl);
      const wrapper = extra?.closest('.cm-list-toolbar');
      if (wrapper) toolbar.appendChild(wrapper);
    }
    const size = field('Per page','select',pageSizes.map(n=>[String(n),String(n)]));
    const reset = document.createElement('button');
    reset.type = 'button';
    reset.className = 'record-reset';
    reset.textContent = 'Clear filters';
    toolbar.append(reset);
    if (config.export) {
      const exportButton = document.createElement('button');
      exportButton.type = 'button';
      exportButton.className = 'record-export';
      if (config.exportIcon) {
        exportButton.innerHTML = '<svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M12 3v11m0 0 4-4m-4 4-4-4M5 16v4h14v-4" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg>';
      } else exportButton.textContent = 'Export Excel';
      exportButton.setAttribute('aria-label',`Export all ${config.label} to an Excel-compatible CSV file`);
      exportButton.addEventListener('click',()=>{
        if (!window.UimsExcelExport) return;
        let headers, rows;
        if (config.exportData) ({headers,rows}=config.exportData());
        else {
          const table = list.closest('table') || list.querySelector('table');
          if (table) {
            const headerCells = [...table.querySelectorAll('thead th')];
            const included = headerCells.map((cell,index)=>({index,label:cell.textContent.trim()}))
              .filter(column=>column.label && !/^(actions?|details?|review|select)$/i.test(column.label));
            headers = included.map(column=>column.label);
            rows = getItems().map(item=>included.map(column=>{
              const cell=item.cells?.[column.index];
              const selected=cell?.querySelector('select')?.selectedOptions?.[0]?.textContent?.trim();
              return selected || cell?.innerText?.trim() || '';
            }));
          } else {
            headers = ['Record'];
            rows = getItems().map(item=>{
              const clean=item.cloneNode(true);
              clean.querySelectorAll('button,a,input,select,textarea,svg,[aria-hidden="true"]').forEach(node=>node.remove());
              return [clean.innerText.trim()];
            });
          }
        }
        const slug=location.pathname.split('/').pop().replace(/\.aspx$/i,'').replace(/[^a-z0-9_-]/gi,'-').toLowerCase() || config.label;
        window.UimsExcelExport.download(`${slug}-${new Date().toISOString().slice(0,10)}.csv`,headers,rows);
      });
      toolbar.append(exportButton);
    }
    const noMatches = document.createElement('p');
    noMatches.className = 'record-no-matches';
    noMatches.textContent = config.emptyMessage || `No ${config.label} match. Try another search or clear your filters.`;
    noMatches.hidden = true;
    target.after(noMatches);
    const observer = new MutationObserver(mutations => {
      if (mutations.some(m => m.type === 'childList' || m.attributeName === 'hidden')) schedule();
    });
    const observe = () => observer.observe(list,{childList:true,subtree:true,attributes:true,attributeFilter:['hidden']});
    const getItems = () => [...list.querySelectorAll(config.item)];
    function schedule() {
      if(scheduled) return;
      scheduled = true;
      queueMicrotask(()=>{ scheduled=false; render(); });
    }
    function render() {
      observer.disconnect();
      const items = getItems();
      items.forEach(el=>{ if(!originalOrder.has(el)) originalOrder.set(el,order++); });
      if(student) {
        const names = [...new Set(items.map(el=>el.dataset.student).filter(Boolean))].sort(compare);
        if(student.options.length !== names.length+1 || names.some((n,i)=>student.options[i+1]?.value !== n)) {
          const selected = student.value;
          student.replaceChildren(new Option('All students',''), ...names.map(n=>new Option(n,n)));
          student.value = names.includes(selected) ? selected : '';
        }
      }
      if(group) {
        const values=[...new Set(items.map(config.group.key).filter(Boolean))].sort(compare);
        if(group.options.length!==values.length+1 || values.some((n,i)=>group.options[i+1]?.value!==n)) {
          const selected=group.value;
          group.replaceChildren(new Option('All providers',''),...values.map(n=>new Option(n,n)));
          group.value=values.includes(selected)?selected:'';
        }
      }
      customFilters.forEach(({filter,control})=>{
        const values=[...new Set(items.map(filter.key).filter(Boolean))].sort(compare);
        const selected=control.value;
        if(control.options.length!==values.length+1 || values.some((value,index)=>control.options[index+1]?.value!==value)) {
          control.replaceChildren(new Option(filter.allLabel || `All ${filter.label.toLowerCase()}`,''), ...values.map(value=>new Option(value,value)));
          control.value=values.includes(selected)?selected:'';
        }
      });
      const query = normalize(search?.value);
      const filtered = items.filter(el=>!el.hidden && (!query || normalize(el.textContent).includes(query)) && (!student?.value || el.dataset.student === student.value) && (!group?.value || config.group.key(el)===group.value) && (!status?.value || el.dataset.status === status.value) && customFilters.every(({filter,control})=>!control.value||filter.key(el)===control.value));
      const sorting = sort?.value || 'source';
      const descending = sorting.startsWith('-');
      const key = sorting.replace(/^-/,'');
      const priority = el => ((config.priority || {done:0,inprogress:1,todo:2,completed:3})[el.dataset.status] ?? 4);
      const getter = config.keys?.[key];
      const ordered = [...items].sort((a,b)=> {
        let result = 0;
        if(key === 'priority') result = priority(a)-priority(b) || (config.keys?.date ? ((config.keys.date(a) ?? Infinity)-(config.keys.date(b) ?? Infinity)) : 0);
        else if(getter) {
          const av = getter(a), bv = getter(b);
          if(av == null || bv == null) return av == null && bv == null ? originalOrder.get(a)-originalOrder.get(b) : av == null ? 1 : -1;
          result = typeof av === 'number' && typeof bv === 'number' ? av-bv : compare(String(av),String(bv));
          if(descending) result *= -1;
        }
        return result || originalOrder.get(a)-originalOrder.get(b);
      });
      // Move existing elements so bound events, WebForms names and entered values survive sorting.
      if(ordered.some((el,i)=>items[i]!==el)) ordered.forEach(el=>list.append(el));
      const matched = new Set(filtered);
      const visible = ordered.filter(el=>matched.has(el));
      const pages = Math.max(1,Math.ceil(visible.length/pageSize));
      currentPage = Math.max(1,Math.min(currentPage,pages));
      const start = (currentPage-1)*pageSize;
      const shown = new Set(visible.slice(start,start+pageSize));
      items.forEach(el=>el.toggleAttribute('data-record-hidden',!shown.has(el)));
      const hasOwnFilter = Boolean(query || student?.value || status?.value || group?.value || customFilters.some(({control})=>control.value));
      noMatches.hidden = visible.length>0 || !items.length || !(hasOwnFilter||config.emptyWhenFiltered);
      toolbar.hidden = !items.length && lastTotal<=0;
      reset.hidden = !hasOwnFilter;
      summary.textContent = visible.length ? `${start+1}–${Math.min(start+pageSize,visible.length)} of ${visible.length} ${config.label}` : `0 ${config.label}`;
      controls.replaceChildren();
      function pageButton(label,pageNumber,disabled,current) {
        const button = document.createElement('button');
        button.type = 'button'; button.textContent = label; button.disabled=disabled;
        button.setAttribute('aria-controls',list.id);
        if(current) button.setAttribute('aria-current','page');
        if(/^\d+$/.test(label)) button.setAttribute('aria-label',`Page ${label}`);
        button.addEventListener('click',()=>{
          currentPage=pageNumber; render();
          const next = [...controls.querySelectorAll('button')].find(b=>b.textContent===label && !b.disabled) || controls.querySelector('[aria-current]');
          next?.focus({preventScroll:true});
          const anchor = toolbar.getBoundingClientRect();
          if(anchor.top<0 || anchor.top>innerHeight) toolbar.scrollIntoView({block:'start',behavior:'auto'});
        });
        controls.append(button);
      }
      pageButton('Previous',currentPage-1,currentPage===1,false);
      const candidates = [...new Set(matchMedia('(max-width: 700px)').matches ? [1,currentPage,pages] : [1,currentPage-1,currentPage,currentPage+1,pages])].filter(n=>n>=1&&n<=pages).sort((a,b)=>a-b);
      let previous = 0;
      candidates.forEach(n=>{
        if(previous && n>previous+1) { const dots=document.createElement('span'); dots.textContent='…'; dots.setAttribute('aria-hidden','true'); controls.append(dots); }
        pageButton(String(n),n,false,n===currentPage); previous=n;
      });
      pageButton('Next',currentPage+1,currentPage===pages,false);
      footer.hidden=!items.length && lastTotal<=0;
      lastTotal=items.length;
      observe();
    }
    fields.forEach(control=>control.addEventListener(control.type==='search'?'input':'change',()=>{
      pageSize=Number(size.value); currentPage=1; render();
    }));
    reset.addEventListener('click',()=>{ if(search)search.value=''; if(student)student.value=''; if(status)status.value=''; if(group)group.value=''; customFilters.forEach(({control})=>control.value=''); currentPage=1;render(); search?.focus(); });
    toolbar.addEventListener('keydown',event=>{ if(event.key==='Enter' && event.target.matches('input[type="search"]')) event.preventDefault(); });
    const main = list.closest('main') || document.body;
    ['input','change','click'].forEach(eventName=>main.addEventListener(eventName,event=>{
      if(toolbar.contains(event.target)||footer.contains(event.target)||list.contains(event.target))return;
      if(event.target.matches('input,select')||event.target.closest('button[data-filter],button[data-view],button[data-status-filter]')) { currentPage=1; schedule(); }
    }));
    render();
  }
  function scan() { definitions.forEach(config=>document.querySelectorAll(config.list).forEach(list=>mount(config,list))); }
  scan();
  // Some pages create their lists after an API response or an existing renderer runs.
  const main = document.querySelector('.student-main, .yu-dashboard-main > main');
  if(main) new MutationObserver(mutations=>{
    if(mutations.some(m=>[...m.addedNodes].some(node=>node.nodeType===1 && !node.closest?.('.record-tools,.record-pagination') && definitions.some(c=>node.matches?.(c.list)||node.querySelector?.(c.list))))) scan();
  }).observe(main,{childList:true,subtree:true});
})();
