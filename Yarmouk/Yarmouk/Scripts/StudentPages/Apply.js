(function () {
    "use strict";

    var dataNode = document.getElementById("application-data");
    if (!dataNode) return;
    var data = {};
    try { data = JSON.parse(dataNode.textContent || "{}"); } catch (error) { data = {}; }
    var student = data.student || {};
    var opportunity = data.opportunity || {};
    var questions = Array.isArray(opportunity.questions) ? opportunity.questions : [];

    function escapeHtml(value) {
        return String(value == null ? "" : value).replace(/[&<>"']/g, function (character) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[character];
        });
    }
    function safeImageUrl(value) {
        var url = String(value || "").trim();
        return /^https:\/\//i.test(url) || /^\/(?!\/)/.test(url) ? url : "";
    }
    var company = document.getElementById("applyCompany");
    var logoUrl = safeImageUrl(opportunity.companyLogoUrl);
    var companyInitial = String(opportunity.companyName || "C").trim().charAt(0).toUpperCase() || "C";
    company.innerHTML = (logoUrl ? '<img src="' + escapeHtml(logoUrl) + '" alt="">' : '<span class="company-fallback" aria-hidden="true">' + escapeHtml(companyInitial) + '</span>') +
        '<span>' + escapeHtml(opportunity.companyName || "Company") + '</span>';
    document.getElementById("studentName").textContent = student.fullName || "Name unavailable";
    document.getElementById("studentEmail").textContent = student.email || "Email unavailable";
    var cv = document.getElementById("studentCv");
    var cvLink = document.getElementById("cvLink");
    var cvUrl = safeImageUrl(student.cvUrl);
    cv.textContent = cvUrl ? (student.cvName || "CV / Resume") : "No CV link saved in your profile";
    if (cvUrl) { cvLink.href = cvUrl; cvLink.hidden = false; }

    var questionsRoot = document.getElementById("companyQuestions");
    var noQuestions = document.getElementById("noQuestions");
    document.getElementById("requiredNote").hidden = questions.length === 0;
    noQuestions.hidden = questions.length > 0;
    questionsRoot.innerHTML = questions.map(function (question, index) {
        var prompt = typeof question === "string" ? question : question.prompt;
        return '<div class="question-field"><label for="question-' + index + '">' + escapeHtml(prompt) + ' <em>*</em></label>' +
            '<textarea id="question-' + index + '" data-question-index="' + index + '" required maxlength="3000" placeholder="Write your answer here..."></textarea></div>';
    }).join("");

    window.prepareApplicationSubmission = function () {
        var inputs = Array.prototype.slice.call(document.querySelectorAll("[data-question-index]"));
        for (var i = 0; i < inputs.length; i++) {
            if (!inputs[i].value.trim()) {
                inputs[i].focus();
                inputs[i].reportValidity();
                return false;
            }
        }
        var answers = {};
        inputs.forEach(function (input) { answers[input.getAttribute("data-question-index")] = input.value.trim(); });
        var hidden = document.querySelector("input[id$='AnswersJson']");
        if (hidden) hidden.value = JSON.stringify(answers);
        return true;
    };
}());
