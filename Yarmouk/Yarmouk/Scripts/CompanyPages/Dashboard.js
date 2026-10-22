// Interactions for CompanyPages/Dashboard.aspx only.
(function () {
    "use strict";

    function initializeDashboard() {
        const dashboard = document.querySelector(".company-dashboard");
        if (!dashboard) return;

        const date = dashboard.querySelector("#dashboardDate");
        if (date) {
            date.dateTime = new Date().toISOString().slice(0, 10);
            date.textContent = new Intl.DateTimeFormat("en-GB", {
                day: "numeric",
                month: "short",
                year: "numeric"
            }).format(new Date());
        }

        const tabs = Array.from(dashboard.querySelectorAll("[data-queue-tab]"));
        const panels = Array.from(dashboard.querySelectorAll("[data-queue-panel]"));

        function activateTab(tab, moveFocus) {
            const targetId = tab.dataset.queueTab;
            tabs.forEach(function (item) {
                const active = item === tab;
                item.classList.toggle("is-active", active);
                item.setAttribute("aria-selected", active ? "true" : "false");
                item.tabIndex = active ? 0 : -1;
            });
            panels.forEach(function (panel) {
                panel.hidden = panel.id !== targetId;
            });
            if (moveFocus) tab.focus();
            filterVisibleQueue();
        }

        tabs.forEach(function (tab, index) {
            tab.addEventListener("click", function () {
                activateTab(tab, false);
            });
            tab.addEventListener("keydown", function (event) {
                if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") return;
                event.preventDefault();
                const direction = event.key === "ArrowRight" ? 1 : -1;
                const nextIndex = (index + direction + tabs.length) % tabs.length;
                activateTab(tabs[nextIndex], true);
            });
        });

        const search = dashboard.querySelector("#queueSearch");

        function filterVisibleQueue() {
            const activePanel = panels.find(function (panel) { return !panel.hidden; });
            if (!activePanel) return;
            const query = search ? search.value.trim().toLocaleLowerCase() : "";
            const rows = Array.from(activePanel.querySelectorAll("[data-search-row]"));
            let visibleCount = 0;

            rows.forEach(function (row) {
                const matches = !query || row.textContent.toLocaleLowerCase().includes(query);
                row.hidden = !matches;
                if (matches) visibleCount += 1;
            });

            const emptyState = activePanel.querySelector("[data-empty-state]");
            if (emptyState) emptyState.hidden = visibleCount > 0;
        }

        if (search) search.addEventListener("input", filterVisibleQueue);
        if (tabs.length) activateTab(tabs.find(function (tab) { return tab.getAttribute("aria-selected") === "true"; }) || tabs[0], false);

        animateOverview(dashboard);
    }

    function animateOverview(dashboard) {
        const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        const cards = Array.from(dashboard.querySelectorAll(".metric-card"));
        const counters = Array.from(dashboard.querySelectorAll(".metric-value"));
        const bars = Array.from(dashboard.querySelectorAll(".seat-track > span, .progress-track > span"));

        if (reduceMotion) return;

        const counterTargets = counters.map(function (element) {
            const target = Number(element.textContent.trim());
            return { element: element, target: Number.isFinite(target) ? target : null };
        }).filter(function (item) { return item.target !== null; });

        const barTargets = bars.map(function (bar) {
            const targetWidth = bar.style.width || window.getComputedStyle(bar).width;
            bar.style.width = "0%";
            return { element: bar, targetWidth: targetWidth };
        });

        cards.forEach(function (card, index) {
            card.style.setProperty("--card-order", index);
        });
        dashboard.classList.add("motion-ready");

        window.requestAnimationFrame(function () {
            barTargets.forEach(function (item) {
                item.element.classList.add("is-animating");
                item.element.style.width = item.targetWidth;
            });
        });

        if (!counterTargets.length) return;
        const duration = 1250;
        let startedAt = null;

        function step(timestamp) {
            if (startedAt === null) startedAt = timestamp;
            const progress = Math.min((timestamp - startedAt) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);

            counterTargets.forEach(function (item) {
                item.element.textContent = String(Math.round(item.target * eased));
            });

            if (progress < 1) window.requestAnimationFrame(step);
        }

        window.requestAnimationFrame(step);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeDashboard);
    } else {
        initializeDashboard();
    }
})();
