namespace SupportDesk.Web.Knowledge;

// Exercício A da aula 18: junta listas ordenadas numa só (Reciprocal Rank Fusion, RRF).
// Cada trecho soma 1 / (K + posição) em cada lista onde aparece. Só conta a posição, não a pontuação:
// as pontuações do texto (ts_rank) e do significado (cosseno) não estão na mesma escala.
public static class RankFusion
{
    // O valor do artigo original do RRF: amortece a diferença entre o 1.º e o 2.º lugar.
    public const int K = 60;

    public static IReadOnlyList<KnowledgeHit> Combine(int take, params IReadOnlyList<KnowledgeHit>[] lists)
    {
        // Order: a ordem de chegada, para desempatar (um Dictionary não garante a ordem em que é percorrido).
        var scores = new Dictionary<int, (KnowledgeHit Hit, double Score, int Order)>();
        var arrival = 0;
        foreach (var list in lists)
        {
            for (var position = 1; position <= list.Count; position++)
            {
                var hit = list[position - 1];
                var points = 1.0 / (K + position);
                // O primeiro resultado encontrado fica (o da pesquisa textual traz as palavras marcadas).
                scores[hit.ChunkId] = scores.TryGetValue(hit.ChunkId, out var current)
                    ? (current.Hit, current.Score + points, current.Order)
                    : (hit, points, arrival++);
            }
        }
        return scores.Values
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.Order)
            .Take(take)
            .Select(s => s.Hit with { Score = s.Score })
            .ToList();
    }
}
