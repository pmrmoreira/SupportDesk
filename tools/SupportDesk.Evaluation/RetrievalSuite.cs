using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Data;
using SupportDesk.Web.Knowledge;

namespace SupportDesk.Evaluation;

// As pesquisas das aulas 17 e 18: o artigo esperado aparece? Em que posição?
public static class RetrievalSuite
{
    // Os resultados que contam: os 5 primeiros, como na página da base de conhecimento.
    public const int Top = 5;

    public static async Task<string> RunAsync(IServiceProvider services, IReadOnlyList<RetrievalCase> cases, CancellationToken ct)
    {
        var md = new StringBuilder();
        md.AppendLine($"## Pesquisa ({cases.Count} perguntas, {Top} resultados)").AppendLine();

        var db = services.GetRequiredService<SupportDeskDbContext>();
        var model = services.GetRequiredService<IOptions<OllamaOptions>>().Value.EmbeddingModel;
        var chunks = await db.KnowledgeChunks.CountAsync(ct);
        var embedded = await db.KnowledgeChunks.CountAsync(c => c.Embedding != null && c.EmbeddingModel == model, ct);
        if (chunks == 0)
        {
            return md.AppendLine("A base de conhecimento está vazia: importa os artigos e gera os embeddings (aulas 17 e 18).")
                .AppendLine().ToString();
        }
        md.AppendLine($"Trechos: {chunks} · com embedding de {model}: {embedded}").AppendLine();

        var search = services.GetRequiredService<KnowledgeSearch>();
        var modes = new (string Name, Func<string, Task<IReadOnlyList<KnowledgeHit>>> Run)[]
        {
            ("Texto", query => search.SearchTextAsync(query, Top, ct)),
            ("Significado", query => search.SearchSemanticAsync(query, Top, ct)),
            ("Híbrida", query => search.SearchHybridAsync(query, Top, ct))
        };
        var ranks = modes.ToDictionary(m => m.Name, _ => new List<int?>());
        // Exercício C: os resultados de cada modo, para explicar as falhas.
        var found = modes.ToDictionary(m => m.Name, _ => new List<IReadOnlyList<KnowledgeHit>>());
        foreach (var test in cases)
        {
            foreach (var (name, run) in modes)
            {
                var hits = await run(test.Query);
                found[name].Add(hits);
                ranks[name].Add(Metrics.RankOf(hits, test.Article));
            }
        }

        md.AppendLine("| Modo | hit@1 | hit@3 | MRR |").AppendLine("|---|---|---|---|");
        foreach (var (name, _) in modes)
        {
            var list = ranks[name];
            md.AppendLine($"| {name} | {Metrics.Number(list.Count(r => Metrics.HitAt(r, 1)) / (double)cases.Count)} " +
                $"| {Metrics.Number(list.Count(r => Metrics.HitAt(r, 3)) / (double)cases.Count)} " +
                $"| {Metrics.Number(list.Average(Metrics.ReciprocalRank))} |");
        }

        // Caso a caso: a posição do artigo esperado em cada modo (— = não apareceu nos 5 primeiros).
        md.AppendLine().AppendLine("| Pergunta | Artigo esperado | Texto | Significado | Híbrida |").AppendLine("|---|---|---|---|---|");
        for (var i = 0; i < cases.Count; i++)
        {
            md.AppendLine($"| {cases[i].Query} | {cases[i].Article} | {Position(ranks["Texto"][i])} " +
                $"| {Position(ranks["Significado"][i])} | {Position(ranks["Híbrida"][i])} |");
        }

        // Exercício C: quando a híbrida não pôs o artigo certo em 1.º, o que veio em vez dele?
        var misses = Enumerable.Range(0, cases.Count).Where(i => ranks["Híbrida"][i] != 1).ToList();
        md.AppendLine().AppendLine($"### Falhas da híbrida ({misses.Count})").AppendLine();
        foreach (var i in misses)
        {
            md.AppendLine($"- «{cases[i].Query}» (esperado: {cases[i].Article})");
            foreach (var (name, _) in modes)
            {
                var top = found[name][i].Take(3).Select(h => h.Heading).ToList();
                md.AppendLine($"  - {name}: {(top.Count == 0 ? "nada" : string.Join("; ", top))}");
            }
        }
        return md.AppendLine().ToString();
    }

    private static string Position(int? rank) => rank is null ? "—" : $"{rank}.º";
}
