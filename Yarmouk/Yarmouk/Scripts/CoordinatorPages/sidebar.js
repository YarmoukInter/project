/* Dashboard sidebar. Configure the HTML data-role and data-active-page.
   Map the .aspx filenames below to your application's existing routes. */
(function () {
  'use strict';
  const menus = {
    coordinator: [['dashboard','Coordinator Dashboard','grid'],['students','Manage Students','people'],['supervisors','Academic Supervisors','badge'],['companies','Manage Companies','building'],['opportunities','Training Opportunities','briefcase'],['applications','Opportunity Applications','send'],['progress','Monitor Placements','chart'],['tasks','Manage Tasks','tasks'],['completion','Completion Review','check'],['messages','Contact Messages','mail'],['settings','Site settings','check']]
  };
  function initialize() {
    document.querySelectorAll('[data-dashboard-sidebar]').forEach(root => {
      if (root.dataset.sidebarReady) return;
      root.dataset.sidebarReady = 'true';
      const role = 'coordinator';
      const base = new URL(root.dataset.baseUrl || './', document.baseURI);
      const pages = {
        coordinator: {
        dashboard:'Dashboard.aspx', students:'ManageStudents.aspx', companies:'ManageCompanies.aspx', opportunities:'ManageOpportunities.aspx', applications:'ApplicationsReview.aspx',
        supervisors:'ManageAcademicSupervisors.aspx', assignments:'AssignAcademicSupervisors.aspx',
        progress:'MonitorPlacements.aspx', tasks:'ManageTasks.aspx', completion:'FinalCompletionReview.aspx',
        messages:'ContactMessages.aspx', settings:'Settings.aspx', account:'Account.aspx'
        }
      };
      const folder = 'CoordinatorPages';
      const route = page => new URL(folder + '/' + pages[role][page], base).href;
      const activePage = root.dataset.activePage || Object.keys(pages[role]).find(page =>
        new URL(route(page)).pathname.replace(/\.aspx$/i,'').toLowerCase() === location.pathname.replace(/\.aspx$/i,'').toLowerCase());
      const aside = root.querySelector('.yu-sidebar'), rail = root.querySelector('.yu-sidebar-rail');
      const list = root.querySelector('[data-sidebar-links]'), toggle = root.querySelector('[data-sidebar-toggle]');
      const menu = root.querySelector('[data-sidebar-menu]'), pin = root.querySelector('[data-sidebar-pin]');
      const shade = root.querySelector('.yu-sidebar-shade'), theme = root.querySelector('[data-sidebar-theme]');
      const small = matchMedia('(max-width:1080px)');
      const storageKey = 'yarmouk:coordinator:sidebar:pinned';
      let pinned = false, hovering = false, focused = false, manual = false, mobileOpen = false, suppressed = false, hoverTimer;
      try { pinned = sessionStorage.getItem(storageKey) === 'true'; } catch (_) {}
      list.replaceChildren();
      menus[role].forEach(([page,label,icon]) => {
        const link = document.createElement('a'); link.className = 'yu-sidebar-link'; link.href = route(page);
        link.title = label; link.setAttribute('aria-label',label); link.dataset.page = page;
        if (page === activePage) link.setAttribute('aria-current','page');
        link.innerHTML = '<svg aria-hidden="true"><use href="#ys-icon-' + icon + '"></use></svg><span>' + label + '</span>';
        list.append(link);
      });
      root.querySelector('[data-sidebar-profile]').href = route('account');
      if (activePage === 'account') root.querySelector('[data-sidebar-profile]').setAttribute('aria-current','page');
      root.querySelector('[data-sidebar-role]').textContent = 'Coordinator';
      root.querySelector('[data-sidebar-home]').href = new URL('PublicPages/Home.aspx',base).href;
      root.classList.add('is-enhanced'); menu.hidden = false;
      function render() {
        const open = small.matches ? mobileOpen : pinned || manual || (!suppressed && (hovering || focused));
        rail.classList.toggle('is-open',open && !small.matches);
        aside.classList.toggle('is-collapsed',!open); aside.classList.toggle('is-mobile-open',small.matches && mobileOpen);
        aside.inert = small.matches && !mobileOpen;
        toggle.setAttribute('aria-expanded',String(open)); toggle.setAttribute('aria-label',open?'Collapse navigation':'Expand navigation');
        pin.setAttribute('aria-checked',String(pinned)); menu.setAttribute('aria-expanded',String(open));
        menu.setAttribute('aria-label',open?'Close navigation':'Open navigation');
        menu.querySelector('svg').innerHTML = open
          ? '<path d="M18 6 6 18M6 6l12 12" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>'
          : '<path d="M3 6h18M3 12h18M3 18h18" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>';
        shade.hidden = !(small.matches && mobileOpen);
      }
      function save() { try { sessionStorage.setItem(storageKey,String(pinned)); } catch (_) {} }
      function closeMobile(restoreFocus) { mobileOpen=false;render();if (restoreFocus) menu.focus(); }
      rail.addEventListener('pointerenter', event => {
        if (small.matches || event.pointerType === 'touch') return;
        clearTimeout(hoverTimer); hovering = true; suppressed = false; render();
      });
      rail.addEventListener('pointerleave', () => {
        if (small.matches) return;
        hoverTimer = setTimeout(() => { hovering = false; manual = false; render(); }, 160);
      });
      aside.addEventListener('focusin',event=>{if(!small.matches && event.target.matches(':focus-visible')){focused=true;suppressed=false;render();}});
      aside.addEventListener('focusout',event=>{if(!aside.contains(event.relatedTarget)&&event.relatedTarget!==menu){focused=false;if(small.matches)closeMobile(false);else render();}});
      toggle.addEventListener('click',()=>{
        if(small.matches){closeMobile(true);return;}
        if(rail.classList.contains('is-open')){pinned=false;manual=false;focused=false;suppressed=true;save();}
        else {manual=true;suppressed=false;}
        render();
      });
      pin.addEventListener('click',()=>{pinned=!pinned;manual=false;save();render();});
      menu.addEventListener('click',()=>{
        if(small.matches){mobileOpen=!mobileOpen;render();if(mobileOpen)list.querySelector('a').focus();return;}
        const open=rail.classList.contains('is-open');
        if(open){pinned=false;manual=false;focused=false;suppressed=true;save();}
        else{manual=true;suppressed=false;}
        render();
      });
      shade.addEventListener('click',()=>closeMobile(true));
      document.addEventListener('keydown',event=>{
        if(event.key!=='Escape')return;
        if(mobileOpen){closeMobile(true);return;}
        if(!small.matches&&aside.contains(document.activeElement)){
          manual=false;focused=false;suppressed=true;render();toggle.focus();
        }
      });
      document.addEventListener('pointerdown',event=>{if(mobileOpen&&!aside.contains(event.target)&&!menu.contains(event.target))closeMobile(false);});
      small.addEventListener('change',()=>{mobileOpen=false;hovering=false;focused=false;manual=false;render();});
      function syncTheme() {
        const dark=document.documentElement.dataset.theme==='dark';
        theme.setAttribute('aria-label',dark?'Switch to light mode':'Switch to dark mode');
        theme.querySelector('span').textContent=dark?'Light mode':'Dark mode';
        theme.querySelector('use').setAttribute('href','#ys-icon-'+(dark?'sun':'moon'));
      }
      try {const savedTheme=localStorage.getItem('yarmouk:portal-theme');document.documentElement.dataset.theme=savedTheme?savedTheme:(document.documentElement.dataset.siteDefaultTheme==='dark'?'dark':'light');}catch(_){}
      theme.addEventListener('click',()=>{const next=document.documentElement.dataset.theme==='dark'?'light':'dark';document.documentElement.dataset.theme=next;try{localStorage.setItem('yarmouk:portal-theme',next);}catch(_){}syncTheme();document.dispatchEvent(new CustomEvent('yarmouk:themechange',{detail:{theme:next}}));});
      render();syncTheme();
    });
  }
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',initialize,{once:true});else initialize();
  if(window.Sys&&window.Sys.Application)window.Sys.Application.add_load(initialize);
})();
