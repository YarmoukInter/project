(function () {
    "use strict";

    function initializeStatCards() {
        var grid = document.querySelector(".stats-grid");
        var cards = Array.prototype.slice.call(document.querySelectorAll(".stat-card"));
        if (!grid || cards.length === 0) return;

        function selectCard(selectedCard) {
            var wasSelected = selectedCard.classList.contains("is-selected");
            cards.forEach(function (card) {
                card.classList.remove("is-selected");
                card.setAttribute("aria-pressed", "false");
            });
            grid.classList.remove("has-selection");

            if (!wasSelected) {
                selectedCard.classList.add("is-selected");
                selectedCard.setAttribute("aria-pressed", "true");
                grid.classList.add("has-selection");
            }
        }

        cards.forEach(function (card) {
            card.setAttribute("tabindex", "0");
            card.setAttribute("role", "button");
            card.setAttribute("aria-pressed", "false");
            card.addEventListener("click", function () { selectCard(card); });
            card.addEventListener("keydown", function (event) {
                if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    selectCard(card);
                }
            });
        });
    }

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

        var hoursPercentage = requiredHours > 0
            ? Math.min(100, Math.max(0, approvedHours / requiredHours * 100))
            : (ringNumber ? parseFloat(ringNumber.textContent) || 0 : 0);
        addNumberAnimation(ringNumber, Math.round(hoursPercentage), "%");

        var taskTarget = taskBar ? Number(taskBar.value) : 0;
        var taskMaximum = taskBar ? Number(taskBar.max) : 1;
        if (taskBar && taskPercentage) addNumberAnimation(taskPercentage, Math.round(taskTarget / taskMaximum * 100), "%");
        if (completedTasks && taskBar) addNumberAnimation(completedTasks, taskTarget, " of " + taskMaximum);

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

    function initializeRevealEffects() {
        if (window.matchMedia("(prefers-reduced-motion: reduce)").matches || !("IntersectionObserver" in window)) return;
        var elements = document.querySelectorAll(".stat-card, .panel, .placement-strip");
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                entry.target.classList.add("is-visible");
                observer.unobserve(entry.target);
            });
        }, { threshold: 0.1 });

        elements.forEach(function (element) {
            element.classList.add("reveal-ready");
            element.addEventListener("animationend", function (event) {
                if (event.animationName === "dashboard-reveal") {
                    element.classList.remove("reveal-ready", "is-visible");
                }
            });
            observer.observe(element);
        });
    }

    function initializeDashboard() {
        initializeStatCards();
        animateDashboard();
        initializeRevealEffects();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeDashboard);
    } else {
        initializeDashboard();
    }
}());
