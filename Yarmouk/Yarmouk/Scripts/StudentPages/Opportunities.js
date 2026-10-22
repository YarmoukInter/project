(function () {
    "use strict";

    var dataNode = document.getElementById("opportunities-data");
    var list = document.getElementById("opportunityList");
    if (!dataNode || !list) return;

    var opportunities = [];
    try { opportunities = JSON.parse(dataNode.textContent || "[]"); } catch (error) { opportunities = []; }
    var selectedId = opportunities.length ? opportunities[0].id : null;
    var search = document.getElementById("opportunitySearch");
    var field = document.getElementById("opportunityField");
    var detail = document.getElementById("opportunityDetail");
    var count = document.getElementById("opportunityCount");
    var empty = document.getElementById("opportunityEmpty");
    var modal = document.getElementById("opportunityModal");
    var modalPanel = modal.querySelector(".opportunity-modal-panel");
    var student = {};
    try { student = JSON.parse(document.getElementById("opportunity-student-data").textContent || "{}"); } catch (error) { student = {}; }
    var lastFocus = null;

    function escapeHtml(value) {
        return String(value == null ? "" : value).replace(/[&<>"']/g, function (character) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[character];
        });
    }
    function safeImageUrl(value) {
        var url = String(value || "").trim();
        return /^https:\/\//i.test(url) || /^\/(?!\/)/.test(url) ? url : "";
    }
    function initials(value) {
        return String(value || "Y").trim().split(/\s+/).slice(0, 2).map(function (part) { return part.charAt(0); }).join("").toUpperCase() || "Y";
    }
    function logo(company) {
        var url = safeImageUrl(company.companyLogoUrl);
        return url ? '<img class="company-photo" src="' + escapeHtml(url) + '" alt="' + escapeHtml(company.companyName) + ' logo" loading="lazy">' :
            '<span class="company-fallback" aria-label="' + escapeHtml(company.companyName || "Company") + '">' + escapeHtml(initials(company.companyName)) + '</span>';
    }
    function fieldName(item) {
        return item.companyIndustry || "Internship";
    }
    function matchingItems() {
        var term = (search.value || "").trim().toLowerCase();
        var selectedField = field.value;
        return opportunities.filter(function (item) {
            var text = [item.title, item.companyName, item.companyIndustry, item.location, item.description].join(" ").toLowerCase();
            return text.indexOf(term) >= 0 && (!selectedField || fieldName(item) === selectedField);
        });
    }
    function renderDetail(item) {
        if (!item) {
            detail.innerHTML = '<div class="detail-empty"><span class="detail-mark">Y</span><h2>Choose an opportunity</h2><p>Select an internship to see its full details.</p></div>';
            return;
        }
        var deadline = item.deadline ? escapeHtml(item.deadline) : "Not specified";
        var seats = Number(item.availablePositions) > 0 ? Math.max(0, Number(item.availablePositions) - Number(item.filledPositions || 0)) + " places left" : "No seats available";
        var facts = ["Deadline: " + deadline, seats];
        if (item.duration) facts.push("Duration: " + escapeHtml(item.duration));
        if (Number(item.requiredTrainingHours) > 0) facts.push("Training hours: " + escapeHtml(item.requiredTrainingHours));
        if (item.workMode) facts.push("Work mode: " + escapeHtml(item.workMode));
        var skills = Array.isArray(item.skills) ? item.skills.filter(Boolean) : [];
        detail.innerHTML = '<div class="detail-brand">' + logo(item) + '<div><h2>' + escapeHtml(item.title) + '</h2><p>' + escapeHtml(item.companyName) + '</p></div></div>' +
            '<div class="detail-tags"><span class="detail-tag">' + escapeHtml(fieldName(item)) + '</span>' +
            (item.location ? '<span class="detail-tag">' + escapeHtml(item.location) + '</span>' : '') +
            '<span class="detail-tag">' + (Array.isArray(item.questions) ? item.questions.length : 0) + ' application questions</span></div>' +
            '<div class="detail-facts">' + facts.map(function (fact) { return '<span>' + fact + '</span>'; }).join("") + '</div>' +
            '<section class="detail-section"><h3>About this opportunity</h3><p>' + escapeHtml(item.description || "Details will be provided by the company.") + '</p></section>' +
            (item.requirements ? '<section class="detail-section"><h3>Requirements</h3><p>' + escapeHtml(item.requirements) + '</p></section>' : '') +
            (skills.length ? '<section class="detail-section"><h3>Skills</h3><div class="detail-tags">' + skills.map(function (skill) { return '<span class="detail-tag">' + escapeHtml(skill) + '</span>'; }).join("") + '</div></section>' : '') +
            (item.available && !item.alreadyApplied && eligible(item) ? '<button type="button" class="apply-link" data-apply-id="' + escapeHtml(item.id) + '">Apply for this opportunity <span aria-hidden="true"> →</span></button>' :
                '<p class="apply-unavailable">' + escapeHtml(item.alreadyApplied ? "Already applied" : !item.available ? (item.availabilityReason || "Unavailable") : "Not available for your major") + '</p>');
    }
    function eligible(item) {
        return !Array.isArray(item.majorIds) || !item.majorIds.length || item.majorIds.some(function (major) {
            return String(major).toLowerCase() === String(student.majorId || "").toLowerCase();
        });
    }
    function closeModal() {
        modal.hidden = true;
        document.body.classList.remove("opportunity-modal-open");
        if (lastFocus) lastFocus.focus();
    }
    function openModal(item) {
        if (!item || !item.available || item.alreadyApplied || !eligible(item)) return;
        lastFocus = document.activeElement;
        document.querySelector("input[id$='SelectedOpportunityId']").value = item.id;
        document.querySelector("input[id$='ApplicationAnswers']").value = "";
        document.getElementById("applicationModalTitle").textContent = "Apply for " + item.title;
        document.getElementById("applicationModalIntro").textContent = "Review your details and answer the company's questions before submitting.";
        document.getElementById("applicationStudent").innerHTML =
            '<div><span>Full name</span><strong>' + escapeHtml(student.fullName || "Not provided") + '</strong></div>' +
            '<div><span>Email</span><strong>' + escapeHtml(student.email || "Not provided") + '</strong></div>' +
            '<div><span>Student ID</span><strong>' + escapeHtml(student.studentId || "Not provided") + '</strong></div>' +
            '<div><span>Major</span><strong>' + escapeHtml(student.majorId || "Not provided") + '</strong></div>';
        document.getElementById("applicationOpportunity").innerHTML =
            '<strong>' + escapeHtml(item.companyName) + '</strong><p>' + escapeHtml(item.description || "No description provided.") + '</p>' +
            '<div class="modal-facts"><span>' + escapeHtml(item.location || "Location not specified") + '</span><span>' + escapeHtml(item.workMode || "Work mode not specified") + '</span><span>' + escapeHtml(item.duration || "Duration not specified") + '</span></div>';
        var questions = Array.isArray(item.questions) ? item.questions : [];
        document.getElementById("applicationQuestions").innerHTML = questions.length ? questions.map(function (question, index) {
            return '<label class="modal-question" for="opportunity-question-' + index + '"><span>' + escapeHtml(question) + ' <em>*</em></span><textarea id="opportunity-question-' + index + '" data-answer-index="' + index + '" maxlength="3000" required placeholder="Write your answer..."></textarea></label>';
        }).join("") : '<p>No company questions for this opportunity.</p>';
        document.getElementById("applicationError").hidden = true;
        modal.hidden = false;
        document.body.classList.add("opportunity-modal-open");
        modalPanel.focus();
    }
    window.prepareOpportunityApplication = function () {
        var answers = {};
        var fields = modal.querySelectorAll("[data-answer-index]");
        for (var i = 0; i < fields.length; i++) {
            if (!fields[i].value.trim()) {
                document.getElementById("applicationError").textContent = "Please answer every company question.";
                document.getElementById("applicationError").hidden = false;
                fields[i].focus();
                return false;
            }
            answers[fields[i].getAttribute("data-answer-index")] = fields[i].value.trim();
        }
        document.querySelector("input[id$='ApplicationAnswers']").value = JSON.stringify(answers);
        return true;
    };
    function render() {
        var filtered = matchingItems();
        if (!filtered.some(function (item) { return item.id === selectedId; })) selectedId = filtered.length ? filtered[0].id : null;
        count.textContent = filtered.length + (filtered.length === 1 ? " opportunity" : " opportunities");
        empty.hidden = filtered.length > 0;
        list.innerHTML = filtered.map(function (item) {
            var selected = item.id === selectedId;
            return '<button type="button" class="opportunity-card' + (selected ? ' selected' : '') + (!item.available || !eligible(item) ? ' unavailable' : '') + '" data-id="' + escapeHtml(item.id) + '" aria-pressed="' + selected + '">' +
                logo(item) + '<span class="opportunity-card-copy"><h3>' + escapeHtml(item.title) + '</h3><p class="company-name">' + escapeHtml(item.companyName) + '</p>' +
                '<span class="opportunity-meta"><span>' + escapeHtml(fieldName(item)) + '</span>' + (item.location ? '<span>' + escapeHtml(item.location) + '</span>' : '') + '</span></span><span class="opportunity-state">' + escapeHtml(!item.available ? item.availabilityReason || "Unavailable" : !eligible(item) ? "Other majors" : "Open") + '</span></button>';
        }).join("");
        renderDetail(filtered.find(function (item) { return item.id === selectedId; }));
    }
    opportunities.forEach(function (item) {
        var label = fieldName(item);
        if (label && !Array.prototype.some.call(field.options, function (option) { return option.value === label; })) {
            field.add(new Option(label, label));
        }
    });
    list.addEventListener("click", function (event) {
        var button = event.target.closest("[data-id]");
        if (!button) return;
        selectedId = button.getAttribute("data-id");
        render();
    });
    search.addEventListener("input", render);
    field.addEventListener("change", render);
    detail.addEventListener("click", function (event) {
        var button = event.target.closest("[data-apply-id]");
        if (button) openModal(opportunities.find(function (item) { return item.id === button.getAttribute("data-apply-id"); }));
    });
    modal.addEventListener("click", function (event) { if (event.target.closest("[data-close-modal]")) closeModal(); });
    document.addEventListener("keydown", function (event) {
        if (modal.hidden) return;
        if (event.key === "Escape") closeModal();
        if (event.key === "Tab") {
            var focusable = modalPanel.querySelectorAll("button:not([disabled]),textarea");
            if (!focusable.length) return;
            var first = focusable[0], last = focusable[focusable.length - 1];
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
        }
    });
    render();
    if (window.reopenOpportunityId) {
        openModal(opportunities.find(function (item) { return item.id === window.reopenOpportunityId; }));
        if (window.reopenOpportunityMessage && !modal.hidden) {
            document.getElementById("applicationError").textContent = window.reopenOpportunityMessage;
            document.getElementById("applicationError").hidden = false;
        }
        try {
            var previousAnswers = JSON.parse(window.reopenOpportunityAnswers || "{}");
            modal.querySelectorAll("[data-answer-index]").forEach(function (input) {
                input.value = previousAnswers[input.getAttribute("data-answer-index")] || "";
            });
        } catch (error) { /* Keep the form open for correction. */ }
    }
}());
