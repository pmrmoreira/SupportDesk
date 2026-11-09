using System.Globalization;
using SupportDesk.Web.Knowledge;

namespace SupportDesk.Evaluation;

// As contas da avaliação, sem base de dados nem modelo: testam-se sozinhas.
public static class Metrics
{
    // A posição (1, 2, 3…) do primeiro trecho do artigo esperado, ou null se não aparecer.
    public static int? RankOf(IReadOnlyList<KnowledgeHit> hits, string articleSlug)
    {
        for (var i = 0; i < hits.Count; i++)
        {
            if (hits[i].ArticleSlug == articleSlug)
            {
                return i + 1;
            }
        }
        return null;
    }

    // hit@k: o artigo esperado apareceu nos k primeiros?
    public static bool HitAt(int? rank, int k) => rank is not null && rank <= k;

    // 1 / posição: 1 em primeiro, 0,5 em segundo… 0 se não apareceu. A média é o MRR.
    public static double ReciprocalRank(int? rank) => rank is null ? 0 : 1.0 / rank.Value;

    // Percentil pelo método do posto mais próximo: p95 = o valor abaixo do qual ficam 95 % das medições.
    public static double Percentile(IReadOnlyList<double> values, double percentile)
    {
        if (values.Count == 0)
        {
            return 0;
        }
        var sorted = values.Order().ToList();
        var index = (int)Math.Ceiling(percentile / 100 * sorted.Count) - 1;
        return sorted[Math.Clamp(index, 0, sorted.Count - 1)];
    }

    // «12/16 (75 %)»: os números do relatório não dependem da língua do Windows.
    public static string Fraction(int part, int total) =>
        total == 0 ? "0/0" : string.Create(CultureInfo.InvariantCulture, $"{part}/{total} ({100.0 * part / total:0} %)");

    public static string Number(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
