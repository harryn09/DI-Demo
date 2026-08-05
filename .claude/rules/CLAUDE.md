# CLAUDE.md

A public-facing website for the **Digital Identity Team, Ministry of Education (New Zealand)**, introducing the Ministry's four Digital Identity Services (ESL, Learner Identity Broker, NSI, Enrol) to the education sector.

Static HTML/CSS/JS pages (homepage `index.html`, one page per service, shared assets in `assets/`) served from `wwwroot/` by a thin ASP.NET Core host (`Program.cs`), which also provides EntraID sign-in/out and an Azure Table Storage-backed user profile (see `tech-stack.md`). Test locally with `dotnet run` from the repo root (needed for auth); `dotnet-serve --directory c:\Projects\DemoSite\wwwroot --port 8080` still works for quick static-only checks.

Detailed project rules live in `.claude/rules/` (loaded automatically), one topic per file:

- `project-overview.md` — what this site is and who it serves
- `services.md` — factual reference for the four services (source of truth for content)
- `design-system.md` — resend.com-inspired dark design system *(loads for HTML/CSS files)*
- `content-and-tone.md` — plain language, NZ English, te reo Māori *(loads for HTML files)*
- `accessibility.md` — WCAG 2.2 AA requirements (non-negotiable)
- `tech-stack.md` — Azure App Service + Azure SQL deployment target
- `site-structure.md` — page/file conventions and encoding pitfalls
- `visual-verification.md` — must-follow: screenshot & compare after major UI changes
- `responsive-and-motion.md` — must-follow: mobile-friendly + scroll animations
- `things-to-avoid.md` — anti-patterns
- `release-process.md` — GitHub repo, branch/PR workflow, and GitHub Actions deploy to Azure App Service
