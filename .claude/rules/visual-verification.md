# Must-Follow: Visual Verification

For every major UI change:

1. Take a screenshot of the affected page(s) and compare it against the style reference `Lookup_Images/resend.com_.png` and the established design system. Correct any drift (spacing, typography, colors, alignment) before considering the change done.
2. Verify at a ~375px-wide viewport in addition to desktop (1440px).

How to screenshot on this machine (no Node/Playwright — use headless Edge):

- Desktop: `& "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" --headless=new --disable-gpu --hide-scrollbars --user-data-dir=<scratch>\edgeprofile --window-size=1440,<height> --virtual-time-budget=8000 --screenshot=<out.png> <url>`
- Mobile: Edge's new headless mode enforces a ~500px minimum window width, so a 375px `--window-size` silently crops instead of reflowing. Render the page inside a 375px-wide `<iframe>` in a wrapper HTML file and screenshot that at window width ~520px.
- The reference image is only 127×1230px; to inspect it, crop into slices and upscale 5× with System.Drawing before comparing.
