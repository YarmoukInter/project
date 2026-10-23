/* Dashboard sidebar. Configure the HTML data-role and data-active-page.
   Map the .aspx filenames below to your application's existing routes. */
(function () {
  'use strict';
  const menus = {
    student: [['dashboard','Dashboard','grid'],['opportunities','Opportunities','briefcase'],['applications','Applications','send'],['tasks','My tasks','tasks'],['progress','Progress','chart']],
    supervisor: [['dashboard','Academic Dashboard','grid'],['progress','Student Progress Monitor','chart'],['tasks','Student Tasks','tasks'],['evaluations','Final Academic Evaluation','check']]
  };
  function initialize() {
    document.querySelectorAll('[data-dashboard-sidebar]').forEach(root => {
      if (root.dataset.sidebarReady) return;
      root.dataset.sidebarReady = 'true';
      const role = root.dataset.role === 'student' ? 'student' : 'supervisor';
      const base = new URL(root.dataset.baseUrl || './', document.baseURI);
      const pages = {
        student: {
          dashboard:'Dashboard.aspx', opportunities:'Opportunities.aspx', applications:'MyApplications.aspx',
          tasks:'MyTasks.aspx', progress:'Progress.aspx', account:'Profile.aspx'
        },
        supervisor: {
          dashboard:'Dashboard.aspx', progress:'StudentProgress.aspx', tasks:'Tasks.aspx', evaluations:'FinalEvaluation.aspx', account:'Account.aspx'
        }
      };
      const folder = role === 'student' ? 'StudentPages' : 'AcademicPages';
      const route = page => new URL(folder + '/' + pages[role][page], base).href;
      const activePage = root.dataset.activePage || Object.keys(pages[role]).find(page =>
        new URL(route(page)).pathname.replace(/\.aspx$/i,'').toLowerCase() === location.pathname.replace(/\.aspx$/i,'').toLowerCase());
      const aside = root.querySelector('.yu-sidebar'), rail = root.querySelector('.yu-sidebar-rail');
      const list = root.querySelector('[data-sidebar-links]'), toggle = root.querySelector('[data-sidebar-toggle]');
      const menu = root.querySelector('[data-sidebar-menu]'), pin = root.querySelector('[data-sidebar-pin]');
      const shade = root.querySelector('.yu-sidebar-shade'), theme = root.querySelector('[data-sidebar-theme]');
      const small = matchMedia('(max-width:1080px)');
      const storageKey = 'yarmouk:' + role + ':sidebar:pinned';
      let pinned = false, hovering = false, focused = false, manual = false, mobileOpen = false, suppressed = false, timer;
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
      root.querySelector('[data-sidebar-role]').textContent = role === 'student' ? 'Student' : 'Academic supervisor';
      root.querySelector('[data-sidebar-home]').href = new URL(role === 'student' ? 'StudentPages/Dashboard.aspx' : 'PublicPages/Home.aspx',base).href;
      root.classList.add('is-enhanced'); menu.hidden = false;
      function render() {
        const open = small.matches ? mobileOpen : pinned || manual || (!suppressed && (hovering || focused));
        rail.classList.toggle('is-open',open && !small.matches);
        aside.classList.toggle('is-collapsed',!open && !small.matches); aside.classList.toggle('is-mobile-open',small.matches && mobileOpen);
        aside.inert = small.matches && !mobileOpen;
        toggle.setAttribute('aria-expanded',String(open)); toggle.setAttribute('aria-label',open?'Collapse navigation':'Expand navigation');
        pin.setAttribute('aria-checked',String(pinned)); menu.setAttribute('aria-expanded',String(small.matches && mobileOpen));
        shade.hidden = !(small.matches && mobileOpen);
      }
      function save() { try { sessionStorage.setItem(storageKey,String(pinned)); } catch (_) {} }
      function closeMobile(restoreFocus) { mobileOpen=false;render();if (restoreFocus) menu.focus(); }
      aside.addEventListener('pointerenter',event=>{if(small.matches || event.pointerType==='touch')return;clearTimeout(timer);hovering=true;suppressed=false;render();});
      aside.addEventListener('pointerleave',()=>{if(small.matches)return;timer=setTimeout(()=>{hovering=false;manual=false;suppressed=false;render();},220);});
      aside.addEventListener('focusin',event=>{if(!small.matches && event.target.matches(':focus-visible')){focused=true;suppressed=false;render();}});
      aside.addEventListener('focusout',event=>{if(!aside.contains(event.relatedTarget)){focused=false;if(small.matches)closeMobile(false);else render();}});
      toggle.addEventListener('click',()=>{
        if(small.matches){closeMobile(true);return;}
        if(rail.classList.contains('is-open')){pinned=false;manual=false;hovering=false;focused=false;suppressed=true;save();}
        else {manual=true;suppressed=false;}
        render();
      });
      pin.addEventListener('click',()=>{pinned=!pinned;manual=false;save();render();});
      menu.addEventListener('click',()=>{mobileOpen=!mobileOpen;render();if(mobileOpen)list.querySelector('a').focus();});
      shade.addEventListener('click',()=>closeMobile(true));
      list.addEventListener('click',event=>{if(small.matches && event.target.closest('a'))closeMobile(false);});
      document.addEventListener('keydown',event=>{
        if(event.key!=='Escape')return;
        if(mobileOpen){closeMobile(true);return;}
        if(!small.matches&&aside.contains(document.activeElement)){
          manual=false;hovering=false;focused=false;suppressed=true;render();toggle.focus();
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
      try {document.documentElement.dataset.theme=localStorage.getItem('yarmouk:portal-theme')==='dark'?'dark':'light';}catch(_){}
      theme.addEventListener('click',()=>{const next=document.documentElement.dataset.theme==='dark'?'light':'dark';document.documentElement.dataset.theme=next;try{localStorage.setItem('yarmouk:portal-theme',next);}catch(_){}syncTheme();document.dispatchEvent(new CustomEvent('yarmouk:themechange',{detail:{theme:next}}));});
      render();syncTheme();
    });
  }
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',initialize,{once:true});else initialize();
  if(window.Sys&&window.Sys.Application)window.Sys.Application.add_load(initialize);
})();
