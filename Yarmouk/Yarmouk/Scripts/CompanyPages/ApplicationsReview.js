(function () {
    "use strict";

    var dataNode = document.getElementById("applicationsReviewData");
    var detailModal = document.getElementById("applicationDetailModal");
    var rejectionModal = document.getElementById("rejectionModal");
    var selectedInput = document.getElementById("SelectedApplicationId");
    if (!dataNode || !detailModal || !rejectionModal || !selectedInput) return;

    var applications = [];
    try { applications = JSON.parse(dataNode.textContent || "[]"); } catch (_) { applications = []; }
    var byId = Object.create(null);
    applications.forEach(function (item) { byId[item.id] = item; });

    function text(selector, value) {
        var node = detailModal.querySelector(selector);
        if (node) node.textContent = value || "—";
    }

    function initials(name) {
        return (name || "Student").trim().split(/\s+/).slice(0, 2)
            .map(function (part) { return part.charAt(0).toUpperCase(); }).join("");
    }

    function setModalOpen(modal, open) {
        modal.hidden = !open;
        document.body.classList.toggle("co-modal-open", open || !rejectionModal.hidden);
    }

    function renderAnswers(item) {
        var host = detailModal.querySelector("[data-detail-answers]");
        host.replaceChildren();
        if (!item.questions || !item.questions.length) {
            var empty = document.createElement("p");
            empty.className = "co-answer co-answer-empty";
            empty.textContent = "No company questions were set for this opportunity.";
            host.appendChild(empty);
            return;
        }
        item.questions.forEach(function (question, index) {
            var card = document.createElement("article");
            card.className = "co-answer";
            var heading = document.createElement("strong");
            heading.textContent = question;
            var answer = document.createElement("span");
            answer.textContent = item.answers && item.answers[index] ? item.answers[index] : "No answer provided.";
            card.append(heading, answer);
            host.appendChild(card);
        });
    }

    function openApplication(id) {
        var item = byId[id];
        if (!item) return;
        selectedInput.value = item.id;
        text("[data-detail-name]", item.name);
        text("[data-detail-name-secondary]", item.name);
        text("[data-detail-summary]", (item.major || "Major not provided") + " · " + (item.number || "Number not provided"));
        text("[data-detail-number]", item.number || "Student number not provided");
        text("[data-detail-initials]", initials(item.name));
        text("[data-detail-email]", item.email || "Not provided");
        text("[data-detail-applied]", item.applied);
        text("[data-detail-opportunity]", item.opportunity);

        var status = detailModal.querySelector("[data-detail-status]");
        status.className = "co-status co-status-" + (item.status || "unknown") + " co-detail-status";
        status.textContent = item.statusLabel || "Unknown";

        var cv = detailModal.querySelector("[data-detail-cv]");
        var noCv = detailModal.querySelector("[data-detail-no-cv]");
        cv.hidden = !item.cvUrl;
        noCv.hidden = !!item.cvUrl;
        if (item.cvUrl) cv.href = item.cvUrl; else cv.removeAttribute("href");
        renderAnswers(item);

        var canDecide = item.status === "new" || item.status === "in-review";
        var actions = document.getElementById("applicationDecisionActions");
        var accept = document.getElementById("AcceptApplicationButton");
        var reject = document.getElementById("openRejectButton");
        actions.hidden = !canDecide;
        accept.hidden = !item.canAccept;
        reject.hidden = !canDecide;

        var blocked = document.getElementById("acceptanceBlockedMessage");
        blocked.hidden = !(canDecide && !item.canAccept);
        blocked.textContent = item.acceptanceBlockedReason || "This student cannot be accepted for this opportunity.";

        var note = detailModal.querySelector("[data-detail-note]");
        note.hidden = !item.note;
        note.className = "co-decision-note co-decision-note-" + (item.status || "unknown");
        note.textContent = item.note || "";
        setModalOpen(detailModal, true);
    }

    document.addEventListener("click", function (event) {
        var opener = event.target.closest("[data-application-open]");
        if (opener) { openApplication(opener.getAttribute("data-application-open")); return; }
        if (event.target.closest("[data-application-close]") || event.target === detailModal) {
            setModalOpen(detailModal, false); selectedInput.value = ""; return;
        }
        if (event.target.closest("#openRejectButton")) {
            rejectionModal.hidden = false; document.body.classList.add("co-modal-open"); return;
        }
        if (event.target.closest("[data-rejection-close]") || event.target === rejectionModal) {
            rejectionModal.hidden = true; document.body.classList.toggle("co-modal-open", detailModal.hidden); return;
        }
    });

    document.addEventListener("keydown", function (event) {
        if (event.key !== "Escape") return;
        if (!rejectionModal.hidden) rejectionModal.hidden = true;
        else if (!detailModal.hidden) { detailModal.hidden = true; selectedInput.value = ""; }
        document.body.classList.toggle("co-modal-open", !detailModal.hidden || !rejectionModal.hidden);
    });

    if (selectedInput.value && byId[selectedInput.value]) openApplication(selectedInput.value);
}());
