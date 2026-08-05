// Site configuration.
// Non-secret, client-visible settings only — never put connection strings,
// API keys or other secrets here (see .claude/rules/tech-stack.md).
window.SITE_CONFIG = {
  forms: {
    // ESL Form 120 — Tertiary Education Services Access Request (forms.html)
    esl120: {
      // Where the submission summary is sent. Change this value to redirect
      // future submissions — no other code changes are required.
      recipientEmail: "harry.nguyen@education.govt.nz",
      subject: "ESL Form 120 - Tertiary Education Services Access Request"
    }
  }
};
