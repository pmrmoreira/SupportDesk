using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;

    public LoginModel(SignInManager<ApplicationUser> signIn)
    {
        _signIn = signIn;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    // Para onde voltar depois de entrar (vem da página que pediu sessão).
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Esse email não é válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signIn.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            // A mesma mensagem para um email sem conta e para uma palavra-passe errada.
            ModelState.AddModelError(string.Empty, "Email ou palavra-passe errados.");
            return Page();
        }

        // Só endereços desta aplicação: um ReturnUrl para outro site seria um redirecionamento aberto.
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/Tickets");
    }
}
