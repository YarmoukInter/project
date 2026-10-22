(() => {
    'use strict';
    const root = document.querySelector('[data-contact-inbox]');
    if (!root) return;
    document.body.classList.add('co-roster-page','co-inbox-page');
    document.title = 'Contact messages — Yarmouk University';
    const header = document.querySelector('.yu-dashboard-main > header');
    if (header) header.innerHTML = '<a class="yr-header-brand" href="Dashboard.aspx"><strong>Yarmouk University</strong><span>Internship administration</span></a><a class="yr-header-account" href="Account.aspx" aria-label="My coordinator account"><span aria-hidden="true">YU</span><span>University coordinator</span></a>';

    const state = window.CoordinatorBackend.state();
    let messages = state.data || [];
    const read = new Set(messages.filter(m => m.isRead).map(m => m.id));
    const archived = new Set(messages.filter(m => m.isArchived).map(m => m.id));
    let saved = {};
    try { saved = JSON.parse(sessionStorage.getItem('coordinatorInboxView') || '{}'); } catch (_) { }
    let view = ['inbox','unread','archived'].includes(saved.view) ? saved.view : 'inbox';
    const notificationId = new URLSearchParams(location.search).get('message');
    if (notificationId) {
        view = 'inbox';
        // Consume the deep link once so a later "Mark unread" stays unread.
        const cleanUrl = new URL(location.href);
        cleanUrl.searchParams.delete('message');
        history.replaceState(null, '', cleanUrl.href);
    }
    let selectedId = notificationId || saved.id || null, deletingId = null;
    function persist(action, id, value) {
        sessionStorage.setItem('coordinatorInboxView', JSON.stringify({view,id}));
        window.CoordinatorBackend.submit(action, {id,value});
    }
    const list = root.querySelector('[data-message-list]');
    const reader = root.querySelector('[data-message-reader]');
    const sort = root.querySelector('[data-message-sort]');
    const feedback = root.querySelector('[data-inbox-feedback]');
    const deleteDialog = root.querySelector('[data-message-delete]');
    const compact = matchMedia('(max-width:740px)');
    const reduceMotion = matchMedia('(prefers-reduced-motion:reduce)');
    let lastOpenedId = null;
    const escape = value => String(value).replace(/[&<>"']/g, ch => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[ch]));
    const icons = {
        mail:'<rect x="3" y="5" width="18" height="14" rx="2"/><path d="m3 6 9 7 9-7"/>',
        archive:'<rect x="3" y="3" width="18" height="5" rx="1"/><path d="M5 8v13h14V8m-10 4h6"/>',
        restore:'<path d="M4 10a8 8 0 1 1 2 8M4 4v6h6"/>',
        trash:'<path d="M3 6h18M9 6V3h6v3M5 6l1 15h12l1-15M10 10v7m4-7v7"/>',
        reply:'<path d="m9 4-7 7 7 7M2 11h11a8 8 0 0 1 8 8"/>',
        back:'<path d="m15 5-7 7 7 7"/>'
    };
    const icon = name => `<svg viewBox="0 0 24 24" aria-hidden="true">${icons[name]}</svg>`;
    const shortDate = new Intl.DateTimeFormat('en-GB',{day:'numeric',month:'short',timeZone:'Asia/Amman'});
    const fullDate = new Intl.DateTimeFormat('en-GB',{day:'numeric',month:'short',year:'numeric',hour:'2-digit',minute:'2-digit',timeZone:'Asia/Amman'});
    const activeMessages = () => messages.filter(message => !archived.has(message.id));
    function filteredMessages() {
        return messages.filter(message => {
            const belongs = view === 'archived' ? archived.has(message.id) : !archived.has(message.id) && (view !== 'unread' || !read.has(message.id));
            return belongs;
        }).sort((a,b) => sort.value === 'oldest' ? a.createdAtUnixMs - b.createdAtUnixMs : b.createdAtUnixMs - a.createdAtUnixMs);
    }

    function render() {
        const inbox = activeMessages();
        const unread = inbox.filter(message => !read.has(message.id)).length;
        const counts = {inbox:inbox.length,unread,archived:messages.length-inbox.length};
        root.querySelectorAll('[data-count]').forEach(span => { span.textContent = counts[span.dataset.count]; });
        root.querySelector('[data-unread-summary]').textContent = `${unread} unread`;
        root.style.setProperty('--cm-view-index', String(['inbox','unread','archived'].indexOf(view)));
        root.querySelectorAll('[data-view]').forEach(button => button.setAttribute('aria-pressed',String(button.dataset.view === view)));
        const filtered = filteredMessages();
        list.innerHTML = filtered.length ? filtered.map(message => `<button type="button" class="cm-message${read.has(message.id) ? '' : ' is-unread'}" data-message-id="${escape(message.id)}" aria-current="${message.id === selectedId}" aria-label="${escape(message.fullName + ': ' + message.subject + (read.has(message.id) ? '' : ', unread'))}">
            <span class="cm-message-top"><span class="cm-sender">${read.has(message.id) ? '' : '<span class="cm-unread-dot" aria-hidden="true"></span>'}<span>${escape(message.fullName)}</span></span><time class="cm-message-time" datetime="${new Date(message.createdAtUnixMs).toISOString()}">${shortDate.format(message.createdAtUnixMs)}</time></span>
            <strong class="cm-message-subject">${escape(message.subject)}</strong><span class="cm-message-snippet">${escape(message.message.replace(/\n+/g,' '))}</span></button>`).join('') : `<div class="cm-list-empty"><strong>${view === 'archived' ? 'No archived messages' : view === 'unread' ? 'You’re all caught up' : 'Your inbox is empty'}</strong><span>${view === 'unread' ? 'There are no unread messages to review.' : view === 'archived' ? 'Messages you archive will appear here.' : 'Messages from the contact form will appear here.'}</span></div>`;
        const message = messages.find(item => item.id === selectedId);
        if (!message) {
            reader.innerHTML = `<div class="cm-reader-empty">${icon('mail')}<h3>Select a message</h3><p>Choose a message from the list to read it and manage your inbox.</p></div>`;
            return;
        }
        const initials = message.fullName.split(/\s+/).slice(0,2).map(name => name[0]).join('');
        const isArchived = archived.has(message.id);
        const replySubject = message.subject.startsWith('Re:') ? message.subject : `Re: ${message.subject}`;
        reader.innerHTML = `
            <div class="cm-reader-actions">
                <button type="button" class="cm-action cm-back" data-message-back>${icon('back')}<span>Messages</span></button>
                <button type="button" class="cm-action" data-message-action="read" aria-label="${read.has(message.id) ? 'Mark unread' : 'Mark read'}" title="${read.has(message.id) ? 'Mark unread' : 'Mark read'}">${icon('mail')}<span>${read.has(message.id) ? 'Mark unread' : 'Mark read'}</span></button>
                <button type="button" class="cm-action" data-message-action="archive" aria-label="${isArchived ? 'Move to inbox' : 'Archive'}" title="${isArchived ? 'Move to inbox' : 'Archive'}">${icon(isArchived ? 'restore' : 'archive')}<span>${isArchived ? 'Move to inbox' : 'Archive'}</span></button>
                <button type="button" class="cm-action cm-action-danger" data-message-action="delete" aria-label="Delete" title="Delete">${icon('trash')}<span>Delete</span></button>
            </div>
            <div class="cm-reader-content">
            <h3 tabindex="-1" data-reader-title>${escape(message.subject)}</h3>
            <div class="cm-reader-meta"><span class="cm-avatar" aria-hidden="true">${escape(initials)}</span><span class="cm-reader-person"><strong>${escape(message.fullName)}</strong><a href="mailto:${encodeURIComponent(message.email)}">${escape(message.email)}</a></span><time class="cm-reader-date" datetime="${new Date(message.createdAtUnixMs).toISOString()}">${fullDate.format(message.createdAtUnixMs)} (Amman)</time></div>
            <div class="cm-message-text">${message.message.split(/\n\n+/).map(paragraph => `<p>${escape(paragraph).replace(/\n/g,'<br>')}</p>`).join('')}</div>
            <div class="cm-reply-row"><a class="yr-button yr-primary" href="mailto:${encodeURIComponent(message.email)}?subject=${encodeURIComponent(replySubject)}">${icon('reply')}Reply by email</a><span>Opens your email app.</span></div>
            </div>`;
    }

    root.addEventListener('click', event => {
        if (event.target.closest('[data-message-back]')) {
            root.classList.remove('is-reading');
            (list.querySelector(`[data-message-id="${lastOpenedId}"]`) || list.querySelector('button') || root.querySelector('[data-view]')).focus({preventScroll:true});
            return;
        }
        const filter = event.target.closest('[data-view]');
        if (filter) { view = filter.dataset.view; selectedId = null; root.classList.remove('is-reading'); feedback.textContent = ''; render(); return; }
        const row = event.target.closest('[data-message-id]');
        if (row) {
            selectedId = row.dataset.messageId;
            lastOpenedId = selectedId;
            if (!read.has(selectedId)) { persist('read', selectedId, true); return; }
            render();
            root.classList.add('is-reading');
            reader.querySelector('[data-reader-title]')?.focus({preventScroll:true});
            if (!reduceMotion.matches) reader.animate([{opacity:.65},{opacity:1}],{duration:140,easing:'ease-out'});
            return;
        }
        const action = event.target.closest('[data-message-action]')?.dataset.messageAction;
        const message = messages.find(item => item.id === selectedId);
        if (!action || !message) return;
        if (action === 'read') {
            persist('read', selectedId, !read.has(selectedId));
        } else if (action === 'archive') {
            persist('archive', selectedId, !archived.has(selectedId));
        } else if (action === 'delete') {
            deletingId = selectedId;
            root.querySelector('[data-delete-description]').textContent = `Permanently delete “${message.subject}”? This cannot be undone.`;
            deleteDialog.showModal();
            deleteDialog.querySelector('[data-delete-cancel]:not(.yr-close)').focus();
        }
    });
    sort.addEventListener('change', render);
    root.querySelectorAll('[data-delete-cancel]').forEach(button => button.addEventListener('click', () => deleteDialog.close()));
    root.querySelector('[data-delete-confirm]').addEventListener('click', () => {
        if (!deletingId) return;
        persist('delete', deletingId, false);
    });
    deleteDialog.addEventListener('close', () => (reader.querySelector('[data-message-action="delete"]') || list.querySelector('button') || root.querySelector('[data-view]')).focus());
    compact.addEventListener('change', () => {
        // Move focus out of whichever pane becomes hidden after a window resize.
        if (compact.matches && reader.contains(document.activeElement) && !root.classList.contains('is-reading')) (list.querySelector('button') || root.querySelector('[data-view]')).focus({preventScroll:true});
    });
    feedback.textContent = state.error || state.notice || '';
    document.addEventListener('coordinator:messages', event => {
        messages = event.detail;
        read.clear();
        archived.clear();
        messages.forEach(message => {
            if (message.isRead) read.add(message.id);
            if (message.isArchived) archived.add(message.id);
        });
        if (selectedId && !messages.some(message => message.id === selectedId)) {
            selectedId = null;
            root.classList.remove('is-reading');
        }
        render();
    });
    if (!state.data) {
        list.innerHTML = '<p class="cm-list-empty">Unable to load messages. Check the error below and reload.</p>';
        root.querySelector('[data-unread-summary]').textContent = 'Unavailable';
        reader.innerHTML = '';
    } else {
        if (notificationId && messages.some(message => message.id === notificationId && !message.isRead)) {
            persist('read', notificationId, true);
            return;
        }
        render();
        if (selectedId && messages.some(message => message.id === selectedId)) root.classList.add('is-reading');
    }
})();
