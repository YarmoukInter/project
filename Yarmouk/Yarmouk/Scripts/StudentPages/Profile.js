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

    var requestedTab = location.hash && document.getElementById("tab-" + location.hash.slice(1));
    if (!requestedTab && activeTabField) requestedTab = document.getElementById("tab-" + activeTabField.value);
    if (requestedTab) showTab(requestedTab);
})();
