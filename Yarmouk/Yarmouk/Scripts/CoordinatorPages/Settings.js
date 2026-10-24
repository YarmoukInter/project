(function () {
  'use strict';
  const root = document.querySelector('[data-site-settings-page]');
  if (!root) return;
  let saved = null;
  const feedback = root.querySelector('[data-settings-feedback]');
  const get = name => root.querySelector('[name="' + name + '"]');
  const dirty = root.querySelector('[data-settings-dirty]');
  let originalTheme = document.documentElement.dataset.theme || null;
  let originalFont = document.documentElement.style.getPropertyValue('--uims-font-family');
  function captureInitialAppearance(){originalTheme=document.documentElement.dataset.theme||'light';originalFont=document.documentElement.style.getPropertyValue('--uims-font-family');}
  if(document.readyState==='loading')document.addEventListener('DOMContentLoaded',captureInitialAppearance,{once:true});else captureInitialAppearance();
  const tabs = Array.from(root.querySelectorAll('[data-settings-tab]'));
  const panels = Array.from(root.querySelectorAll('.settings-tab-panel'));
  function activateTab(tab, focus) {
    const selected = tab.dataset.settingsTab;
    root.classList.toggle('is-subscription-tab', selected === 'subscription');
    tabs.forEach(item => {
      const active = item === tab;
      item.classList.toggle('is-active', active);
      item.setAttribute('aria-selected', active ? 'true' : 'false');
      item.tabIndex = active ? 0 : -1;
    });
    panels.forEach(panel => { panel.hidden = panel.id !== 'settings-panel-' + selected; });
    if (focus) tab.focus();
  }
  tabs.forEach((tab, index) => {
    tab.addEventListener('click', () => activateTab(tab, false));
    tab.addEventListener('keydown', event => {
      let next = index;
      if (event.key === 'ArrowRight' || event.key === 'ArrowDown') next = (index + 1) % tabs.length;
      else if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') next = (index - 1 + tabs.length) % tabs.length;
      else if (event.key === 'Home') next = 0;
      else if (event.key === 'End') next = tabs.length - 1;
      else return;
      event.preventDefault();
      activateTab(tabs[next], true);
    });
  });
  const defaults = {defaultTheme:'light',fontFamily:'DM Sans',logoDataUrl:'',faviconDataUrl:'',firebaseDatabaseUrl:'',homeTitle:'Yarmouk University — Your practical training journey',homeDescription:'Explore practical training at Yarmouk University. Discover an opportunity, confirm your placement, develop your skills and complete your internship.',metaKeywords:'Yarmouk University, practical training, internship',headPlugins:'',bodyPlugins:'',planCaps:{basicStudents:100,basicCompanies:10,advancedStudents:500,advancedCompanies:50},subscription:{currentPlan:'basic',trialStart:'',trialEnd:''}};
  function markDirty(){dirty.hidden=false;}
  function updateThemeButton(){const dark=document.documentElement.dataset.theme==='dark',button=document.querySelector('[data-sidebar-theme]');if(!button)return;button.setAttribute('aria-label',dark?'Switch to light mode':'Switch to dark mode');const label=button.querySelector('span'),icon=button.querySelector('use');if(label)label.textContent=dark?'Light mode':'Dark mode';if(icon)icon.setAttribute('href','#ys-icon-'+(dark?'sun':'moon'));}
  function previewTheme(theme){document.documentElement.dataset.theme=theme;updateThemeButton();document.dispatchEvent(new CustomEvent('yarmouk:themechange',{detail:{theme}}));}
  function previewFont(font){document.documentElement.style.setProperty('--uims-font-family',JSON.stringify(font)+',system-ui,sans-serif');let link=document.getElementById('uims-settings-font-preview');if(font==='DM Sans'||font==='Inter'){if(!link){link=document.createElement('link');link.id='uims-settings-font-preview';link.rel='stylesheet';document.head.appendChild(link);}link.href='https://fonts.googleapis.com/css2?family='+encodeURIComponent(font)+':wght@400;500;600;700&display=swap';}else if(link)link.remove();}
  function readBackend() { try { const value=JSON.parse(document.getElementById('backend-data').textContent||'{}'); return value.data||defaults; } catch (_) { return defaults; } }
  function fill(value) {
    saved=Object.assign({},defaults,value||{}); saved.subscription=Object.assign({},defaults.subscription,saved.subscription||{});saved.planCaps=Object.assign({},defaults.planCaps,saved.planCaps||{});
    ['defaultTheme','fontFamily','firebaseDatabaseUrl','homeTitle','homeDescription','metaKeywords','headPlugins','bodyPlugins'].forEach(k=>{if(get(k))get(k).value=saved[k]||'';});
    ['currentPlan'].forEach(k=>{if(get(k))get(k).value=saved.subscription[k]||'basic';});
    const preview=root.querySelector('[data-logo-preview]'), img=preview.querySelector('img'), label=preview.querySelector('span');
    const currentLogo=saved.logoDataUrl||(document.querySelector('.yu-sidebar-brand img:not(.yu-sidebar-crest),.yu-nav-brand img,.gate-logo-img,.evaluation-hero__logo')||{}).src||'';
    if(currentLogo){img.src=currentLogo;img.hidden=false;label.textContent='Current website logo';label.hidden=false;}else{img.removeAttribute('src');img.hidden=true;label.textContent='No logo available';label.hidden=false;}
    const faviconPreview=root.querySelector('[data-favicon-preview]'),faviconImage=faviconPreview.querySelector('img'),faviconLabel=faviconPreview.querySelector('[data-favicon-status]');
    faviconImage.src=saved.faviconDataUrl||root.dataset.defaultFavicon;faviconImage.hidden=false;
    faviconLabel.textContent=saved.faviconDataUrl?'Current favicon':'Default Yarmouk favicon';faviconLabel.hidden=false;
    root.querySelectorAll('[data-plan-card]').forEach(card=>{const active=card.dataset.planCard===saved.subscription.currentPlan;card.classList.toggle('is-current',active);const request=card.querySelector('[data-plan-request]');if(request){const plan=card.querySelector('h3')?.textContent.trim()||card.dataset.planCard;const message=active?`Hello, I am the coordinator for Yarmouk University. I would like to discuss our current ${plan} plan.`:`Hello, I am the coordinator for Yarmouk University. I would like to request the ${plan} plan.`;request.href='https://wa.me/213540154968?text='+encodeURIComponent(message);request.textContent=card.dataset.planCard==='custom'?'Contact us about Custom':active?'Current plan':'Choose this plan';}});
    showTrial(saved.subscription);
  }
  function showTrial(subscription){
    const end=subscription.trialEnd,out=root.querySelector('[data-trial-remaining]'),banner=root.querySelector('[data-trial-banner]');
    if(!end){out.textContent='';if(banner)banner.hidden=true;return;}
    if(banner)banner.hidden=false;
    const days=Math.ceil((new Date(end+'T23:59:59')-new Date())/86400000);
    out.textContent=days>0?(days+' days remaining in the free trial'):days===0?'Free trial ends today':'Free trial ended';
  }
  function value(section) {
    if(section==='appearance')return {settingsSection:section,defaultTheme:get('defaultTheme').value,fontFamily:get('fontFamily').value};
    if(section==='homepage')return {settingsSection:section,firebaseDatabaseUrl:get('firebaseDatabaseUrl').value,homeTitle:get('homeTitle').value,homeDescription:get('homeDescription').value,metaKeywords:get('metaKeywords').value,headPlugins:get('headPlugins').value,bodyPlugins:get('bodyPlugins').value};
    return {settingsSection:'subscription',subscription:{currentPlan:get('currentPlan').value}};
  }
  get('defaultTheme').addEventListener('change',()=>{previewTheme(get('defaultTheme').value);markDirty();});
  get('fontFamily').addEventListener('change',()=>{previewFont(get('fontFamily').value);markDirty();});
  root.querySelectorAll('input:not([type="hidden"]):not([type="file"]),select,textarea').forEach(field=>field.addEventListener('input',markDirty));
  root.querySelectorAll('input:not([type="hidden"]),select,textarea').forEach(field=>field.addEventListener('change',markDirty));
  function previewLogoFile(file){if(!file)return;if(!/^image\/(png|jpeg|webp)$/.test(file.type)){feedback.textContent='Choose a PNG, JPEG or WebP image.';get('logoFile').value='';return;}if(file.size>1024*1024){feedback.textContent='Logo files must be 1 MB or smaller.';get('logoFile').value='';return;}const reader=new FileReader();reader.onload=()=>{const preview=root.querySelector('[data-logo-preview]'),img=preview.querySelector('img'),label=preview.querySelector('[data-logo-status]');img.src=reader.result;img.hidden=false;label.textContent='New logo preview · save to apply site-wide';label.hidden=false;feedback.textContent='Previewing the new logo. Save settings to apply it across the site.';markDirty();};reader.readAsDataURL(file);}
  get('logoFile').addEventListener('change',()=>previewLogoFile(get('logoFile').files[0]));
  const dropzone=root.querySelector('[data-logo-dropzone]');
  ['dragenter','dragover'].forEach(type=>dropzone.addEventListener(type,event=>{event.preventDefault();dropzone.classList.add('is-dragging');}));
  ['dragleave','drop'].forEach(type=>dropzone.addEventListener(type,event=>{event.preventDefault();dropzone.classList.remove('is-dragging');}));
  dropzone.addEventListener('drop',event=>previewLogoFile(event.dataTransfer.files[0]));
  function previewFaviconFile(file){
    selectedFaviconFile=file||null;selectedFaviconDataUrl=null;
    if(!file)return;
    const extension=(file.name.split('.').pop()||'').toLowerCase(),type=(file.type||'').toLowerCase();
    // Some browsers and mobile file pickers report application/octet-stream for
    // image files. Trust the selected extension as well as the reported MIME.
    const isPng=type==='image/png'||extension==='png',isIco=['image/x-icon','image/vnd.microsoft.icon'].includes(type)||extension==='ico';
    if(!isPng&&!isIco){feedback.textContent='Choose a PNG or ICO favicon.';get('faviconFile').value='';return;}
    if(file.size>10*1024*1024){feedback.textContent='Choose a favicon image smaller than 10 MB.';get('faviconFile').value='';return;}
    feedback.textContent='Optimizing favicon for preview…';
    readFaviconDataUrl(file).then(dataUrl=>{selectedFaviconDataUrl=dataUrl;const preview=root.querySelector('[data-favicon-preview]'),img=preview.querySelector('img'),label=preview.querySelector('[data-favicon-status]');img.src=dataUrl;img.hidden=false;label.textContent='Optimized PNG preview · save to apply site-wide';label.hidden=false;feedback.textContent='Favicon optimized and previewed. Save settings to apply it across the site.';markDirty();}).catch(error=>{selectedFaviconDataUrl=null;feedback.textContent=error.message||'The selected favicon could not be optimized.';});
  }
  let selectedFaviconDataUrl=null,selectedFaviconFile=null;
  function readFaviconDataUrl(file){
    return new Promise((resolve,reject)=>{
      if(file.size>10*1024*1024){reject(new Error('Choose a favicon image smaller than 10 MB.'));return;}
      const image=new Image(),objectUrl=URL.createObjectURL(file);
      image.onload=()=>{
        URL.revokeObjectURL(objectUrl);
        if(!image.naturalWidth||!image.naturalHeight||image.naturalWidth*image.naturalHeight>40000000){reject(new Error('This image is too large to process as a favicon.'));return;}
        const encode=(sizeIndex)=>{
          const size=[256,128,64][sizeIndex],scale=Math.min(1,size/Math.max(image.naturalWidth,image.naturalHeight)),canvas=document.createElement('canvas');
          canvas.width=Math.max(1,Math.round(image.naturalWidth*scale));canvas.height=Math.max(1,Math.round(image.naturalHeight*scale));
          const context=canvas.getContext('2d');if(!context){reject(new Error('Your browser could not prepare the favicon image.'));return;}
          context.clearRect(0,0,canvas.width,canvas.height);context.drawImage(image,0,0,canvas.width,canvas.height);
          canvas.toBlob(blob=>{
            if(!blob){reject(new Error('Your browser could not convert this image to a favicon.'));return;}
            if(blob.size>512*1024&&sizeIndex<2){encode(sizeIndex+1);return;}
            if(blob.size>512*1024){reject(new Error('The optimized favicon is still too large. Choose a simpler image.'));return;}
            const reader=new FileReader();reader.onload=()=>resolve(String(reader.result||''));reader.onerror=()=>reject(new Error('The optimized favicon could not be read.'));reader.readAsDataURL(blob);
          },'image/png');
        };
        encode(0);
      };
      image.onerror=()=>{URL.revokeObjectURL(objectUrl);reject(new Error('This PNG or ICO could not be opened as an image.'));};
      image.src=objectUrl;
    });
  }
  get('faviconFile').addEventListener('change',()=>previewFaviconFile(get('faviconFile').files[0]));
  const faviconDropzone=root.querySelector('[data-favicon-dropzone]');
  ['dragenter','dragover'].forEach(type=>faviconDropzone.addEventListener(type,event=>{event.preventDefault();faviconDropzone.classList.add('is-dragging');}));
  ['dragleave','drop'].forEach(type=>faviconDropzone.addEventListener(type,event=>{event.preventDefault();faviconDropzone.classList.remove('is-dragging');}));
  faviconDropzone.addEventListener('drop',event=>previewFaviconFile(event.dataTransfer.files[0]));
  root.querySelector('[data-save-settings]').addEventListener('click',()=>{
    const activeTab=root.querySelector('[data-settings-tab].is-active'),section=activeTab?activeTab.dataset.settingsTab:'appearance';
    feedback.textContent='Saving '+(section==='appearance'?'appearance':section==='homepage'?'homepage and integrations':'subscription')+' settings…';const payload=value(section),file=section==='appearance'?get('logoFile').files[0]:null,faviconFile=section==='appearance'?(get('faviconFile').files[0]||selectedFaviconFile):null;
    const submit=data=>{
      const body=new URLSearchParams({adminCsrf:get('adminCsrf').value,adminAction:'save-site-settings',adminPayload:JSON.stringify(data)});
      // FriendlyUrls permanently redirects *.aspx to the extensionless route. Posting
      // to the .aspx URL would turn this POST into a GET and discard the settings.
      fetch(location.href,{method:'POST',credentials:'same-origin',cache:'no-store',body})
        .then(response=>response.text().then(html=>({response,html})))
        .then(({response,html})=>{
          const documentCopy=new DOMParser().parseFromString(html,'text/html'),dataNode=documentCopy.getElementById('backend-data');
          if(!response.ok)throw new Error('The server returned an error ('+response.status+').');
          if(!dataNode)throw new Error('The server did not confirm the save. Check that you are still signed in, then try again.');
          const result=JSON.parse(dataNode.textContent||'{}');
          if(result.error){feedback.textContent=result.error;return;}
          if(!result.notice){feedback.textContent='The server response did not confirm that the settings were saved. Your edits are still here; please try again.';return;}
          feedback.textContent=result.notice;dirty.hidden=true;
          try{localStorage.setItem('yarmouk:portal-theme',get('defaultTheme').value);}catch(_){}
          window.setTimeout(()=>location.reload(),700);
        })
        .catch(error=>{feedback.textContent=error.message||'Settings could not be saved. Check your connection and try again.';});
    };
    const readFile=file=>new Promise((resolve,reject)=>{if(!file){resolve(null);return;}const reader=new FileReader();reader.onload=()=>resolve(reader.result);reader.onerror=()=>reject(new Error('The selected image could not be read.'));reader.readAsDataURL(file);});
    const faviconDataPromise=faviconFile?(selectedFaviconDataUrl?Promise.resolve(selectedFaviconDataUrl):readFaviconDataUrl(faviconFile)):Promise.resolve(null);
    Promise.all([readFile(file),faviconDataPromise]).then(([logoData,faviconData])=>{if(logoData)payload.logoDataUrl=logoData;if(faviconData)payload.faviconDataUrl=faviconData;submit(payload);}).catch(error=>{feedback.textContent=error.message;});
  });
  root.querySelector('[data-reset-settings]').addEventListener('click',()=>{get('logoFile').value='';get('faviconFile').value='';selectedFaviconFile=null;selectedFaviconDataUrl=null;fill(saved);previewTheme(originalTheme);if(originalFont)document.documentElement.style.setProperty('--uims-font-family',originalFont);else document.documentElement.style.removeProperty('--uims-font-family');const link=document.getElementById('uims-settings-font-preview');if(link)link.remove();dirty.hidden=true;feedback.textContent='Unsaved changes discarded.';});
  fill(readBackend());
  try{const notice=JSON.parse(document.getElementById('backend-data').textContent||'{}').notice;if(notice)feedback.textContent=notice;}catch(_){}
  const data=JSON.parse(document.getElementById('backend-data').textContent||'{}');if(data.error)feedback.textContent=data.error;
})();
