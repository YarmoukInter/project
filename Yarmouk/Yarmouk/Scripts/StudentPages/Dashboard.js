(function () {
    "use strict";

    function animateDashboard() {
        var ring = document.querySelector(".progress-ring");
        var ringNumber = document.querySelector(".progress-ring-center");
        var hoursElement = document.querySelector(".hours-value");
        var taskBar = document.querySelector(".task-progress-bar");
        var taskPercentage = document.querySelector(".progress-percentage");
        var completedTasks = document.querySelector(".progress-label span:last-child");
        var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        var numberAnimations = [];

        function addNumberAnimation(element, target, suffix) {
            if (!element || !Number.isFinite(target)) return;
            numberAnimations.push({ element: element, target: target, suffix: suffix || "" });
            element.textContent = "0" + (suffix || "");
        }

        document.querySelectorAll(".stat-number").forEach(function (element) {
            addNumberAnimation(element, Number(element.textContent.trim()));
        });

        var approvedHours = 0;
        var requiredHours = 0;
        if (hoursElement) {
            var match = hoursElement.textContent.match(/(\d+)\s*\/\s*(\d+)/);
            if (match) {
                approvedHours = Number(match[1]);
                requiredHours = Number(match[2]);
                var animatedHours = document.createElement("span");
                hoursElement.replaceChildren(animatedHours, document.createTextNode(" / " + requiredHours + " hours"));
                addNumberAnimation(animatedHours, approvedHours);
            }
        }

        var hoursPercentage = ringNumber ? parseFloat(ringNumber.textContent) || 0 : 0;
        addNumberAnimation(ringNumber, Math.round(hoursPercentage), "%");

        var taskTarget = taskBar ? Number(taskBar.value) : 0;
        var taskMaximum = taskBar ? Number(taskBar.max) : 1;
        if (taskBar && taskPercentage) addNumberAnimation(taskPercentage, Math.round(taskTarget / taskMaximum * 100), "%");
        if (completedTasks && taskBar) addNumberAnimation(completedTasks, taskTarget, " of " + taskMaximum + " tasks submitted");

        if (reduceMotion) {
            numberAnimations.forEach(function (item) {
                item.element.textContent = item.target + item.suffix;
            });
            return;
        }

        if (ring) ring.style.setProperty("--progress-angle", "0deg");
        if (taskBar) taskBar.value = 0;

        var startTime = null;
        function animateNumbers(timestamp) {
            if (startTime === null) startTime = timestamp;
            var progress = Math.min((timestamp - startTime) / 1400, 1);
            var easedProgress = 1 - Math.pow(1 - progress, 3);

            numberAnimations.forEach(function (item) {
                item.element.textContent = Math.round(item.target * easedProgress) + item.suffix;
            });
            if (ring) ring.style.setProperty("--progress-angle", (hoursPercentage / 100 * 360 * easedProgress) + "deg");
            if (taskBar) taskBar.value = taskTarget * easedProgress;
            if (progress < 1) window.requestAnimationFrame(animateNumbers);
        }
        window.requestAnimationFrame(animateNumbers);
    }

    function initializeDashboard() {
        animateDashboard();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeDashboard);
    } else {
        initializeDashboard();
    }
}());
