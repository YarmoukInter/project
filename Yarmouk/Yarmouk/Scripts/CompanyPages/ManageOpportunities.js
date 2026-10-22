// JavaScript for CompanyPages/ManageOpportunities.aspx only.
(function() {
    "use strict";

    var rows = document.getElementById("opportunityRows");
    if (!rows) return;

    var opportunities = [];
    var page = document.querySelector(".co-opportunities");
    var apiUrl = page.getAttribute("data-api-url");
    var feedback = document.getElementById("opportunityFeedback");

    var modal = document.getElementById("opportunityModal");
    var form = document.getElementById("opportunityForm");
    var search = document.getElementById("opportunitySearch");
    var statusFilter = document.getElementById("opportunityStatusFilter");
    var editingId = null;

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
    function loadOpportunities() {
        feedback.textContent = "Loading opportunities from Firebase…";
        return api("opportunities").then(function(data) {
            opportunities = data.opportunities || [];
            render();
            feedback.textContent = "Your opportunities are connected to Firebase.";
        }).catch(function(error) {
            opportunities = [];
            render();
            feedback.textContent = "Could not load opportunities: " + error.message;
        });
    }

    function escapeHtml(value) {
        return String(value == null ? "" : value).replace(/[&<>"']/g, function(character) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[character];
        });
    }
    function formatDate(value) {
        if (!value) return "Not set";
        var parts = value.split("-");
        return parts.length === 3 ? parts[2] + " " + new Date(Number(parts[0]), Number(parts[1]) - 1, 1).toLocaleString("en", { month: "short" }) + " " + parts[0] : escapeHtml(value);
    }
    function statusLabel(status) { return status.charAt(0).toUpperCase() + status.slice(1); }
    function filteredItems() {
        var query = (search.value || "").trim().toLowerCase();
        var status = statusFilter.value;
        return opportunities.filter(function(item) {
            var matchesText = [item.title].concat(item.majors || []).join(" ").toLowerCase().indexOf(query) >= 0;
            return matchesText && (status === "all" || item.status === status);
        });
    }
    function renderStats() {
        var active = opportunities.filter(function(item) { return item.status === "active"; }).length;
        var drafts = opportunities.filter(function(item) { return item.status === "draft"; }).length;
        var totalApplications = opportunities.reduce(function(sum, item) { return sum + Number(item.applications || 0); }, 0);
        document.getElementById("opportunityStats").innerHTML = [
            ["▣", "Active opportunities", active, ""], ["▤", "Draft opportunities", drafts, "amber"], ["♧", "Total applications", totalApplications, ""]
        ].map(function(card) {
            return '<article class="co-stat-card"><span class="co-stat-icon ' + card[3] + '" aria-hidden="true">' + card[0] + '</span><span class="co-stat-copy"><span>' + card[1] + '</span><strong>' + card[2] + '</strong></span></article>';
        }).join("");
    }
    function render() {
        var items = filteredItems();
        document.getElementById("opportunityCount").textContent = items.length + (items.length === 1 ? " opportunity" : " opportunities");
        document.getElementById("opportunityEmpty").hidden = items.length > 0;
        rows.innerHTML = items.map(function(item) {
            var status = escapeHtml(item.status);
            var majors = (item.majors || []).map(function(major) { return '<span class="co-major-chip">' + escapeHtml(major) + '</span>'; }).join("");
            var action = item.status === "active" ? "Close" : item.status === "closed" ? "Reopen" : "Publish";
            return '<tr><td><span class="co-opportunity-title">' + escapeHtml(item.title) + '</span><span class="co-opportunity-subtitle">' + escapeHtml(item.id) + ' · ' + Number(item.applications || 0) + ' applications</span></td>' +
                '<td><span class="co-major-list">' + majors + '</span></td><td><span class="co-seat-value">' + Number(item.seats || 0) + ' seats</span></td><td>' + formatDate(item.deadline) + '</td>' +
                '<td><span class="co-status co-status-' + status + '">' + statusLabel(status) + '</span></td><td><span class="co-row-actions"><button type="button" class="co-text-action" data-edit="' + escapeHtml(item.id) + '">Edit</button><button type="button" class="co-text-action ' + (action === "Close" ? "close" : "") + '" data-toggle="' + escapeHtml(item.id) + '">' + action + '</button></span></td></tr>';
        }).join("");
        renderStats();
    }
    function openModal(item) {
        document.getElementById("opportunityTitleInput").value = "";
        document.getElementById("opportunityDeadlineInput").value = "";
        document.getElementById("opportunitySeatsInput").value = "1";
        document.getElementById("opportunityDurationInput").value = "";
        document.getElementById("opportunityHoursInput").value = "0";
        document.getElementById("opportunityWorkModeInput").value = "OnSite";
        document.getElementById("opportunityDescriptionInput").value = "";
        document.getElementById("opportunityRequirementsInput").value = "";
        document.getElementById("opportunityQuestionsInput").value = "";
        Array.prototype.forEach.call(form.querySelectorAll('input[name="major"]'), function(checkbox) { checkbox.checked = false; });
        editingId = item ? item.id : null;
        document.getElementById("opportunityModalTitle").textContent = item ? "Edit opportunity" : "Add opportunity";
        document.getElementById("saveOpportunityButton").textContent = item ? "Save changes" : "Save opportunity";
        if (item) {
            document.getElementById("opportunityTitleInput").value = item.title || "";
            document.getElementById("opportunityDeadlineInput").value = item.deadline || "";
            document.getElementById("opportunitySeatsInput").value = item.seats || 1;
            document.getElementById("opportunityDurationInput").value = item.duration || "";
            document.getElementById("opportunityHoursInput").value = item.hours || 0;
            document.getElementById("opportunityWorkModeInput").value = item.workMode || "OnSite";
            document.getElementById("opportunityDescriptionInput").value = item.description || "";
            document.getElementById("opportunityRequirementsInput").value = item.requirements || "";
            document.getElementById("opportunityQuestionsInput").value = (item.questions || []).join("\n");
            Array.prototype.forEach.call(form.querySelectorAll('input[name="major"]'), function(checkbox) { checkbox.checked = (item.majors || []).indexOf(checkbox.value) >= 0; });
        }
        modal.hidden = false;
        document.body.classList.add("co-modal-open");
        document.getElementById("opportunityTitleInput").focus();
    }
    function closeModal() { modal.hidden = true; document.body.classList.remove("co-modal-open"); }

    document.getElementById("openOpportunityForm").addEventListener("click", function() { openModal(null); });
    modal.addEventListener("click", function(event) {
        if (event.target === modal || event.target.closest("[data-close-opportunity-modal]")) closeModal();
    });
    document.addEventListener("keydown", function(event) { if (event.key === "Escape" && !modal.hidden) closeModal(); });
    search.addEventListener("input", render);
    statusFilter.addEventListener("change", render);
    rows.addEventListener("click", function(event) {
        var editButton = event.target.closest("[data-edit]");
        var toggleButton = event.target.closest("[data-toggle]");
        if (editButton) {
            var selected = opportunities.find(function(item) { return item.id === editButton.getAttribute("data-edit"); });
            if (selected) openModal(selected);
        }
        if (toggleButton) {
            var item = opportunities.find(function(entry) { return entry.id === toggleButton.getAttribute("data-toggle"); });
            if (!item) return;
            var nextStatus = item.status === "active" ? "closed" : "active";
            toggleButton.disabled = true;
            api("setOpportunityStatus", { id: item.id, status: nextStatus }).then(loadOpportunities).catch(function(error) {
                feedback.textContent = "Could not update the opportunity: " + error.message;
                toggleButton.disabled = false;
            });
        }
    });
    document.getElementById("saveOpportunityButton").addEventListener("click", function() {
        var controls = form.querySelectorAll("input:not([type=checkbox]), textarea, select");
        for (var controlIndex = 0; controlIndex < controls.length; controlIndex++) {
            if (!controls[controlIndex].checkValidity()) {
                controls[controlIndex].reportValidity();
                return;
            }
        }
        var selectedMajors = Array.prototype.map.call(form.querySelectorAll('input[name="major"]:checked'), function(checkbox) { return checkbox.value; });
        if (!selectedMajors.length) { window.alert("Select at least one eligible major."); return; }
        var item = editingId ? opportunities.find(function(entry) { return entry.id === editingId; }) : null;
        var values = {
            id: editingId,
            title: document.getElementById("opportunityTitleInput").value.trim(),
            deadline: document.getElementById("opportunityDeadlineInput").value,
            seats: Number(document.getElementById("opportunitySeatsInput").value),
            status: item ? item.status : "active",
            duration: document.getElementById("opportunityDurationInput").value.trim(),
            hours: Number(document.getElementById("opportunityHoursInput").value || 0),
            workMode: document.getElementById("opportunityWorkModeInput").value,
            majors: selectedMajors,
            description: document.getElementById("opportunityDescriptionInput").value.trim(),
            requirements: document.getElementById("opportunityRequirementsInput").value.trim(),
            questions: document.getElementById("opportunityQuestionsInput").value.split(/\r?\n/).map(function(question) { return question.trim(); }).filter(Boolean)
        };
        var saveButton = document.getElementById("saveOpportunityButton");
        saveButton.disabled = true;
        saveButton.textContent = "Saving…";
        api("saveOpportunity", { opportunity: values }).then(function() {
            closeModal();
            feedback.textContent = "Opportunity saved to Firebase.";
            return loadOpportunities();
        }).catch(function(error) {
            feedback.textContent = "Could not save the opportunity: " + error.message;
        }).then(function() {
            saveButton.disabled = false;
            saveButton.textContent = editingId ? "Save changes" : "Save opportunity";
        });
    });

    render();
    loadOpportunities();
}());
