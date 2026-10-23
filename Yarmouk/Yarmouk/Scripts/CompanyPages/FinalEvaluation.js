(function () {
    'use strict';
    var dialog = document.getElementById('evaluationDialog');
    var workspace = document.querySelector('.evaluation-main');
    var host = document.getElementById('evaluationDialogBody');
    var selected = document.getElementById('SelectedPlacementId');
    if (!dialog || !workspace || !host) return;
    host.appendChild(workspace);
    document.documentElement.classList.add('evaluation-list-view');
    var opener = null;
    document.addEventListener('click', function (event) {
        var link = event.target.closest('[data-evaluation-open]');
        if (link) {
            var placement = new URL(link.href, location.href).searchParams.get('placementId');
            if (selected && placement === selected.value) {
                event.preventDefault();
                opener = link;
                dialog.showModal();
            }
        }
        if (event.target.closest('[data-close-evaluation]') || event.target === dialog) dialog.close();
    });
    dialog.addEventListener('close', function () {
        if (opener) opener.focus();
    });
    if (new URLSearchParams(location.search).has('placementId') && workspace.querySelector('.evaluation-workspace')) {
        dialog.showModal();
    }
}());
(function () {
    "use strict";

    const tabs = document.querySelectorAll("[data-evaluation-tab]");
    const sections = document.querySelectorAll("[data-evaluation-section]");
    const activeTabField = document.getElementById("ActiveEvaluationTab");

    if (!tabs.length || !sections.length) return;

    function activateTab(tab, moveFocus) {
        const selectedTab = tab.dataset.evaluationTab;

        tabs.forEach(function (item) {
            const isActive = item === tab;

            item.classList.toggle("is-active", isActive);
            item.setAttribute("aria-selected", String(isActive));
            item.tabIndex = isActive ? 0 : -1;
        });

        sections.forEach(function (section) {
            section.hidden = section.dataset.evaluationSection !== selectedTab;
        });

        if (activeTabField) {
            activeTabField.value = selectedTab;
        }

        if (moveFocus) tab.focus();
    }

    tabs.forEach(function (tab, index) {
        tab.addEventListener("click", function () {
            activateTab(tab, false);
        });

        tab.addEventListener("keydown", function (event) {
            let nextIndex = index;

            if (event.key === "ArrowRight") {
                nextIndex = (index + 1) % tabs.length;
            } else if (event.key === "ArrowLeft") {
                nextIndex = (index - 1 + tabs.length) % tabs.length;
            } else if (event.key === "Home") {
                nextIndex = 0;
            } else if (event.key === "End") {
                nextIndex = tabs.length - 1;
            } else {
                return;
            }

            event.preventDefault();
            activateTab(tabs[nextIndex], true);
        });
    });

    const requestedTab = activeTabField && activeTabField.value;
    const initialTab = Array.from(tabs).find(function (tab) {
        return tab.dataset.evaluationTab === requestedTab;
    });

    activateTab(initialTab || tabs[0], false);
})();

