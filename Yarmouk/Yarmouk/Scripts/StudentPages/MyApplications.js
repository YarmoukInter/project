(function () {
    "use strict";

    var dataNode = document.getElementById("my-applications-data");
    var list = document.getElementById("applicationList");
    if (!dataNode || !list) return;
    var applications = [];
    try { applications = JSON.parse(dataNode.textContent || "[]"); } catch (error) { applications = []; }
    var activeFilter = "all";
    var search = document.getElementById("applicationSearch");
    var sort = document.getElementById("applicationSort");
    var empty = document.getElementById("applicationsEmpty");
    var feedback = document.getElementById("applicationsFeedback");
    var modal = document.getElementById("applicationDetailsModal");

    function escapeHtml(value) {
        return String(value == null ? "" : value).replace(/[&<>"']/g, function (character) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[character];
        });
    }
    function safeImageUrl(value) {
        var url = String(value || "").trim();
        return /^https:\/\//i.test(url) || /^\/(?!\/)/.test(url) ? url : "";
    }
    function initials(value) { return String(value || "Y").trim().split(/\s+/).slice(0, 2).map(function (part) { return part.charAt(0); }).join("").toUpperCase() || "Y"; }
    function normalizedStatus(value) {
        value = String(value || "pending").toLowerCase().replace(/[ _-]/g, "");
        if (value === "offerreceived" || value === "offered") return "offered";
        if (value === "accepted") return "accepted";
        if (value === "rejected") return "rejected";
        return "pending";
    }
    function statusLabel(status) {
        return { pending: "Pending", offered: "Offer received", accepted: "Accepted", rejected: "Rejected" }[status];
    }
    function dateLabel(timestamp) {
        if (!timestamp || !Number(timestamp)) return "Date unavailable";
        var date = new Date(Number(timestamp));
        return isNaN(date.getTime()) ? "Date unavailable" : date.toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" });
    }
    function logo(item) {
        var url = safeImageUrl(item.companyLogoUrl);
        return url ? '<img class="company-photo" src="' + escapeHtml(url) + '" alt="' + escapeHtml(item.companyName) + ' logo" loading="lazy">' :
            '<span class="company-fallback" aria-hidden="true">' + escapeHtml(initials(item.companyName)) + '</span>';
    }
    function render() {
        var term = (search.value || "").trim().toLowerCase();
        var acceptedExists = applications.some(function (item) { return normalizedStatus(item.status) === "accepted"; });
        var filtered = applications.filter(function (item) {
            var status = normalizedStatus(item.status);
            var text = [item.title, item.companyName, item.description].join(" ").toLowerCase();
            return (activeFilter === "all" || status === activeFilter) && text.indexOf(term) >= 0;
        });
        filtered.sort(function (a, b) {
            var difference = Number(a.appliedAtUnixMs || 0) - Number(b.appliedAtUnixMs || 0);
            return sort && sort.value === "oldest" ? difference : -difference;
        });
        list.innerHTML = filtered.map(function (item) {
            var status = normalizedStatus(item.status);
            var rejection = status === "rejected" && item.rejectionReason ? '<div class="rejection-reason"><strong>Company feedback:</strong> ' + escapeHtml(item.rejectionReason) + '</div>' : "";
            var statusInfo = { pending: ["Under Review", "Your application is being reviewed by the company."], offered: ["Offer Received", "The company has sent you an offer. Review the details and accept it if you wish."], accepted: ["Accepted", "You selected this internship as your final choice."], rejected: ["Rejected", "This application was not selected. You can continue exploring other opportunities."] }[status];
            var action = status === "offered" ? (acceptedExists ? '<span class="offer-locked">You have already confirmed an internship.</span>' : '<button type="button" class="accept-offer-button" data-accept-offer="' + escapeHtml(item.id) + '">✓ &nbsp; Accept Offer</button>') : "";
            return '<article class="application-card status-card-' + status + '" data-application-id="' + escapeHtml(item.id) + '"><div class="application-main"><span class="application-symbol" aria-hidden="true">' + escapeHtml(initials(item.companyName)) + '</span><div class="application-copy"><p class="application-category">INTERNSHIP · PRACTICAL TRAINING</p><h2>' + escapeHtml(item.title || "Internship opportunity") + '</h2>' +
                '<p class="application-company">' + logo(item) + '<span>' + escapeHtml(item.companyName || "Company") + '</span></p><p class="application-summary">' + escapeHtml(item.description || "") + '</p></div></div>' +
                '<div class="application-status-column"><div class="application-status-panel status-' + status + '"><strong>' + statusInfo[0] + '</strong><p>' + statusInfo[1] + '</p></div><p class="application-date"><span aria-hidden="true">▦</span> Applied on <strong>' + escapeHtml(dateLabel(item.appliedAtUnixMs)) + '</strong></p>' + rejection + '</div>' +
                '<div class="application-actions"><button type="button" class="view-details-button" data-view-details="' + escapeHtml(item.id) + '">◎ &nbsp; View Details <span>›</span></button>' + action + '</div></article>';
        }).join("");
        empty.hidden = filtered.length > 0;
        empty.style.display = filtered.length ? "none" : "block";
        if (applications.length && !filtered.length) {
            empty.querySelector("h2").textContent = "No matching applications";
            empty.querySelector("p").textContent = "Try another status filter or search term.";
        } else {
            empty.querySelector("h2").textContent = "No applications to show";
            empty.querySelector("p").textContent = "When you apply for an opportunity, it will appear here.";
        }
    }
    document.querySelectorAll(".filter-button").forEach(function (button) {
        button.addEventListener("click", function () {
            activeFilter = button.getAttribute("data-filter") || "all";
            document.querySelectorAll(".filter-button").forEach(function (other) { other.classList.toggle("active", other === button); });
            render();
        });
    });
    list.addEventListener("click", function (event) {
        var detailsButton = event.target.closest("[data-view-details]");
        if (detailsButton) {
            var selected = applications.find(function (item) { return String(item.id) === detailsButton.getAttribute("data-view-details"); });
            if (selected && modal) {
                document.getElementById("applicationModalTitle").textContent = selected.title || "Internship opportunity";
                document.getElementById("applicationModalBody").innerHTML = '<p class="modal-company">' + escapeHtml(selected.companyName || "Company") + '</p><p>' + escapeHtml(selected.description || "No additional description is available.") + '</p><p class="modal-date">Applied on ' + escapeHtml(dateLabel(selected.appliedAtUnixMs)) + '</p><span class="status-pill status-' + normalizedStatus(selected.status) + '">' + statusLabel(normalizedStatus(selected.status)) + '</span>';
                modal.hidden = false;
                document.body.classList.add("application-modal-open");
            }
            return;
        }
        var button = event.target.closest("[data-accept-offer]");
        if (!button) return;
        if (!window.confirm("Confirm this offer as your one internship placement? You can confirm only one offer.")) return;
        var idField = document.querySelector("input[id$='AcceptApplicationId']");
        var postback = document.querySelector("input[id$='AcceptApplicationPostback']");
        if (!idField || !postback) return;
        idField.value = button.getAttribute("data-accept-offer");
        postback.click();
    });
    if (modal) {
        modal.addEventListener("click", function (event) {
            if (event.target === modal || event.target.closest("[data-close-details]")) {
                modal.hidden = true;
                document.body.classList.remove("application-modal-open");
            }
        });
        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape" && !modal.hidden) { modal.hidden = true; document.body.classList.remove("application-modal-open"); }
        });
    }
    search.addEventListener("input", render);
    if (sort) sort.addEventListener("change", render);
    render();
}());
