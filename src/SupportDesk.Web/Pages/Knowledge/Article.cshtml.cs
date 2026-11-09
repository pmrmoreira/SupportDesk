using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Data;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Pages.Knowledge;

// Exercício A da aula 17: um artigo e os seus trechos, pela ordem em que foram cortados.
public class ArticleModel : PageModel
{
    private readonly SupportDeskDbContext _db;

    public ArticleModel(SupportDeskDbContext db)
    {
        _db = db;
    }

    public KnowledgeArticle Article { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        var article = await _db.KnowledgeArticles
            .AsNoTracking()
            .Include(a => a.Chunks.OrderBy(c => c.Position))
            .FirstOrDefaultAsync(a => a.Slug == slug, ct);
        if (article is null)
        {
            return NotFound();
        }
        Article = article;
        return Page();
    }
}
