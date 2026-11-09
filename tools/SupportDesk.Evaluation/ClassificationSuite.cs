using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Web.Ai;

namespace SupportDesk.Evaluation;

// A classificação da aula 15, caso a caso: acertou na categoria? E na prioridade?
public static class ClassificationSuite
{
    public static async Task<string> RunAsync(IServiceProvider services, IReadOnlyList<ClassificationCase> cases, CancellationToken ct)
    {
        var classifier = services.GetRequiredService<TicketClassifier>();
        int category = 0, priority = 0, invalid = 0;
        var times = new List<double>();
        var failures = new List<string>();

        foreach (var test in cases)
        {
            var result = await classifier.ClassifyAsync(test.Title, test.Description, ct);
            times.Add(result.Elapsed.TotalMilliseconds);
            if (result.Classification is not { } got)
            {
                invalid++;
                failures.Add($"- «{test.Title}»: sem classificação ({result.Error})");
                continue;
            }
            if (got.Category == test.Category)
            {
                category++;
            }
            else
            {
                failures.Add($"- «{test.Title}»: esperado {test.Category}, veio {got.Category}");
            }
            if (got.Priority == test.Priority)
            {
                priority++;
            }
        }

        var md = new StringBuilder();
        md.AppendLine($"## Classificação ({cases.Count} casos)").AppendLine();
        md.AppendLine("| Métrica | Valor |").AppendLine("|---|---|");
        md.AppendLine($"| Categoria certa | {Metrics.Fraction(category, cases.Count)} |");
        md.AppendLine($"| Prioridade certa (indicativa) | {Metrics.Fraction(priority, cases.Count)} |");
        md.AppendLine($"| Sem classificação | {invalid} |");
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
