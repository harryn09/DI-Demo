using System.Security.Claims;
using DigitalIdentitySite.Endpoints;
using DigitalIdentitySite.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<UserProfileStore>();
builder.Services.AddSingleton<ChatDocsStore>();
builder.Services.AddSingleton<ChatBotService>();
builder.Services.AddHttpClient("anthropic");

builder.Services.AddRazorPages();

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events.OnTokenValidated += async context =>
    {
        var principal = context.Principal;
        if (principal is null)
        {
            return;
        }

        var objectId = principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? principal.FindFirstValue("oid");
        var displayName = principal.FindFirstValue("name") ?? string.Empty;
        var email = principal.FindFirstValue("preferred_username") ?? string.Empty;

        if (!string.IsNullOrEmpty(objectId))
        {
            var store = context.HttpContext.RequestServices.GetRequiredService<UserProfileStore>();
            try
            {
                await store.UpsertUserAsync(objectId, displayName, email, context.HttpContext.RequestAborted);
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Failed to persist user profile for {ObjectId}; sign-in will continue.", objectId);
            }
        }
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error.html");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapAccountEndpoints();
app.MapChatEndpoints();

app.Run();
