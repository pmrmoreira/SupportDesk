using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportDesk.Web.Ai;

namespace SupportDesk.Web.Pages.Ai;

// Laboratório da classificação: experimentar o classificador com um texto qualquer, sem gravar nada.
public class ClassifyModel : PageModel
{
    private readonly TicketClassifier _classifier;

    public ClassifyModel(TicketClassifier classifier)
    {
        _classifier = classifier;
    }

    [BindProperty]
    public ClassifyInput Input { get; set; } = new();

    public ClassificationResult? Result { get; private set; }

    public class ClassifyInput
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
        Result = await _classifier.ClassifyAsync(Input.Title.Trim(), Input.Description?.Trim() ?? string.Empty, ct);
        return Page();
    }
}
