// ESL Form 120 — Tertiary Education Services Access Request
// Builds a plain-text summary of the submitted answers, confirms with the
// visitor that it's correct, then sends it to the configured Digital
// Identity team recipient via a mailto: link.
//
// This site is currently static HTML/CSS/JS with no server-side layer, so a
// browser cannot silently send email on a user's behalf. mailto: is the
// honest client-only equivalent: it opens the visitor's own email app with
// the recipient, subject and summary pre-filled and triggers send from a
// single confirmed user action. When a server-side layer is introduced (see
// .claude/rules/tech-stack.md — ASP.NET Core on Azure App Service), replace
// sendSummaryEmail() with a POST to a server endpoint that sends the email
// itself.
(function () {
  "use strict";

  var form = document.getElementById("esl120-form");
  if (!form) return;

  var config = (window.SITE_CONFIG && window.SITE_CONFIG.forms && window.SITE_CONFIG.forms.esl120) || {
    recipientEmail: "service.desk@education.govt.nz",
    subject: "ESL Form 120 - Tertiary Education Services Access Request"
  };

  var summaryPanel = document.getElementById("esl120-summary");
  var summaryText = document.getElementById("esl120-summary-text");
  var summaryHeading = document.getElementById("esl120-summary-heading");
  var recipientNote = document.getElementById("esl120-recipient");
  var copyBtn = document.getElementById("esl120-copy");
  var copyStatus = document.getElementById("esl120-copy-status");
  var startOverBtn = document.getElementById("esl120-start-over");

  if (recipientNote) recipientNote.textContent = config.recipientEmail;

  // Collect a readable label for a field wrapper, stripping the trailing
  // required-marker text (e.g. "Given names *" -> "Given names").
  function cleanLabel(text) {
    return text.replace(/\s*\*\s*$/, "").replace(/\s+/g, " ").trim();
  }

  function labelFor(wrapper) {
    var labelEl = wrapper.querySelector("label, legend");
    return labelEl ? cleanLabel(labelEl.textContent) : "";
  }

  // Short display text for a single radio/checkbox option: prefer the
  // ".option-title" span (skips the longer ".option-desc" helper text).
  function optionText(input) {
    var optLabel = input.closest("label");
    if (!optLabel) return input.value;
    var title = optLabel.querySelector(".option-title");
    return cleanLabel(title ? title.textContent : optLabel.textContent);
  }

  // Read one field wrapper (.form-field or fieldset.form-fieldset) into a
  // "Label: value" line, or null if the visitor left it blank/unchecked.
  function readField(wrapper) {
    var label = labelFor(wrapper);
    var radios = wrapper.querySelectorAll('input[type="radio"]');
    var checkboxes = wrapper.querySelectorAll('input[type="checkbox"]');

    if (radios.length) {
      var checkedRadio = wrapper.querySelector('input[type="radio"]:checked');
      if (!checkedRadio) return null;
      return label + ": " + optionText(checkedRadio);
    }

    if (checkboxes.length) {
      var picked = [];
      checkboxes.forEach(function (cb) {
        if (cb.checked) picked.push(optionText(cb));
      });
      if (!picked.length) return null;
      return label + ": " + picked.join("; ");
    }

    var input = wrapper.querySelector("input, textarea, select");
    if (!input) return null;
    var val = input.value.trim();
    if (!val) return null;
    return label + ": " + val;
  }

  function buildSummary() {
    var lines = [];
    lines.push(config.subject);
    lines.push("Submitted: " + new Date().toLocaleString("en-NZ"));
    lines.push("");

    var parts = form.querySelectorAll(".form-part");
    parts.forEach(function (part) {
      var heading = part.querySelector("h2");
      var partLines = [];
      var fields = part.querySelectorAll(".form-field, .form-fieldset");
      fields.forEach(function (field) {
        var line = readField(field);
        if (line) partLines.push("- " + line);
      });
      if (partLines.length) {
        lines.push((heading ? heading.textContent.trim() : "Section").toUpperCase());
        lines.push.apply(lines, partLines);
        lines.push("");
      }
    });

    return lines.join("\n").trim();
  }

  // Opens the visitor's email application with the summary addressed to the
  // configured Digital Identity team recipient (window.SITE_CONFIG.forms.esl120
  // in assets/js/config.js — change recipientEmail there to redirect future
  // submissions, no code changes required).
  function sendSummaryEmail(text) {
    var mailBody = encodeURIComponent(text + "\n\n(Sent from the Digital Identity Services website, forms.html)");
    var mailSubject = encodeURIComponent(config.subject);
    window.location.href = "mailto:" + encodeURIComponent(config.recipientEmail) + "?subject=" + mailSubject + "&body=" + mailBody;
  }

  function showSummary(text) {
    summaryText.textContent = text;
    summaryPanel.hidden = false;
    form.hidden = true;

    // Move focus to the new region so screen reader / keyboard users land
    // on the confirmation instead of a now-hidden form.
    summaryHeading.setAttribute("tabindex", "-1");
    summaryHeading.focus();
    summaryPanel.scrollIntoView({ behavior: reduceMotionSafe() ? "auto" : "smooth", block: "start" });
  }

  function reduceMotionSafe() {
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  }

  form.addEventListener("submit", function (e) {
    e.preventDefault();
    if (!form.checkValidity()) {
      form.reportValidity();
      return;
    }

    var confirmed = window.confirm(
      "Is the information you've entered correct? Pressing OK will send it to " +
      config.recipientEmail + " (the Digital Identity team)."
    );
    if (!confirmed) return;

    var summary = buildSummary();
    sendSummaryEmail(summary);
    showSummary(summary);
  });

  if (copyBtn) {
    copyBtn.addEventListener("click", function () {
      var text = summaryText.textContent;
      if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(text).then(function () {
          copyStatus.textContent = "Summary copied to clipboard.";
        }, function () {
          copyStatus.textContent = "Couldn't copy automatically — please select and copy the text above.";
        });
      } else {
        copyStatus.textContent = "Couldn't copy automatically — please select and copy the text above.";
      }
    });
  }

  if (startOverBtn) {
    startOverBtn.addEventListener("click", function () {
      form.reset();
      form.hidden = false;
      summaryPanel.hidden = true;
      copyStatus.textContent = "";
      form.scrollIntoView({ behavior: reduceMotionSafe() ? "auto" : "smooth", block: "start" });
      var firstField = form.querySelector("input, select, textarea");
      if (firstField) firstField.focus();
    });
  }
})();
