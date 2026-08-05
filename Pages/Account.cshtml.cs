using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DigitalIdentitySite.Pages;

[Authorize]
public class AccountModel : PageModel
{
    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public void OnGet()
    {
        DisplayName = User.FindFirstValue("name") ?? User.Identity?.Name ?? "there";
        Email = User.FindFirstValue("preferred_username") ?? User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    }
}
