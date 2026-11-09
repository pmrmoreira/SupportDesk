using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Web.Ai;

namespace SupportDesk.Evaluation;

// O RAG da aula 19: as fontes certas chegaram ao modelo? O rascunho cita-as?
public static class RagSuite
{
    public static async Task<string> RunAsync(IServiceProvider services, IReadOnlyList<RagCase> cases, CancellationToken ct)
    {
        var drafter = services.GetRequiredService<ReplyDrafter>();
        int made = 0, retrieved = 0, citesExpected = 0, withCitation = 0, withInvalid = 0, clarification = 0;
        var times = new List<double>();
        var failures = new List<string>();

        foreach (var test in cases)
        {
            var result = await drafter.DraftAsync(test.Title, test.Description, ct);
            times.Add(result.Elapsed.TotalMilliseconds);
            if (result.Sources.Any(s => s.ArticleSlug == test.Article))
            {
                retrieved++;
            }
            if (result.Draft is not { } draft)
            {
                failures.Add($"- «{test.Title}»: sem rascunho ({result.Error})");
                continue;
            }
            made++;
            var cited = result.Citations?.Cited ?? [];
            if (cited.Count > 0)
            {
                withCitation++;
            }
            if (result.Citations?.Invalid.Count > 0)
            {
                withInvalid++;
            }
            if (draft.NeedsClarification)
            {
                clarification++;
            }
            var citedArticles = result.Sources.Where(s => cited.Contains(s.Number)).Select(s => s.ArticleSlug).Distinct().ToList();
            if (citedArticles.Contains(test.Article))
            {
                citesExpected++;
            }
            else
            {
                failures.Add($"- «{test.Title}»: esperado {test.Article}, citou {(citedArticles.Count == 0 ? "nada" : string.Join(", ", citedArticles))}");
            }
        }

        var md = new StringBuilder();
        md.AppendLine($"## Rascunhos com fontes ({cases.Count} casos)").AppendLine();
        md.AppendLine("| Métrica | Valor |").AppendLine("|---|---|");
        md.AppendLine($"| Rascunhos feitos | {Metrics.Fraction(made, cases.Count)} |");
        md.AppendLine($"| O artigo esperado estava nas fontes | {Metrics.Fraction(retrieved, cases.Count)} |");
        md.AppendLine($"| Citam o artigo esperado | {Metrics.Fraction(citesExpected, cases.Count)} |");
        md.AppendLine($"| Com pelo menos uma citação válida | {Metrics.Fraction(withCitation, cases.Count)} |");
        md.AppendLine($"| Com citações sem fonte (retiradas) | {Metrics.Fraction(withInvalid, cases.Count)} |");
        md.AppendLine($"| Pediram esclarecimento | {Metrics.Fraction(clarification, cases.Count)} |");
        md.AppendLine($"| Tempo p50 / p95 | {Metrics.Percentile(times, 50):0} ms / {Metrics.Percentile(times, 95):0} ms |");
        md.AppendLine();
        md.AppendLine(failures.Count == 0 ? "Sem falhas." : "Falhas:");
        foreach (var failure in failures)
        {
            md.AppendLine(failure);
        }
        return md.AppendLine().ToString();
    }
}
