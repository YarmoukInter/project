(function () {
    "use strict";

    var backdrop = document.getElementById("OpportunityEditorPanel");
    var addButton = document.querySelector("[data-opportunity-add]");
    if (!backdrop || !addButton) return;

    var dialog = backdrop.querySelector("[role='dialog']");
    var title = document.getElementById("opportunityModalTitle");
    var saveButton = document.getElementById("SaveOpportunityButton");
    var returnFocus = null;
    var fields = {
        id: document.getElementById("EditingOpportunityId"),
        status: document.getElementById("EditingOpportunityStatus"),
        title: document.getElementById("OpportunityTitleInput"),
        deadline: document.getElementById("OpportunityDeadlineInput"),
        seats: document.getElementById("OpportunitySeatsInput"),
        duration: document.getElementById("OpportunityDurationInput"),
        hours: document.getElementById("OpportunityHoursInput"),
        workMode: document.getElementById("OpportunityWorkModeInput"),
        cis: document.getElementById("MajorCisCheckBox"),
        cs: document.getElementById("MajorCsCheckBox"),
        se: document.getElementById("MajorSeCheckBox"),
        description: document.getElementById("OpportunityDescriptionInput"),
        requirements: document.getElementById("OpportunityRequirementsInput"),
        questions: document.getElementById("OpportunityQuestionsInput"),
        feedback: backdrop.querySelector(".co-modal-feedback")
    };

    function setOpen(open, trigger) {
        backdrop.hidden = !open;
        document.body.classList.toggle("co-modal-open", open);
        if (open) {
            returnFocus = trigger || document.activeElement;
            window.requestAnimationFrame(function () {
                if (fields.title) fields.title.focus();
            });
        } else if (returnFocus && typeof returnFocus.focus === "function") {
            returnFocus.focus();
        }
    }

    function clearEditor() {
        fields.id.value = "";
        fields.status.value = "active";
        fields.title.value = "";
        fields.deadline.value = "";
        fields.seats.value = "1";
        fields.duration.value = "";
        fields.hours.value = "0";
        fields.workMode.value = "OnSite";
        fields.cis.checked = false;
        fields.cs.checked = false;
        fields.se.checked = false;
        fields.description.value = "";
        fields.requirements.value = "";
        fields.questions.value = "";
        if (fields.feedback) {
            fields.feedback.hidden = true;
            fields.feedback.textContent = "";
        }
        title.textContent = "Add opportunity";
        saveButton.value = "Save opportunity";
    }

    function editEditor(card) {
        clearEditor();
        fields.id.value = card.dataset.id || "";
        fields.status.value = card.dataset.status || "active";
        fields.title.value = card.dataset.title || "";
        fields.deadline.value = card.dataset.deadline || "";
        fields.seats.value = card.dataset.seats || "1";
        fields.duration.value = card.dataset.duration || "";
        fields.hours.value = card.dataset.hours || "0";
        if (Array.prototype.some.call(fields.workMode.options, function (option) { return option.value === card.dataset.workMode; }))
            fields.workMode.value = card.dataset.workMode;
        var majors = (card.dataset.majors || "").split("|");
        fields.cis.checked = majors.indexOf("CIS") !== -1;
        fields.cs.checked = majors.indexOf("CS") !== -1;
        fields.se.checked = majors.indexOf("SE") !== -1;
        fields.description.value = card.dataset.description || "";
        fields.requirements.value = card.dataset.requirements || "";
        fields.questions.value = card.dataset.questions || "";
        title.textContent = "Edit opportunity";
        saveButton.value = "Save changes";
    }

    addButton.addEventListener("click", function () {
        clearEditor();
        setOpen(true, addButton);
    });

    document.addEventListener("click", function (event) {
        var editButton = event.target.closest("[data-opportunity-edit]");
        if (editButton) {
            var card = editButton.closest("[data-opportunity-card]");
            if (card) {
                editEditor(card);
                setOpen(true, editButton);
            }
            return;
        }
        if (event.target.closest("[data-opportunity-close]")) setOpen(false);
    });

    backdrop.addEventListener("click", function (event) {
        if (event.target === backdrop) setOpen(false);
    });

    document.addEventListener("keydown", function (event) {
        if (backdrop.hidden) return;
        if (event.key === "Escape") {
            event.preventDefault();
            setOpen(false);
            return;
        }
        if (event.key !== "Tab" || !dialog) return;
        var focusable = Array.prototype.slice.call(dialog.querySelectorAll("button, input, select, textarea, a[href]"))
            .filter(function (element) { return !element.disabled && element.offsetParent !== null; });
        if (!focusable.length) return;
        var first = focusable[0];
        var last = focusable[focusable.length - 1];
        if (event.shiftKey && document.activeElement === first) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus();
        }
    });

    if (!backdrop.hidden) setOpen(true, addButton);
})();
