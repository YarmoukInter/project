// JavaScript for CompanyPages/ApplicationsReview.aspx only.
(function() {
    "use strict";

    var rows = document.getElementById("applicationRows");
    if (!rows) return;

    var applications = [];
    var page = document.querySelector(".co-applications");
    var apiUrl = page.getAttribute("data-api-url");
    var feedback = document.getElementById("applicationFeedback");

    var selectedId = applications[0] ? applications[0].id : null;
    var search = document.getElementById("applicationSearch");
    var opportunityFilter = document.getElementById("applicationOpportunityFilter");
    var statusFilter = document.getElementById("applicationStatusFilter");
    var detail = document.getElementById("candidateDetail");
    var rejectionModal = document.getElementById("rejectionModal");
    var pendingRejectionId = null;

    function api(action, payload) {
        var options = { method: payload ? "POST" : "GET", credentials: "same-origin", headers: {} };
        if (payload) {
            options.headers["Content-Type"] = "application/json";
            options.headers["X-Requested-With"] = "XMLHttpRequest";
            options.body = JSON.stringify(Object.assign({ action: action }, payload));
        }
        return fetch(apiUrl + "?action=" + encodeURIComponent(action), options).then(function(response) {
            return response.json().then(function(data) {
                if (!response.ok || !data.success) throw new Error(data.message || "The request could not be completed.");
                return data;
            });
        });
    }
    function loadApplications(successMessage) {
        if (!successMessage) feedback.textContent = "Loading applications from Firebase…";
        return api("applications").then(function(data) {
            applications = data.applications || [];
            var previousSelection = selectedId;
            selectedId = applications.some(function(item) { return item.id === previousSelection; })
                ? previousSelection : (applications[0] ? applications[0].id : null);
            opportunityFilter.innerHTML = '<option value="all">All opportunities</option>';
            var seen = {};
            applications.forEach(function(item) {
                if (!seen[item.opportunityId]) {
                    seen[item.opportunityId] = true;
                    opportunityFilter.add(new Option(item.opportunity, item.opportunityId));
                }
            });
            renderRows();
            feedback.textContent = successMessage || "Applications loaded from Firebase.";
            return applications;
        }).catch(function(error) {
            feedback.textContent = "Could not load applications: " + error.message;
            throw error;
        });
    }

    function escapeHtml(value) {
        return String(value == null ? "" : value).replace(/[&<>"']/g, function(character) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[character];
        });
    }
    function statusLabel(status) { return status === "in-review" ? "In review" : status.charAt(0).toUpperCase() + status.slice(1); }
    function formatDate(value) {
        if (typeof value === "number" && value > 0) {
            var appliedDate = new Date(value);
            return appliedDate.toLocaleDateString("en", { day: "2-digit", month: "short", year: "numeric" });
        }
        var parts = (value || "").split("-");
        return parts.length === 3 ? parts[2] + " " + new Date(Number(parts[0]), Number(parts[1]) - 1, 1).toLocaleString("en", { month: "short" }) + " " + parts[0] : escapeHtml(value || "—");
    }
    function filteredItems() {
        var term = (search.value || "").trim().toLowerCase();
        return applications.filter(function(item) {
            var matchesText = [item.name, item.number, item.major, item.opportunity].join(" ").toLowerCase().indexOf(term) >= 0;
            return matchesText && (opportunityFilter.value === "all" || item.opportunityId === opportunityFilter.value) && (statusFilter.value === "all" || item.status === statusFilter.value);
        });
    }
    function renderStats() {
        var definitions = [
            ["♧", "New applications", "new"], ["◷", "In review", "in-review"], ["✓", "Accepted", "accepted"], ["×", "Rejected", "rejected"]
        ];
        document.getElementById("applicationStats").innerHTML = definitions.map(function(entry) {
            var count = applications.filter(function(item) { return item.status === entry[2]; }).length;
            return '<article class="co-review-stat"><span class="co-review-stat-icon" aria-hidden="true">' + entry[0] + '</span><span class="co-review-stat-copy"><span>' + entry[1] + '</span><strong>' + count + '</strong></span></article>';
        }).join("");
    }
    function renderRows() {
        var items = filteredItems();
        document.getElementById("applicationResultsCount").textContent = items.length + (items.length === 1 ? " application" : " applications");
        document.getElementById("applicationEmpty").hidden = items.length > 0;
        if (!items.some(function(item) { return item.id === selectedId; })) selectedId = items.length ? items[0].id : null;
        rows.innerHTML = items.map(function(item) {
            var selected = item.id === selectedId;
            return '<tr class="co-candidate-row' + (selected ? " selected" : "") + '" data-application-id="' + escapeHtml(item.id) + '" tabindex="0" aria-selected="' + selected + '">' +
                '<td><span class="co-student-name">' + escapeHtml(item.name) + '</span><span class="co-student-number">' + escapeHtml(item.number) + '</span></td>' +
                '<td class="co-major-cell">' + escapeHtml(item.major) + '</td><td>' + formatDate(item.appliedAtUnixMs) + '</td><td><span class="co-status co-status-' + escapeHtml(item.status) + '">' + statusLabel(item.status) + '</span></td><td><button class="co-row-open" type="button" aria-label="Review ' + escapeHtml(item.name) + '">›</button></td></tr>';
        }).join("");
        renderStats();
        renderDetail();
    }
    function renderDetail() {
        var item = applications.find(function(candidate) { return candidate.id === selectedId; });
        if (!item) {
            detail.innerHTML = '<div class="co-detail-empty"><span class="co-detail-mark" aria-hidden="true">Y</span><h2>Select an application</h2><p>Choose a student from the list to review their details.</p></div>';
            return;
        }
        var questions = Array.isArray(item.questions) ? item.questions : [];
        var answers = Array.isArray(item.answers) ? item.answers : [];
        var responses = questions.length ? questions.map(function(question, index) {
            return '<p class="co-answer"><strong>' + escapeHtml(question || "Question " + (index + 1)) + '</strong><span>' + escapeHtml(answers[index] || "No answer provided.") + '</span></p>';
        }).join("") : '<p class="co-answer co-answer-empty">No company questions were set for this opportunity.</p>';
        var actions = item.status === "new" || item.status === "in-review" ?
            '<div class="co-detail-actions"><button class="co-button co-button-primary" type="button" data-accept="' + escapeHtml(item.id) + '">✓ Accept application</button><button class="co-button co-button-danger" type="button" data-reject="' + escapeHtml(item.id) + '">× Reject</button></div>' : "";
        var note = item.status === "accepted" ? '<p class="co-offer-note">✓ ' + escapeHtml(item.decisionNote || "Offer sent; waiting for student confirmation.") + '</p>' :
            item.status === "rejected" && item.rejectionReason ? '<p class="co-rejection-note"><strong>Rejection reason</strong><br>' + escapeHtml(item.rejectionReason) + '</p>' : "";
        detail.innerHTML = '<header class="co-detail-head"><div><h2>' + escapeHtml(item.name) + '</h2><p>' + escapeHtml(item.major) + ' · ' + escapeHtml(item.number) + '</p></div><span class="co-status co-status-' + escapeHtml(item.status) + ' co-detail-status">' + statusLabel(item.status) + '</span></header>' +
            '<div class="co-detail-facts"><span class="co-detail-fact"><span>Email</span><strong>' + escapeHtml(item.email) + '</strong></span><span class="co-detail-fact"><span>Applied</span><strong>' + formatDate(item.appliedAtUnixMs) + '</strong></span><span class="co-detail-fact"><span>Opportunity</span><strong>' + escapeHtml(item.opportunity) + '</strong></span></div>' +
            '<section class="co-detail-section"><h3>CV / RESUME</h3>' + (isSafeUrl(item.cvUrl) ? '<a class="co-cv-link" href="' + escapeHtml(item.cvUrl) + '" target="_blank" rel="noopener noreferrer">▤ Open student CV</a>' : '<span class="co-detail-fact">No CV attached</span>') + '</section>' +
            '<section class="co-detail-section"><h3>COMPANY QUESTIONS &amp; ANSWERS</h3>' + responses + '</section>' + actions + note;
    }
    function isSafeUrl(value) { return typeof value === "string" && /^(https:\/\/|http:\/\/|\/)/i.test(value); }
    function setDecision(id, status, reason) {
        feedback.textContent = "Saving application decision…";
        detail.querySelectorAll("[data-accept], [data-reject]").forEach(function(button) { button.disabled = true; });
        return api("setApplicationStatus", { id: id, status: status, reason: reason || "" }).then(function() {
            return loadApplications("Checking the saved decision…");
        }).then(function(items) {
            var expected = status === "offered" ? "accepted" : status === "rejected" ? "rejected" : "in-review";
            var saved = items.find(function(item) { return item.id === id; });
            if (!saved || saved.status !== expected) throw new Error("Firebase did not confirm the decision.");
            feedback.textContent = status === "offered" ? "Offer saved. The student can now confirm it." :
                status === "rejected" ? "Rejection saved in Firebase." : "Application marked in review.";
        }).catch(function(error) {
            renderRows();
            feedback.textContent = "Could not save this decision: " + error.message;
        });
    }
    function closeRejectionModal() { rejectionModal.hidden = true; document.body.classList.remove("co-modal-open"); pendingRejectionId = null; }

    [search, opportunityFilter, statusFilter].forEach(function(control) {
        control.addEventListener(control === search ? "input" : "change", renderRows);
    });
    rows.addEventListener("click", function(event) {
        var row = event.target.closest("[data-application-id]");
        if (!row) return;
        selectedId = row.getAttribute("data-application-id");
        renderRows();
    });
    rows.addEventListener("keydown", function(event) {
        if ((event.key === "Enter" || event.key === " ") && event.target.matches("[data-application-id]")) {
            event.preventDefault(); selectedId = event.target.getAttribute("data-application-id"); renderRows();
        }
    });
    detail.addEventListener("click", function(event) {
        var acceptButton = event.target.closest("[data-accept]");
        var rejectButton = event.target.closest("[data-reject]");
        if (acceptButton) {
            var accepted = applications.find(function(item) { return item.id === acceptButton.getAttribute("data-accept"); });
            if (accepted) setDecision(accepted.id, "offered");
        }
        if (rejectButton) {
            pendingRejectionId = rejectButton.getAttribute("data-reject");
            document.getElementById("rejectionReasonInput").value = "";
            rejectionModal.hidden = false;
            document.body.classList.add("co-modal-open");
            document.getElementById("rejectionReasonInput").focus();
        }
    });
    rejectionModal.addEventListener("click", function(event) {
        if (event.target === rejectionModal || event.target.closest("[data-close-rejection]")) closeRejectionModal();
    });
    document.getElementById("confirmRejection").addEventListener("click", function() {
        var reason = document.getElementById("rejectionReasonInput").value.trim();
        if (!reason) { document.getElementById("rejectionReasonInput").focus(); return; }
        var id = pendingRejectionId;
        closeRejectionModal();
        setDecision(id, "rejected", reason);
    });
    document.addEventListener("keydown", function(event) { if (event.key === "Escape" && !rejectionModal.hidden) closeRejectionModal(); });

    renderRows();
    loadApplications().catch(function() { /* The visible feedback contains the error. */ });
}());
