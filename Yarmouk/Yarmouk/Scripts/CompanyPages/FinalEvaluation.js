(function () {
    'use strict';
    var dialog = document.getElementById('evaluationDialog');
    var host = document.getElementById('evaluationDialogBody');
    var opener = null;
    var requestId = 0;
    if (!dialog || !host) return;

    document.documentElement.classList.add('evaluation-list-view');

    function syncPostbackState(responseDocument, actionUrl) {
        var currentForm = document.forms[0];
        var responseForm = responseDocument.forms[0];
        if (!currentForm || !responseForm) return;
        currentForm.action = actionUrl;
        Array.prototype.forEach.call(responseForm.querySelectorAll('input[type="hidden"]'), function (source) {
            if (!/^__(VIEWSTATE|VIEWSTATEGENERATOR|VIEWSTATEENCRYPTED|EVENTVALIDATION)/.test(source.name)) return;
            var matches = currentForm.querySelectorAll('input[type="hidden"][name="' + source.name + '"]');
            if (matches.length) Array.prototype.forEach.call(matches, function (target, index) {
                if (source.name === '__VIEWSTATE' && matches.length > 1) {
                    var responseParts = responseForm.querySelectorAll('input[type="hidden"][name="__VIEWSTATE"]');
                    target.value = responseParts[index] ? responseParts[index].value : '';
                } else target.value = source.value;
            });
            else {
                var clone = source.cloneNode(true);
                clone.value = source.value;
                currentForm.appendChild(clone);
            }
        });
    }

    function showLoading() {
        host.innerHTML = '<div class="evaluation-loading" role="status"><span class="evaluation-loading__spinner" aria-hidden="true"></span><p>Loading trainee evaluation…</p></div>';
    }

    function updateRatingRow(radio) {
        if (!radio || !radio.matches('input[type="radio"]')) return;
        var row = radio.closest('.criteria-row');
        if (!row) return;
        row.querySelectorAll('.rating-option').forEach(function (option) {
            var choice = option.querySelector('input[type="radio"]');
            option.classList.toggle('is-selected', Boolean(choice && choice.checked));
        });
    }

    function updateSubmitState(root) {
        var button = root.querySelector('#MainContent_SubmitEvaluation');
        if (!button) return;
        var academicAssigned = button.dataset.academicSupervisorAssigned === 'true';
        var locked = button.dataset.evaluationLocked === 'true';
        var selectedCriteria = new Set();
        root.querySelectorAll('input[type="radio"][name^="companyCriterion_"]:checked').forEach(function (radio) {
            var match = /^companyCriterion_(\d+)$/.exec(radio.name);
            if (match) selectedCriteria.add(match[1]);
        });
        var score = root.querySelector('#CompanyScore');
        var validScore = Boolean(score && score.value !== '' && score.validity.valid);
        button.disabled = !academicAssigned || locked || selectedCriteria.size !== 18 || !validScore;
    }

    document.addEventListener('click', function (event) {
        var rating = event.target.closest('label.rating-option');
        if (rating) {
            var radio = rating.querySelector('input[type="radio"]');
            if (radio && !radio.disabled && !radio.checked) {
                radio.checked = true;
                radio.dispatchEvent(new Event('change', { bubbles: true }));
            }
            updateRatingRow(radio);
            updateSubmitState(dialog);
        }

        if (event.target.closest('[data-evaluation-print]')) {
            event.preventDefault();
            document.documentElement.classList.add('evaluation-print-mode');
            window.addEventListener('afterprint', function clearPrintMode() {
                document.documentElement.classList.remove('evaluation-print-mode');
            }, { once: true });
            window.print();
            return;
        }

        var link = event.target.closest('[data-evaluation-open]');
        if (link) {
            event.preventDefault();
            opener = link;
            var thisRequest = ++requestId;
            var targetUrl = new URL(link.href, location.href);
            showLoading();
            if (!dialog.open) dialog.showModal();
            fetch(targetUrl.href, { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
                .then(function (response) {
                    if (!response.ok) throw new Error('Evaluation could not be loaded.');
                    return response.text();
                })
                .then(function (html) {
                    if (thisRequest !== requestId || !dialog.open) return;
                    var parsed = new DOMParser().parseFromString(html, 'text/html');
                    var nextWorkspace = parsed.querySelector('.evaluation-main');
                    if (!nextWorkspace || !nextWorkspace.querySelector('.evaluation-workspace')) throw new Error('Evaluation form was not found.');
                    syncPostbackState(parsed, targetUrl.href);
                    host.replaceChildren(nextWorkspace);
                    if (window.uimsInitEvaluationTabs) window.uimsInitEvaluationTabs(nextWorkspace);
                    if (window.uimsInitStudentPhotos) window.uimsInitStudentPhotos(nextWorkspace);
                    updateSubmitState(nextWorkspace);
                })
                .catch(function () {
                    if (thisRequest !== requestId) return;
                    dialog.close();
                    location.href = targetUrl.href;
                });
            return;
        }
        if (event.target.closest('[data-close-evaluation]') || event.target === dialog) dialog.close();
    });

    document.addEventListener('change', function (event) {
        if (event.target.matches('input[type="radio"][name^="companyCriterion_"]')) {
            updateRatingRow(event.target);
            updateSubmitState(dialog);
        }
    });
    document.addEventListener('input', function (event) {
        if (event.target.matches('#CompanyScore')) updateSubmitState(dialog);
    });

    dialog.addEventListener('close', function () {
        requestId++;
        if (opener) opener.focus();
    });

    if (new URLSearchParams(location.search).has('placementId')) {
        var initialWorkspace = document.querySelector('.evaluation-layout > .evaluation-main');
        if (initialWorkspace && initialWorkspace.querySelector('.evaluation-workspace')) {
            host.replaceChildren(initialWorkspace);
            updateSubmitState(initialWorkspace);
            dialog.showModal();
        }
    }
}());

(function () {
    'use strict';
    function initEvaluationTabs(root) {
        root = root || document;
        var tabs = root.querySelectorAll('[data-evaluation-tab]');
        var sections = root.querySelectorAll('[data-evaluation-section]');
        var activeTabField = root.querySelector('#ActiveEvaluationTab');
        if (!tabs.length || !sections.length || tabs[0].dataset.tabsInitialized) return;

        function activateTab(tab, moveFocus) {
            var selectedTab = tab.dataset.evaluationTab;
            tabs.forEach(function (item) {
                var active = item === tab;
                item.classList.toggle('is-active', active);
                item.setAttribute('aria-selected', String(active));
                item.tabIndex = active ? 0 : -1;
            });
            sections.forEach(function (section) { section.hidden = section.dataset.evaluationSection !== selectedTab; });
            if (activeTabField) activeTabField.value = selectedTab;
            if (moveFocus) tab.focus();
        }

        tabs.forEach(function (tab, index) {
            tab.addEventListener('click', function () { activateTab(tab, false); });
            tab.addEventListener('keydown', function (event) {
                var next = index;
                if (event.key === 'ArrowRight') next = (index + 1) % tabs.length;
                else if (event.key === 'ArrowLeft') next = (index - 1 + tabs.length) % tabs.length;
                else if (event.key === 'Home') next = 0;
                else if (event.key === 'End') next = tabs.length - 1;
                else return;
                event.preventDefault();
                activateTab(tabs[next], true);
            });
        });
        tabs[0].dataset.tabsInitialized = 'true';
        var requested = activeTabField && activeTabField.value;
        var initial = Array.prototype.find.call(tabs, function (tab) { return tab.dataset.evaluationTab === requested; });
        activateTab(initial || tabs[0], false);
        window.uimsSelectEvaluationTab = function (name) {
            var selected = Array.prototype.find.call(tabs, function (tab) { return tab.dataset.evaluationTab === name; });
            if (selected) activateTab(selected, false);
        };
    }
    window.uimsInitEvaluationTabs = initEvaluationTabs;
    initEvaluationTabs(document);
}());
