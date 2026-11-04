using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SupportDesk.Web.Pages;

// Aula 8: UseStatusCodePagesWithReExecute traz para aqui as respostas de erro sem corpo das páginas
// (o NotFound() do detalhe, um endereço que não existe, ...). O código original fica na resposta.
// Sem antiforgery: um formulário que deu erro chega aqui como POST, às vezes precisamente porque o token falhou.
[IgnoreAntiforgeryToken]
public class StatusCodeModel : PageModel
{
    public int Code { get; private set; }

    public IActionResult OnGet() => Show();

    public IActionResult OnPost() => Show();

    private IActionResult Show()
    {
        // Aberta diretamente, sem um erro por trás, a página faz de conta que não existe.
        if (HttpContext.Features.Get<IStatusCodeReExecuteFeature>() is null)
        {
            return NotFound();
        }
        Code = Response.StatusCode;
        return Page();
    }
}
