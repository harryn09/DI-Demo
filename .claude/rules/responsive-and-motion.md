# Must-Follow: Mobile & Scroll Animations

## Mobile-friendly is mandatory
Every page must work well on mobile — responsive layout, no horizontal scroll, touch-friendly tap targets, readable font sizes. The nav collapses to a hamburger toggle below 900px.

## Scroll animations on every section
Each page section must animate into view on scroll: add the `reveal` class (plus optional `reveal-delay-1/2/3` for stagger) and let the IntersectionObserver in `assets/js/main.js` add `visible`. Keep animations consistent site-wide, subtle and professional (fade + slide-up), and disabled under `prefers-reduced-motion` (see accessibility rules).
