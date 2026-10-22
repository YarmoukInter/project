// Interactions for AcademicPages/Dashboard.aspx only.
(function () {
    "use strict";

    function initializeDashboard() {
        var dashboard = document.querySelector(".academic-dashboard");
        if (!dashboard) return;

        var date = dashboard.querySelector("#dashboardDate");
        if (date) {
            var now = new Date();
            var month = String(now.getMonth() + 1).padStart(2, "0");
            var day = String(now.getDate()).padStart(2, "0");
            date.dateTime = now.getFullYear() + "-" + month + "-" + day;
            date.textContent = new Intl.DateTimeFormat("en-GB", {
                weekday: "short",
                day: "numeric",
                month: "short",
                year: "numeric"
            }).format(new Date());
        }

        var search = dashboard.querySelector("#studentSearch");
        var rows = Array.prototype.slice.call(dashboard.querySelectorAll("[data-student-row]"));
        var emptyState = dashboard.querySelector("#studentSearchEmpty");

        function filterStudents() {
            if (!search) return;
            var query = search.value.trim().toLocaleLowerCase();
            var visibleCount = 0;
            rows.forEach(function (row) {
                var visible = !query || row.textContent.toLocaleLowerCase().indexOf(query) !== -1;
                row.hidden = !visible;
                if (visible) visibleCount += 1;
            });
            if (emptyState) emptyState.hidden = visibleCount > 0;
        }

        if (search) search.addEventListener("input", filterStudents);

        var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        var metrics = Array.prototype.slice.call(dashboard.querySelectorAll(".academic-metric"));
        var counters = Array.prototype.slice.call(dashboard.querySelectorAll(".metric-value"));
        var progressBars = Array.prototype.slice.call(dashboard.querySelectorAll(".training-track span"));
        if (reduceMotion) return;

        metrics.forEach(function (metric, index) {
            metric.style.setProperty("--metric-order", index);
        });
        dashboard.classList.add("motion-ready");

        var countTargets = counters.map(function (element) {
            var target = Number(element.textContent.trim());
            return { element: element, target: Number.isFinite(target) ? target : null };
        }).filter(function (item) { return item.target !== null; });

        countTargets.forEach(function (item) { item.element.textContent = "0"; });
        progressBars.forEach(function (bar) {
            var targetWidth = bar.style.width || window.getComputedStyle(bar).width;
            bar.style.width = "0%";
            window.requestAnimationFrame(function () { bar.style.width = targetWidth; });
        });

        if (!countTargets.length) return;
        var startedAt = null;
        function animateCounts(timestamp) {
            if (startedAt === null) startedAt = timestamp;
            var progress = Math.min((timestamp - startedAt) / 1000, 1);
            var eased = 1 - Math.pow(1 - progress, 3);
            countTargets.forEach(function (item) {
                item.element.textContent = String(Math.round(item.target * eased));
            });
            if (progress < 1) window.requestAnimationFrame(animateCounts);
        }
        window.requestAnimationFrame(animateCounts);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeDashboard);
    } else {
        initializeDashboard();
    }
}());
