using SupportDesk.Evaluation;
using SupportDesk.Web.Knowledge;

namespace SupportDesk.UnitTests;

// Exercício B da aula 20: as contas do relatório, com números feitos à mão.
public class MetricsTests
{
    private static KnowledgeHit Hit(string slug) => new(1, slug, "Artigo", "Trecho", "", 0);

    [Fact]
    public void Rank_is_the_position_of_the_first_chunk_of_the_article()
    {
        IReadOnlyList<KnowledgeHit> hits = [Hit("vpn"), Hit("impressora"), Hit("impressora")];

        Assert.Equal(2, Metrics.RankOf(hits, "impressora"));
        Assert.Null(Metrics.RankOf(hits, "faturas"));
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(3, 3, true)]
    [InlineData(4, 3, false)]
    [InlineData(null, 5, false)]
    public void Hit_at_k(int? rank, int k, bool expected)
    {
        Assert.Equal(expected, Metrics.HitAt(rank, k));
    }

    [Fact]
    public void Mean_reciprocal_rank_of_first_second_and_missing()
    {
        int?[] ranks = [1, 2, null];

        // (1 + 0,5 + 0) / 3 = 0,5
        Assert.Equal(0.5, ranks.Average(Metrics.ReciprocalRank), precision: 10);
    }

    [Fact]
    public void Percentile_uses_the_nearest_rank()
    {
        double[] times = [100, 300, 200, 400, 1000, 500, 600, 700, 800, 900];

        Assert.Equal(500, Metrics.Percentile(times, 50));
        Assert.Equal(1000, Metrics.Percentile(times, 95));
        Assert.Equal(0, Metrics.Percentile([], 95));
    }

    [Fact]
    public void Fractions_and_numbers_do_not_depend_on_the_language()
    {
        Assert.Equal("12/16 (75 %)", Metrics.Fraction(12, 16));
        Assert.Equal("0.58", Metrics.Number(7 / 12.0));
    }
}
