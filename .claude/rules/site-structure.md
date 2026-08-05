# Site Structure & Conventions

- One page per service — `esl.html`, `learner-identity-broker.html`, `nsi.html`, `enrol.html` — plus `index.html` (home) and, when added, about and support/contact pages. These static pages live under `wwwroot/` (served by the ASP.NET Core host, see `tech-stack.md`); server-side auth pages (`Pages/Account.cshtml`) live at the repo root alongside `Program.cs`.
- Shared assets live under `wwwroot/assets/` (`assets/css/style.css`, `assets/js/main.js` — referenced with root-relative paths); images in `assets/`. The style reference lives in `Lookup_Images/` (reference only, kept outside `wwwroot/` so it's never shipped).
- Nav, footer, and mock-window markup are duplicated across pages — when changing one, apply the same change to all five pages.
- Mobile-first responsive: single column on mobile, grids collapse gracefully; no horizontal scroll. Breakpoints at 900px and 560px.
- Files are UTF-8 (no BOM). Beware PowerShell `Get-Content`/`Set-Content` re-encoding — it corrupts macrons and em-dashes; use `[System.IO.File]` .NET APIs with `UTF8Encoding($false)` for scripted edits.
