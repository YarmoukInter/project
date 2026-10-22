(() => {
    'use strict';
    function start() {
        const configNode = document.getElementById('co-notification-config');
        const config = configNode && JSON.parse(configNode.textContent);
        if (!config || !config.idToken || !config.databaseUrl) return;
        // Keep the short-lived, rules-scoped user token in memory only.
        configNode.remove();
        const header = document.querySelector('.yu-dashboard-main > header');
        if (!header) return;
        const bell = document.createElement('button');
        bell.type = 'button';
        bell.className = 'co-notifications cn-bell';
        bell.setAttribute('aria-label', 'Notifications');
        bell.setAttribute('aria-expanded', 'false');
        bell.setAttribute('aria-controls', 'co-notification-panel');
        bell.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.65" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9m-11 12a2 2 0 0 0 4 0"/></svg><span class="cn-badge" hidden></span>';
        const previous = header.querySelector('.co-notifications');
        if (previous) previous.replaceWith(bell);
        else {
            const account = header.querySelector('.yr-header-account, .co-account');
            if (account) account.before(bell);
            else header.append(bell);
        }
        const announcement = document.createElement('span');
        announcement.className = 'cn-announcement';
        announcement.setAttribute('role', 'status');
        announcement.setAttribute('aria-atomic', 'true');
        bell.after(announcement);
        const panel = document.createElement('section');
        panel.id = 'co-notification-panel';
        panel.className = 'cn-panel';
        panel.hidden = true;
        panel.setAttribute('aria-label', 'Notifications');
        panel.innerHTML = '<div class="cn-heading"><strong>Notifications</strong><button type="button" class="cn-close" aria-label="Close notifications">×</button></div><p class="cn-status">Connecting…</p><div class="cn-list"></div><a class="cn-all">View all messages</a>';
        document.body.append(panel);
        panel.querySelector('.cn-all').href = config.inboxUrl;
        const badge = bell.querySelector('.cn-badge');
        const status = panel.querySelector('.cn-status');
        const list = panel.querySelector('.cn-list');
        const date = new Intl.DateTimeFormat('en-GB', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit', timeZone: 'Asia/Amman' });
        let cache = Object.create(null), connected = false, terminal = false;
        function position() {
            const rect = bell.getBoundingClientRect();
            const top = Math.max(12, Math.min(rect.bottom + 10, window.innerHeight - 100));
            panel.style.top = top + 'px';
            panel.style.maxHeight = Math.max(80, window.innerHeight - top - 12) + 'px';
            const width = Math.min(360, window.innerWidth - 24);
            panel.style.right = Math.max(12, Math.min(window.innerWidth - width - 12, window.innerWidth - rect.right)) + 'px';
        }
        function close(restoreFocus = false) {
            panel.hidden = true;
            bell.setAttribute('aria-expanded', 'false');
            if (restoreFocus) bell.focus();
        }
        bell.addEventListener('click', () => {
            panel.hidden = !panel.hidden;
            bell.setAttribute('aria-expanded', String(!panel.hidden));
            if (!panel.hidden) { position(); panel.querySelector('.cn-close').focus(); }
        });
        panel.querySelector('.cn-close').addEventListener('click', () => close(true));
        document.addEventListener('click', event => {
            if (!bell.contains(event.target) && !panel.contains(event.target)) close();
        });
        document.addEventListener('keydown', event => {
            if (event.key === 'Escape' && !panel.hidden) { event.preventDefault(); close(true); }
        });
        document.addEventListener('focusin', event => {
            if (!bell.contains(event.target) && !panel.contains(event.target)) close();
        });
        window.addEventListener('resize', position);
        window.addEventListener('scroll', () => { if (!panel.hidden) position(); }, true);
        function messages() {
            return Object.entries(cache || {}).filter(([, value]) => value && typeof value === 'object')
                .map(([id, value]) => ({ ...value, id }))
                .sort((a, b) => (Number(b.createdAtUnixMs) || 0) - (Number(a.createdAtUnixMs) || 0));
        }
        function render() {
            const unread = messages().filter(message => !message.isRead && !message.isArchived);
            badge.hidden = !connected || !unread.length;
            badge.textContent = unread.length > 99 ? '99+' : String(unread.length);
            bell.setAttribute('aria-label', connected ? `Notifications, ${unread.length} unread messages` : 'Notifications, connection unavailable');
            status.textContent = connected ? (unread.length ? `${unread.length} unread message${unread.length === 1 ? '' : 's'}` : 'You’re all caught up') : status.textContent;
            if (announcement.textContent !== status.textContent) announcement.textContent = status.textContent;
            const focusedId = document.activeElement?.dataset.notificationId;
            list.replaceChildren();
            unread.slice(0, 8).forEach(message => {
                const link = document.createElement('a');
                link.className = 'cn-item';
                link.dataset.notificationId = message.id;
                const target = new URL(config.inboxUrl, location.href);
                target.searchParams.set('message', message.id);
                link.href = target.href;
                const sender = document.createElement('strong');
                sender.textContent = message.fullName || 'New contact message';
                const subject = document.createElement('span');
                subject.textContent = message.subject || 'Contact message';
                const time = document.createElement('small');
                const timestamp = Number(message.createdAtUnixMs);
                time.textContent = Number.isFinite(timestamp) && timestamp > 0 && timestamp <= 8640000000000000 ? date.format(timestamp) + ' · Amman' : '';
                link.append(sender, subject, time);
                list.append(link);
            });
            if (focusedId) {
                const restored = Array.from(list.children).find(link => link.dataset.notificationId === focusedId);
                (restored || panel.querySelector('.cn-all')).focus();
            }
        }
        // Firebase REST streaming sends an initial put and subsequent put/patch events.
        // Reject prototype keys before applying nested paths from the database.
        function replace(path, value) {
            const keys = path.split('/').filter(Boolean);
            if (keys.some(key => ['__proto__', 'prototype', 'constructor'].includes(key))) return;
            if (!keys.length) { cache = value || Object.create(null); return; }
            let node = cache;
            keys.slice(0, -1).forEach(key => {
                if (!node[key] || typeof node[key] !== 'object') node[key] = Object.create(null);
                node = node[key];
            });
            const key = keys[keys.length - 1];
            if (value === null) delete node[key];
            else node[key] = value;
        }
        const endpoint = new URL(config.databaseUrl.replace(/\/$/, '') + '/contactMessages.json');
        endpoint.searchParams.set('auth', config.idToken);
        const stream = new EventSource(endpoint.href);
        function update(event) {
            const change = JSON.parse(event.data);
            if (event.type === 'patch') Object.entries(change.data || {}).forEach(([key, value]) => replace(change.path + '/' + key, value));
            else replace(change.path, change.data);
            connected = true;
            render();
            document.dispatchEvent(new CustomEvent('coordinator:messages', { detail: messages() }));
        }
        stream.addEventListener('put', update);
        stream.addEventListener('patch', update);
        let checkingAccess = false;
        stream.onerror = async () => {
            if (terminal) return;
            connected = false;
            status.textContent = 'Connection interrupted. Reconnecting…';
            render();
            // EventSource hides HTTP status codes. Distinguish a denied initial
            // connection from a temporary network failure instead of retrying forever.
            if (checkingAccess) return;
            checkingAccess = true;
            const controller = new AbortController();
            const timeout = setTimeout(() => controller.abort(), 10000);
            try {
                const probe = new URL(endpoint);
                probe.searchParams.set('shallow', 'true');
                const response = await fetch(probe.href, { cache: 'no-store', signal: controller.signal });
                if (response.status === 401 || response.status === 403)
                    stop('Notifications unavailable. Sign in again; if this persists, check Firebase permissions.');
            } catch (_) { /* EventSource automatically retries network failures. */ }
            finally { clearTimeout(timeout); checkingAccess = false; }
        };
        function stop(message) {
            terminal = true;
            stream.close();
            cache = Object.create(null);
            connected = false;
            status.textContent = message;
            render();
        }
        stream.addEventListener('cancel', () => stop('Access denied. Check your coordinator account and Firebase rules.'));
        stream.addEventListener('auth_revoked', () => stop('Your session expired. Sign in again to receive notifications.'));
        window.addEventListener('pagehide', () => stream.close());
        // A page restored from the browser back/forward cache needs a fresh stream.
        window.addEventListener('pageshow', event => { if (event.persisted && !terminal) location.reload(); });
    }
    // Individual coordinator pages replace their header during startup.
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
    else start();
})();
