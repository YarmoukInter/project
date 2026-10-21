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
        var seats = Number(item.availablePositions) > 0 ? escapeHtml(item.availablePositions) + " places" : "Places available";
        var facts = ["Deadline: " + deadline, seats];
        if (item.duration) facts.push("Duration: " + escapeHtml(item.duration));
        if (Number(item.requiredTrainingHours) > 0) facts.push("Training hours: " + escapeHtml(item.requiredTrainingHours));
        if (item.workMode) facts.push("Work mode: " + escapeHtml(item.workMode));
        var skills = Array.isArray(item.skills) ? item.skills.filter(Boolean) : [];
        detail.innerHTML = '<div class="detail-brand">' + logo(item) + '<div><h2>' + escapeHtml(item.title) + '</h2><p>' + escapeHtml(item.companyName) + '</p></div></div>' +
            '<div class="detail-tags"><span class="detail-tag">' + escapeHtml(fieldName(item)) + '</span>' +
            (item.location ? '<span class="detail-tag">' + escapeHtml(item.location) + '</span>' : '') +
            '<span class="detail-tag">' + Number(item.questionsCount || 0) + ' application questions</span></div>' +
            '<div class="detail-facts">' + facts.map(function (fact) { return '<span>' + fact + '</span>'; }).join("") + '</div>' +
            '<section class="detail-section"><h3>About this opportunity</h3><p>' + escapeHtml(item.description || "Details will be provided by the company.") + '</p></section>' +
            (item.requirements ? '<section class="detail-section"><h3>Requirements</h3><p>' + escapeHtml(item.requirements) + '</p></section>' : '') +
            (skills.length ? '<section class="detail-section"><h3>Skills</h3><div class="detail-tags">' + skills.map(function (skill) { return '<span class="detail-tag">' + escapeHtml(skill) + '</span>'; }).join("") + '</div></section>' : '') +
            '<a class="apply-link" href="Apply.aspx?id=' + encodeURIComponent(item.id || "") + '">Apply for this opportunity <span aria-hidden="true"> →</span></a>';
    }
    function render() {
        var filtered = matchingItems();
        if (!filtered.some(function (item) { return item.id === selectedId; })) selectedId = filtered.length ? filtered[0].id : null;
        count.textContent = filtered.length + (filtered.length === 1 ? " opportunity" : " opportunities");
        empty.hidden = filtered.length > 0;
        list.innerHTML = filtered.map(function (item) {
            var selected = item.id === selectedId;
            return '<button type="button" class="opportunity-card' + (selected ? ' selected' : '') + '" data-id="' + escapeHtml(item.id) + '" aria-pressed="' + selected + '">' +
                logo(item) + '<span class="opportunity-card-copy"><h3>' + escapeHtml(item.title) + '</h3><p class="company-name">' + escapeHtml(item.companyName) + '</p>' +
                '<span class="opportunity-meta"><span>' + escapeHtml(fieldName(item)) + '</span>' + (item.location ? '<span>' + escapeHtml(item.location) + '</span>' : '') + '</span></span></button>';
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
    render();
}());
