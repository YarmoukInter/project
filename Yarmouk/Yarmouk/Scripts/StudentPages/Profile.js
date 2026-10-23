(function () {
    "use strict";
    var tabs = Array.prototype.slice.call(document.querySelectorAll("[role=tab][aria-controls^=panel-]"));
    var groups = Array.prototype.slice.call(document.querySelectorAll("[data-skill-group]"));
    var activeTabField = document.getElementById("hidActiveProfileTab");

    function hidden(group) {
        return group.querySelector("input[type=hidden]");
    }

    function values(group) {
        try {
            var result = JSON.parse(hidden(group).value || "[]");
            return Array.isArray(result) ? result : [];
        } catch (_) { return []; }
    }

    function save(group, items) {
        var field = hidden(group);
        field.value = JSON.stringify(items);
        var list = group.querySelector("[data-skill-list]");
        list.replaceChildren();
        items.forEach(function (name, index) {
            var chip = document.createElement("span");
            chip.className = "skill-chip";
            chip.dir = "auto";
            chip.appendChild(document.createTextNode(name));
            var remove = document.createElement("button");
            remove.type = "button";
            remove.dataset.skillRemove = String(index);
            remove.setAttribute("aria-label", "Remove " + name);
            remove.textContent = "×";
            chip.appendChild(remove);
            list.appendChild(chip);
        });
    }

    function showTab(tab) {
        var panelId = tab.getAttribute("aria-controls");
        if (activeTabField) activeTabField.value = panelId.replace("panel-", "");
        tabs.forEach(function (item) {
            var selected = item === tab;
            item.setAttribute("aria-selected", selected ? "true" : "false");
            item.tabIndex = selected ? 0 : -1;
            document.getElementById(item.getAttribute("aria-controls")).hidden = !selected;
        });
        if (history.replaceState) history.replaceState(null, "", "#" + panelId.replace("panel-", ""));
    }

    tabs.forEach(function (tab, index) {
        tab.addEventListener("click", function () { showTab(tab); });
        tab.addEventListener("keydown", function (event) {
            if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") return;
            event.preventDefault();
            var step = event.key === "ArrowRight" ? 1 : -1;
            var next = tabs[(index + step + tabs.length) % tabs.length];
            showTab(next);
            next.focus();
        });
    });

    groups.forEach(function (group) { save(group, values(group)); });

    function addSkill(group) {
        var items = values(group);
        var error = group.querySelector("[data-skill-error]");
        var input = group.querySelector("[data-skill-input]");
        var value = input.value.trim();
        error.textContent = "";
        if (!value) return true;
        if (value.length > 40) { error.textContent = "Keep each skill to 40 characters or fewer."; return false; }
        if (items.some(function (item) { return item.toLowerCase() === value.toLowerCase(); })) { error.textContent = "That skill is already in the list."; return false; }
        if (items.length >= 20) { error.textContent = "You can add up to 20 skills here."; return false; }
        items.push(value);
        input.value = "";
        save(group, items);
        return true;
    }

    document.addEventListener("click", function (event) {
        var group = event.target.closest("[data-skill-group]");
        if (!group) return;
        var items = values(group);
        var error = group.querySelector("[data-skill-error]");
        var remove = event.target.closest("[data-skill-remove]");
        if (remove) {
            items.splice(Number(remove.dataset.skillRemove), 1);
            error.textContent = "";
            save(group, items);
            return;
        }
        if (!event.target.closest("[data-skill-add]")) return;
        addSkill(group);
        group.querySelector("[data-skill-input]").focus();
    });

    document.addEventListener("keydown", function (event) {
        if (event.target.matches("[data-skill-input]") && event.key === "Enter") {
            event.preventDefault();
            event.target.closest("[data-skill-group]").querySelector("[data-skill-add]").click();
        }
    });

    var saveSkillsButton = document.getElementById("btnSaveSkills");
    if (saveSkillsButton) saveSkillsButton.addEventListener("click", function (event) {
        var valid = groups.every(addSkill);
        if (!valid) event.preventDefault();
    });

    var profileImageInput = document.getElementById("profileImageUpload");
    var profileImageFileName = document.getElementById("profileImageFileName");
    var profilePreview = document.querySelector(".profile-avatar-image");
    var profileInitial = document.querySelector(".profile-avatar-wrap .profile-avatar");
    var profileSave = document.querySelector(".profile-photo-save");
    var profileOriginalSrc = profilePreview ? profilePreview.getAttribute("src") : "";
    var profileOriginalDisplay = profilePreview ? profilePreview.style.display : "";
    var previewObjectUrl = "";
    if (profileImageInput && profileImageFileName) {
        if (profileSave) profileSave.disabled = true;
        profileImageInput.addEventListener("change", function () {
            var selected = profileImageInput.files && profileImageInput.files[0];
            if (!selected) {
                if (previewObjectUrl) URL.revokeObjectURL(previewObjectUrl);
                previewObjectUrl = "";
                if (profilePreview) {
                    profilePreview.src = profileOriginalSrc || "";
                    profilePreview.style.display = profileOriginalDisplay;
                }
                if (profileInitial) profileInitial.style.display = "";
                profileImageFileName.textContent = "Choose a JPG or PNG image (up to 3 MB).";
                if (profileSave) profileSave.disabled = true;
                return;
            }
            var supportedExtension = /\.(jpe?g|png)$/i.test(selected.name || "");
            var supportedMime = !selected.type || selected.type === "image/jpeg" || selected.type === "image/png";
            var supportedType = supportedExtension && supportedMime;
            if (!supportedType || selected.size <= 0 || selected.size > 3 * 1024 * 1024) {
                profileImageInput.value = "";
                profileImageFileName.textContent = "Choose a JPG or PNG image up to 3 MB.";
                if (profileSave) profileSave.disabled = true;
                return;
            }
            if (previewObjectUrl) URL.revokeObjectURL(previewObjectUrl);
            previewObjectUrl = URL.createObjectURL(selected);
            if (profilePreview) {
                profilePreview.src = previewObjectUrl;
                profilePreview.style.display = "block";
            }
            if (profileInitial) profileInitial.style.display = "none";
            profileImageFileName.textContent = "Preview ready · " + selected.name + " · " + (selected.size / (1024 * 1024)).toFixed(2) + " MB. Select Save photo to upload.";
            if (profileSave) profileSave.disabled = false;
        });
        window.addEventListener("beforeunload", function () {
            if (previewObjectUrl) URL.revokeObjectURL(previewObjectUrl);
        });
    }

    var cvInput = document.getElementById("cvUpload");
    var cvDropzone = document.getElementById("cvDropzone");
    var cvFileName = document.getElementById("cvFileName");
    var cvError = document.getElementById("cvUploadError");
    var cvUploadButton = document.querySelector(".profile-cv-upload .btn");
    function validateCv(file) {
        if (!file) return false;
        var isPdf = file.type === "application/pdf" || /\.pdf$/i.test(file.name || "");
        if (!isPdf || file.size <= 0 || file.size > 2 * 1024 * 1024) {
            if (cvError) {
                cvError.textContent = !isPdf ? "Please choose a PDF file." : "The PDF must be 2 MB or smaller.";
                cvError.hidden = false;
            }
            if (cvInput) cvInput.value = "";
            if (cvFileName) cvFileName.textContent = "Choose a PDF to upload";
            if (cvUploadButton) cvUploadButton.disabled = true;
            return false;
        }
        if (cvError) { cvError.textContent = ""; cvError.hidden = true; }
        if (cvFileName) cvFileName.textContent = file.name + " · " + (file.size / (1024 * 1024)).toFixed(2) + " MB";
        if (cvUploadButton) cvUploadButton.disabled = false;
        return true;
    }
    if (cvInput && cvFileName) {
        if (cvUploadButton) cvUploadButton.disabled = true;
        cvInput.addEventListener("change", function () { validateCv(cvInput.files && cvInput.files[0]); });
        if (cvDropzone) {
            ["dragenter", "dragover"].forEach(function (eventName) {
                cvDropzone.addEventListener(eventName, function (event) {
                    event.preventDefault();
                    cvDropzone.classList.add("is-dragging");
                });
            });
            ["dragleave", "drop"].forEach(function (eventName) {
                cvDropzone.addEventListener(eventName, function (event) {
                    event.preventDefault();
                    cvDropzone.classList.remove("is-dragging");
                });
            });
            cvDropzone.addEventListener("drop", function (event) {
                var files = event.dataTransfer && event.dataTransfer.files;
                if (!files || !files.length) return;
                try { cvInput.files = files; } catch (_) { return; }
                validateCv(cvInput.files[0]);
            });
        }
    }

    var requestedTab = location.hash && document.getElementById("tab-" + location.hash.slice(1));
    if (!requestedTab && activeTabField) requestedTab = document.getElementById("tab-" + activeTabField.value);
    if (requestedTab) showTab(requestedTab);
})();
