using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Ai;

namespace SupportDesk.Web.Pages.Ai;

// Laboratório do RAG: um pedido inventado → fontes da base de conhecimento → rascunho citado. Não grava nada.
public class DraftModel : PageModel
{
    private readonly ReplyDrafter _drafter;

    public DraftModel(ReplyDrafter drafter)
    {
        _drafter = drafter;
    }

    [BindProperty]
    public DraftInput Input { get; set; } = new();

    public DraftResult? Result { get; private set; }

    public class DraftInput
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(120, ErrorMessage = "O título pode ter no máximo 120 caracteres.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "A descrição pode ter no máximo 2000 caracteres.")]
        public string? Description { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        Result = await _drafter.DraftAsync(Input.Title.Trim(), Input.Description?.Trim() ?? string.Empty, ct);
        return Page();
    }
}
