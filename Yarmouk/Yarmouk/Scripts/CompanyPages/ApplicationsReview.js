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
    var detailOpener = null;
    var rejectionOpener = null;

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
        modal.setAttribute("aria-hidden", String(!open));
        document.body.classList.toggle("co-modal-open", open || !rejectionModal.hidden);
    }

    function focusFirst(modal, selector) {
        var target = selector && modal.querySelector(selector);
        if (!target) target = modal.querySelector("button:not([hidden]):not([disabled]), [href]:not([hidden]), input:not([hidden]), textarea:not([hidden]), select:not([hidden])");
        if (target) target.focus();
    }

    function closeRejection(restoreFocus) {
        if (rejectionModal.hidden) return;
        setModalOpen(rejectionModal, false);
        var rejectionInput = rejectionModal.querySelector("textarea");
        var rejectionFeedback = rejectionModal.querySelector(".co-modal-feedback");
        if (rejectionInput) rejectionInput.value = "";
        if (rejectionFeedback) { rejectionFeedback.textContent = ""; rejectionFeedback.hidden = true; }
        if (restoreFocus && rejectionOpener && document.contains(rejectionOpener)) rejectionOpener.focus();
        rejectionOpener = null;
    }

    function closeApplication(restoreFocus) {
        closeRejection(false);
        setModalOpen(detailModal, false);
        selectedInput.value = "";
        if (restoreFocus && detailOpener && document.contains(detailOpener)) detailOpener.focus();
        detailOpener = null;
    }

    function trapFocus(event, modal) {
        if (event.key !== "Tab" || modal.hidden) return;
        var focusable = Array.prototype.filter.call(modal.querySelectorAll("button:not([hidden]):not([disabled]), [href]:not([hidden]), input:not([hidden]):not([disabled]), textarea:not([hidden]):not([disabled]), select:not([hidden]):not([disabled])"), function (node) {
            return node.offsetParent !== null;
        });
        if (!focusable.length) { event.preventDefault(); return; }
        var first = focusable[0];
        var last = focusable[focusable.length - 1];
        if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
        else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
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

    function openApplication(id, opener) {
        var item = byId[id];
        if (!item) return;
        detailOpener = opener || document.activeElement;
        selectedInput.value = item.id;
        text("[data-detail-name]", item.name);
        text("[data-detail-name-secondary]", item.name);
        text("[data-detail-summary]", (item.major || "Major not provided") + " · " + (item.number || "Number not provided"));
        text("[data-detail-number]", item.number || "Student number not provided");
        text("[data-detail-initials]", initials(item.name));
        var photo = detailModal.querySelector("[data-detail-photo]");
        if (photo) {
            photo.dataset.studentPhotoUrl = item.profileImageUrl || "";
            photo.removeAttribute("data-student-photo-ready");
            photo.querySelectorAll(".uims-student-photo-image").forEach(function (image) { image.remove(); });
            window.UimsStudentPhotos && window.UimsStudentPhotos.enhance(detailModal);
        }
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
        if (actions) actions.hidden = !canDecide;
        if (accept) accept.hidden = !item.canAccept;
        if (reject) reject.hidden = !canDecide;

        var blocked = document.getElementById("acceptanceBlockedMessage");
        if (blocked) {
            blocked.hidden = !(canDecide && !item.canAccept);
            blocked.textContent = item.acceptanceBlockedReason || "This student cannot be accepted for this opportunity.";
        }

        var note = detailModal.querySelector("[data-detail-note]");
        note.hidden = !item.note;
        note.className = "co-decision-note co-decision-note-" + (item.status || "unknown");
        note.textContent = item.note || "";
        setModalOpen(detailModal, true);
        focusFirst(detailModal, "[data-application-close]");
    }

    document.addEventListener("click", function (event) {
        var opener = event.target.closest("[data-application-open]");
        if (opener) { openApplication(opener.getAttribute("data-application-open"), opener); return; }
        if (event.target.closest("[data-application-close]") || event.target === detailModal) {
            closeApplication(true); return;
        }
        if (event.target.closest("#openRejectButton")) {
            rejectionOpener = event.target.closest("#openRejectButton");
            var rejectionInput = rejectionModal.querySelector("textarea");
            var rejectionFeedback = rejectionModal.querySelector(".co-modal-feedback");
            if (rejectionInput) rejectionInput.value = "";
            if (rejectionFeedback) { rejectionFeedback.textContent = ""; rejectionFeedback.hidden = true; }
            setModalOpen(rejectionModal, true);
            focusFirst(rejectionModal, "textarea");
            return;
        }
        if (event.target.closest("[data-rejection-close]") || event.target === rejectionModal) {
            closeRejection(true); return;
        }
    });

    document.addEventListener("keydown", function (event) {
        if (!rejectionModal.hidden) trapFocus(event, rejectionModal);
        else if (!detailModal.hidden) trapFocus(event, detailModal);
        if (event.key !== "Escape") return;
        event.preventDefault();
        if (!rejectionModal.hidden) closeRejection(true);
        else if (!detailModal.hidden) closeApplication(true);
    });

    if (selectedInput.value && byId[selectedInput.value]) openApplication(selectedInput.value, null);
}());
