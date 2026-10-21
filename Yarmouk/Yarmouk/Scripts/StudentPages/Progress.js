(function () {
    "use strict";

    var ring = document.querySelector(".progress-ring");
    if (!ring || window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;

    var target = ring.style.getPropertyValue("--progress");
    if (!target || target === "0%") return;

    if (window.CSS && typeof window.CSS.registerProperty === "function") {
        try {
            window.CSS.registerProperty({ name: "--progress", syntax: "<percentage>", inherits: false, initialValue: "0%" });
        } catch (error) {
            // The property may already be registered by another page script.
        }
    }

    ring.style.setProperty("--progress", "0%");
    window.requestAnimationFrame(function () {
        window.requestAnimationFrame(function () {
            ring.style.setProperty("--progress", target);
        });
    });
}());
