---
paths:
  - "**/*.html"
  - "assets/**/*.css"
---

# Design System

The visual style is modelled on `Lookup_Images/resend.com_.png` (resend.com): a dark, modern, developer-grade aesthetic — but adapted to be **simple, professional, and learner-friendly** for a government education context.

## Core aesthetic

- **Dark theme first**: near-black background (`#0a0a0a`–`#111`), white/near-white headings, muted grey body text (`#a1a1aa` range). Provide a light theme only if explicitly requested.
- **Generous whitespace** and a single centered content column (max-width ~1100–1200px). Sections are tall, calm, and clearly separated — never cramped.
- **Large, confident typography**: oversized hero headline (two lines, tight tracking) set in the serif display font (Newsreader); short one-sentence subheadings in muted grey. One clear idea per section.
- **Subtle depth, not decoration**: faint radial gradients/glows behind hero and section headers, thin 1px borders (`rgba(255,255,255,0.08)`) on cards, soft rounded corners (8–12px). No heavy shadows, no busy illustrations.
- **A single restrained accent color** (teal, `--accent`) used sparingly for links, highlighted words in headings, and primary CTAs.
- **Monochrome logo/partner rows**: sector names shown in a muted single-color strip (two rows).

## Page patterns (mirroring the reference)

- **Hero**: announcement pill, bold two-line serif headline, one-sentence supporting line, primary white pill CTA + plain text link, subtle abstract visual on the right.
- **Section rhythm**: eyebrow label or 3D-style app icon → large section heading → short paragraph → visual or card grid. Alternate centered and left-aligned section headings.
- **Card grids**: 2–3 column feature cards with a small icon, short bold title, and 1–2 line description; service cards open with a small mock-UI visual. Large "why" grids are borderless (plain icon + title + text).
- **Product screenshots** framed in dark rounded mock windows with titlebar dots and a mock URL.
- **Stat or trust band**: short row of proof points (e.g. learners served, schools connected, uptime).
- **Closing CTA section**: large centered serif statement with a primary button + "Contact us →" text link.
- **Footer**: multi-column dark footer — Services, Resources, Support — with muted links and a green "All services operational" status pill.

## Typography & spacing

- Body font: `Inter` (system UI stack fallback), 400, 16px, 1.6 line height. Headings 600 weight, tight letter-spacing (-0.02em).
- Display font: `Newsreader` serif for hero and closing-CTA headlines only (`.h-serif`).
- Spacing scale: multiples of 4px; section vertical padding ~96–128px.
- Keep all design tokens (colors, spacing, radii, fonts) in `:root` custom properties in `assets/css/style.css`.
