using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace DigitalIdentitySite.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapGet("/account/signin", (string? returnUrl) =>
            Results.Challenge(
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    RedirectUri = string.IsNullOrEmpty(returnUrl) ? "/account" : returnUrl
                },
                new[] { OpenIdConnectDefaults.AuthenticationScheme }));

        app.MapGet("/account/signout", () =>
            Results.SignOut(
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = "/" },
                new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme }));

        app.MapGet("/api/me", (ClaimsPrincipal user) =>
        {
            if (user.Identity is not { IsAuthenticated: true })
            {
                return Results.Json(new { isAuthenticated = false });
            }

            var name = user.FindFirstValue("name") ?? user.Identity.Name ?? string.Empty;
            var email = user.FindFirstValue("preferred_username")
                ?? user.FindFirstValue(ClaimTypes.Email)
                ?? string.Empty;

            return Results.Json(new { isAuthenticated = true, name, email });
        }).AllowAnonymous();
    }
}
