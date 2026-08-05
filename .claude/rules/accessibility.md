# Accessibility (non-negotiable)

- WCAG 2.2 AA minimum — this is a government website.
- Contrast: body text on dark background must meet 4.5:1 (verify muted greys).
- Full keyboard navigation, visible focus states, semantic HTML landmarks, alt text on all imagery; decorative visuals get `aria-hidden="true"`.
- Respect `prefers-reduced-motion`; animations are subtle fades/slides only, and the scroll-reveal script must show all content immediately when reduced motion is requested.
