// Profile preview interaction for AcademicPages/Account.aspx only.
(function () {
    "use strict";

    function initializeProfilePreview() {
        var nameInput = document.getElementById("FullNameInput");
        var namePreview = document.getElementById("profileNamePreview");
        var avatar = document.querySelector(".profile-avatar");
        if (!nameInput || !namePreview || !avatar) return;

        function updateNamePreview() {
            var name = nameInput.value.trim();
            var parts = name.split(/\s+/).filter(Boolean);
            namePreview.textContent = name || "Academic Supervisor";
            if (!parts.length) {
                avatar.textContent = "AS";
            } else if (parts.length === 1) {
                avatar.textContent = parts[0].charAt(0).toUpperCase();
            } else {
                avatar.textContent = (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
            }
        }

        nameInput.addEventListener("input", updateNamePreview);
        updateNamePreview();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeProfilePreview);
    } else {
        initializeProfilePreview();
    }
}());
