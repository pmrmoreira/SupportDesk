using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Data;

namespace SupportDesk.Web.Pages.Ai;

// Exercício A da aula 21: quanto se usa a IA, a partir do que já fica gravado (aulas 15, 16 e 19).
public class UsageModel : PageModel
{
    // Uma linha por funcionalidade. percentile_disc dá um valor que existe mesmo (o tempo de uma chamada).
    private const string Sql = """
        SELECT 'Sugestões de classificação' AS "Feature", count(*)::int AS "Calls",
               coalesce(sum("InputTokens"), 0)::bigint AS "InputTokens", coalesce(sum("OutputTokens"), 0)::bigint AS "OutputTokens",
               coalesce(percentile_disc(0.5) WITHIN GROUP (ORDER BY "DurationMs"), 0) AS "P50Ms",
               coalesce(percentile_disc(0.95) WITHIN GROUP (ORDER BY "DurationMs"), 0) AS "P95Ms"
        FROM "AiSuggestions"
        UNION ALL
        SELECT 'Resumos (o último de cada pedido)', count(*)::int,
               coalesce(sum("InputTokens"), 0)::bigint, coalesce(sum("OutputTokens"), 0)::bigint,
               coalesce(percentile_disc(0.5) WITHIN GROUP (ORDER BY "DurationMs"), 0),
               coalesce(percentile_disc(0.95) WITHIN GROUP (ORDER BY "DurationMs"), 0)
        FROM "AiSummaries"
        UNION ALL
        SELECT 'Rascunhos de resposta', count(*)::int,
               coalesce(sum("InputTokens"), 0)::bigint, coalesce(sum("OutputTokens"), 0)::bigint,
               coalesce(percentile_disc(0.5) WITHIN GROUP (ORDER BY "DurationMs"), 0),
               coalesce(percentile_disc(0.95) WITHIN GROUP (ORDER BY "DurationMs"), 0)
        FROM "AiDrafts"
        """;

    private readonly SupportDeskDbContext _db;

    public UsageModel(SupportDeskDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<UsageRow> Rows { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        // SQL à mão: o LINQ do EF Core não traduz percentis. O SQL é fixo, sem nada vindo de quem usa a página.
        Rows = await _db.Database.SqlQueryRaw<UsageRow>(Sql).ToListAsync(ct);
    }

    public record UsageRow(string Feature, int Calls, long InputTokens, long OutputTokens, int P50Ms, int P95Ms);
}
