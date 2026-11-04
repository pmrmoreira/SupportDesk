using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;

    public LogoutModel(SignInManager<ApplicationUser> signIn)
    {
        _signIn = signIn;
    }

    public void OnGet()
    {
    }

    // Só por POST: um link (GET) noutro site não consegue terminar a tua sessão.
    public async Task<IActionResult> OnPostAsync()
    {
        await _signIn.SignOutAsync();
        TempData["Message"] = "Sessão terminada.";
        return RedirectToPage("/Index");
    }
}
