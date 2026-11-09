using SupportDesk.Web.Knowledge;

namespace SupportDesk.UnitTests;

// Exercício B da aula 18: a fusão de listas (RRF), sem base de dados nem modelo.
public class RankFusionTests
{
    private static KnowledgeHit Hit(int id, string snippet = "", double score = 0) =>
        new(id, "artigo", "Artigo", $"Trecho {id}", snippet, score);

    [Fact]
    public void A_chunk_in_both_lists_beats_one_that_is_first_in_only_one()
    {
        IReadOnlyList<KnowledgeHit> text = [Hit(1), Hit(2)];
        IReadOnlyList<KnowledgeHit> semantic = [Hit(3), Hit(2)];

        var fused = RankFusion.Combine(3, text, semantic);

        Assert.Equal([2, 1, 3], fused.Select(h => h.ChunkId));
    }

    [Fact]
    public void The_score_adds_one_over_k_plus_position()
    {
        IReadOnlyList<KnowledgeHit> text = [Hit(7)];
        IReadOnlyList<KnowledgeHit> semantic = [Hit(8), Hit(7)];

        var fused = RankFusion.Combine(1, text, semantic);

        var best = Assert.Single(fused);
        Assert.Equal(7, best.ChunkId);
        Assert.Equal(1.0 / 61 + 1.0 / 62, best.Score, precision: 10);
    }

    [Fact]
    public void On_a_tie_the_first_list_wins()
    {
        IReadOnlyList<KnowledgeHit> text = [Hit(1)];
        IReadOnlyList<KnowledgeHit> semantic = [Hit(2)];

        Assert.Equal([1, 2], RankFusion.Combine(2, text, semantic).Select(h => h.ChunkId));
    }

    [Fact]
    public void The_snippet_from_the_first_list_is_kept()
    {
        IReadOnlyList<KnowledgeHit> text = [Hit(1, "[[impressora]] encravada")];
        IReadOnlyList<KnowledgeHit> semantic = [Hit(1, "Quase sempre, um documento…")];

        var fused = RankFusion.Combine(5, text, semantic);

        Assert.Equal("[[impressora]] encravada", Assert.Single(fused).Snippet);
    }

    [Fact]
    public void Take_limits_the_result_and_empty_lists_are_fine()
    {
        IReadOnlyList<KnowledgeHit> text = [Hit(1), Hit(2), Hit(3)];
        IReadOnlyList<KnowledgeHit> semantic = [];

        Assert.Equal([1, 2], RankFusion.Combine(2, text, semantic).Select(h => h.ChunkId));
        Assert.Empty(RankFusion.Combine(5, semantic, semantic));
    }
}
