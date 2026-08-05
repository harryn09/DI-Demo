# Release Process

- **Source of truth**: [github.com/harryn09/DI-Demo](https://github.com/harryn09/DI-Demo). All future work happens against this repo, not a local-only copy — clone it, branch, and open PRs there. This CLAUDE.md/rules directory is the same one tracked in the repo.
- **Workflow**: create a feature branch off `main`, commit changes, push, and open a pull request into `main`. Do not push directly to `main`.
- **CI/CD**: merges to `main` trigger the GitHub Actions workflow at `.github/workflows/azure-deploy.yml`, which builds the ASP.NET Core app (`DigitalIdentitySite.csproj`) and deploys it to the Azure App Service **harrydemo002** at [harrydemo002.azurewebsites.net](https://harrydemo002.azurewebsites.net/) using `azure/webapps-deploy`.
- **Deployment credential**: the workflow authenticates via the `AZURE_WEBAPP_PUBLISH_PROFILE` GitHub Actions repository secret (downloaded from the Azure Portal for the `harrydemo002` App Service). This secret is managed in the GitHub repo settings only — never hard-coded, never committed, never printed in logs.
- **App settings/secrets** (`AzureAd__ClientSecret`, `TableStorage__ConnectionString`, etc.) live in Azure App Service Application Settings, not in the repo or the workflow file — see `tech-stack.md`.
- Manual re-deploys are not the norm; if a hotfix must bypass a PR, get sign-off first and still merge through `main` so the deployed state matches the repo.
