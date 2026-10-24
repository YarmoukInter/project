(function () {
  'use strict';
  const endpoint=document.documentElement.dataset.siteSettingsEndpoint||'/SiteSettings.ashx';
  fetch(endpoint,{credentials:'same-origin',cache:'no-store'})
    .then(response=>response.ok?response.json():null).then(settings=>{
      if(!settings)return;
      window.uimsSiteSettings=settings;
      const root=document.documentElement;
      root.dataset.siteDefaultTheme=settings.defaultTheme==='dark'?'dark':'light';
      root.style.setProperty('--uims-font-family',JSON.stringify(settings.fontFamily||'DM Sans')+',system-ui,sans-serif');
      if(settings.fontFamily==='DM Sans'||settings.fontFamily==='Inter'){const font=document.createElement('link');font.rel='stylesheet';font.href='https://fonts.googleapis.com/css2?family='+encodeURIComponent(settings.fontFamily)+':wght@400;500;600;700&display=swap';document.head.appendChild(font);}
      let style=document.getElementById('uims-site-font');if(!style){style=document.createElement('style');style.id='uims-site-font';document.head.appendChild(style);}style.textContent='body,body *:not(svg):not(svg *){font-family:var(--uims-font-family)!important}';
      try{if(!localStorage.getItem('yarmouk:portal-theme')){localStorage.setItem('yarmouk:portal-theme',root.dataset.siteDefaultTheme);root.dataset.theme=root.dataset.siteDefaultTheme;}}catch(_){}
      if(settings.logoDataUrl){
        const applyLogo=()=>document.querySelectorAll('.yu-sidebar-brand img:not(.yu-sidebar-crest),.yu-nav-brand img,.yu-footer-brand img,.footer-brand img,.final-logo,.gate-logo-img,.evaluation-hero__logo,.apply-brand img,.profile-cover__banner img,.about-logo-frame img').forEach(img=>{
          if(img.src!==settings.logoDataUrl)img.src=settings.logoDataUrl;
          // Some public-page themes use CSS content:url(...) to swap the fixed
          // light/dark logo. Disable that override for the coordinator's chosen logo.
          img.style.setProperty('content','none','important');
        });
        applyLogo();
        if(document.body){const observer=new MutationObserver(applyLogo);observer.observe(document.body,{childList:true,subtree:true});}
      }
      const compactIcon=settings.faviconDataUrl||settings.logoDataUrl;
      if(compactIcon){document.querySelectorAll('.yu-sidebar-brand .yu-sidebar-crest').forEach(img=>{if(img.src!==compactIcon)img.src=compactIcon;img.style.setProperty('content','none','important');});}
      if(settings.faviconDataUrl){
        let icons=Array.from(document.querySelectorAll('link[rel~="icon"]'));
        if(!icons.length){const icon=document.createElement('link');icon.rel='icon';document.head.appendChild(icon);icons=[icon];}
        const mime=(settings.faviconDataUrl.match(/^data:(image\/[a-z0-9.+-]+);/i)||[])[1]||'image/png';
        icons.forEach(icon=>{icon.href=settings.faviconDataUrl;icon.type=mime;});
      }
      const isHome=document.body.hasAttribute('data-home-root')||/\/PublicPages\/Home(?:\.aspx)?$/i.test(location.pathname);
      if(isHome){if(settings.homeTitle)document.title=settings.homeTitle;const description=document.querySelector('meta[name="description"]');if(description&&settings.homeDescription)description.content=settings.homeDescription;let keywords=document.querySelector('meta[name="keywords"]');if(settings.metaKeywords){if(!keywords){keywords=document.createElement('meta');keywords.name='keywords';document.head.appendChild(keywords);}keywords.content=settings.metaKeywords;}}
      document.dispatchEvent(new CustomEvent('uims:settings-applied',{detail:settings}));
    }).catch(()=>{});
})();
