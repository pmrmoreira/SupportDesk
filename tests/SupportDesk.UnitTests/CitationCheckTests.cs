using SupportDesk.Web.Ai;
using SupportDesk.Web.Models;
using SupportDesk.Web.Services;

namespace SupportDesk.UnitTests;

// Exercício B da aula 19: as citações confirmadas contra as fontes, sem modelo nenhum.
public class CitationCheckTests
{
    [Fact]
    public void Valid_citations_stay_in_the_text()
    {
        var report = CitationCheck.Check("Reinicia a impressora [1]. Esvazia a fila [2].", sourceCount: 4);

        Assert.Equal("Reinicia a impressora [1]. Esvazia a fila [2].", report.Text);
        Assert.Equal([1, 2], report.Cited);
        Assert.Empty(report.Invalid);
    }

    [Fact]
    public void A_citation_without_a_source_is_removed_with_its_space()
    {
        var report = CitationCheck.Check("Vê o artigo das faturas [7].", sourceCount: 4);

        Assert.Equal("Vê o artigo das faturas.", report.Text);
        Assert.Empty(report.Cited);
        Assert.Equal([7], report.Invalid);
    }

    [Fact]
    public void In_a_list_only_the_valid_numbers_stay()
    {
        var report = CitationCheck.Check("Os dois dizem o mesmo [1, 7].", sourceCount: 4);

        Assert.Equal("Os dois dizem o mesmo [1].", report.Text);
        Assert.Equal([1], report.Cited);
        Assert.Equal([7], report.Invalid);
    }

    [Theory]
    [InlineData("Sem fontes nenhumas.")]
    [InlineData("O ano [2026] não é uma citação.")]
    public void Text_without_citations_does_not_change(string text)
    {
        var report = CitationCheck.Check(text, sourceCount: 4);

        Assert.Equal(text, report.Text);
        Assert.Empty(report.Cited);
        Assert.Empty(report.Invalid);
    }

    [Fact]
    public void Zero_is_not_a_source()
    {
        var report = CitationCheck.Check("Primeira fonte [0].", sourceCount: 4);

        Assert.Equal([0], report.Invalid);
    }

    [Fact]
    public void Only_the_cited_sources_go_to_the_end_of_the_comment()
    {
        TicketAiDraftSource[] cited =
        [
            new() { Number = 2, Heading = "A impressora não imprime — Sintomas" },
            new() { Number = 1, Heading = "A impressora não imprime — O que fazer" }
        ];

        var comment = DraftService.WithSources("Texto [1] [2].", cited);

        Assert.Equal("Texto [1] [2].\n\nFontes:\n[1] A impressora não imprime — O que fazer\n[2] A impressora não imprime — Sintomas", comment);
    }
}
