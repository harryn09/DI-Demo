// Site-wide chat assistant widget
(function () {
  "use strict";

  var STORAGE_KEY = "di-chatbot-history";
  var DOCK_DELAY_MS = 10000;
  var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  var launcher = document.createElement("button");
  launcher.type = "button";
  launcher.className = "chatbot-launcher" + (reduceMotion ? " chatbot-docked" : "");
  launcher.setAttribute("aria-label", "Open chat assistant");
  launcher.setAttribute("aria-expanded", "false");
  launcher.textContent = "?";

  var panel = document.createElement("div");
  panel.className = "chatbot-panel";
  panel.setAttribute("role", "dialog");
  panel.setAttribute("aria-modal", "false");
  panel.setAttribute("aria-label", "Chat assistant");
  panel.innerHTML =
    '<div class="chatbot-header">' +
    "<h2>Ask us a question</h2>" +
    '<button type="button" class="chatbot-close" aria-label="Close chat assistant">&times;</button>' +
    "</div>" +
    '<div class="chatbot-messages" role="log" aria-live="polite"></div>' +
    '<form class="chatbot-form">' +
    '<textarea class="chatbot-input" rows="1" maxlength="2000" placeholder="Ask a question..." aria-label="Your message"></textarea>' +
    '<button type="submit" class="chatbot-send">Send</button>' +
    "</form>";

  document.body.appendChild(launcher);
  document.body.appendChild(panel);

  var messagesEl = panel.querySelector(".chatbot-messages");
  var formEl = panel.querySelector(".chatbot-form");
  var inputEl = panel.querySelector(".chatbot-input");
  var closeEl = panel.querySelector(".chatbot-close");

  var history = loadHistory();
  history.forEach(function (m) { renderMessage(m.role, m.content); });

  if (!reduceMotion) {
    window.setTimeout(function () {
      launcher.classList.add("chatbot-docked");
    }, DOCK_DELAY_MS);
  }

  launcher.addEventListener("click", openPanel);
  closeEl.addEventListener("click", closePanel);
  panel.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
      closePanel();
    }
  });

  formEl.addEventListener("submit", function (e) {
    e.preventDefault();
    sendMessage();
  });

  inputEl.addEventListener("keydown", function (e) {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  });

  function openPanel() {
    launcher.classList.add("chatbot-open");
    panel.classList.add("chatbot-open");
    launcher.setAttribute("aria-expanded", "true");
    inputEl.focus();
  }

  function closePanel() {
    launcher.classList.remove("chatbot-open");
    panel.classList.remove("chatbot-open");
    launcher.setAttribute("aria-expanded", "false");
    launcher.focus();
  }

  function sendMessage() {
    var text = inputEl.value.trim();
    if (!text) {
      return;
    }

    addMessage("user", text);
    inputEl.value = "";

    var sendButton = panel.querySelector(".chatbot-send");
    sendButton.disabled = true;
    inputEl.disabled = true;

    fetch("/api/chat", {
      method: "POST",
      credentials: "same-origin",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        message: text,
        history: history.map(function (m) { return { role: m.role, content: m.content }; })
      })
    })
      .then(function (res) {
        if (!res.ok) {
          throw new Error("Request failed");
        }
        return res.json();
      })
      .then(function (data) {
        addMessage("assistant", data && data.reply ? data.reply : "Sorry, I couldn't find an answer to that.");
      })
      .catch(function () {
        renderMessage("error", "Sorry, something went wrong reaching the chat assistant. Please try again shortly.");
      })
      .finally(function () {
        sendButton.disabled = false;
        inputEl.disabled = false;
        inputEl.focus();
      });
  }

  function addMessage(role, content) {
    history.push({ role: role, content: content });
    saveHistory();
    renderMessage(role, content);
  }

  function renderMessage(role, content) {
    var el = document.createElement("div");
    el.className = "chatbot-message chatbot-message-" + role;
    el.textContent = content;
    messagesEl.appendChild(el);
    messagesEl.scrollTop = messagesEl.scrollHeight;
  }

  function loadHistory() {
    try {
      var raw = window.sessionStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch (e) {
      return [];
    }
  }

  function saveHistory() {
    try {
      window.sessionStorage.setItem(STORAGE_KEY, JSON.stringify(history.slice(-20)));
    } catch (e) {
      /* sessionStorage unavailable; conversation just won't persist across navigation */
    }
  }
})();
