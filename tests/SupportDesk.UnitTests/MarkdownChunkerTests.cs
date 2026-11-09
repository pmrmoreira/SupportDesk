using SupportDesk.Web.Knowledge;

namespace SupportDesk.UnitTests;

// Exercício B da aula 17: o divisor de artigos, testado sem base de dados nem ficheiros.
public class MarkdownChunkerTests
{
    [Fact]
    public void Each_section_becomes_a_chunk_with_the_article_title()
    {
        var markdown = "# Impressora\n\nIntrodução.\n\n## Sintomas\n\nNão imprime.\n\n## O que fazer\n\nReinicia.";

        var article = MarkdownChunker.Parse(markdown, "ficheiro");

        ArticleChunk[] expected =
        [
            new("Impressora", "Introdução."),
            new("Impressora — Sintomas", "Não imprime."),
            new("Impressora — O que fazer", "Reinicia.")
        ];
        Assert.Equal("Impressora", article.Title);
        Assert.Equal(expected, article.Chunks);
    }

    [Fact]
    public void Without_a_title_the_file_name_is_used()
    {
        var article = MarkdownChunker.Parse("Só texto.", "sem-titulo");

        Assert.Equal("sem-titulo", article.Title);
        Assert.Equal("sem-titulo", Assert.Single(article.Chunks).Heading);
    }

    [Fact]
    public void A_long_section_is_split_by_paragraphs()
    {
        var paragraph = new string('a', 300);
        var markdown = "# Longo\n\n## Passos\n\n" + string.Join("\n\n", Enumerable.Repeat(paragraph, 5));

        var article = MarkdownChunker.Parse(markdown, "longo");

        // 300 + 2 + 300 = 602 cabe; mais um parágrafo dava 904. Fica 2 + 2 + 1.
        Assert.Equal([602, 602, 300], article.Chunks.Select(c => c.Text.Length));
    }

    [Fact]
    public void Windows_line_endings_give_the_same_chunks()
    {
        var unix = MarkdownChunker.Parse("# T\n\nA.\n\n## S\n\nB.", "t");
        var windows = MarkdownChunker.Parse("# T\r\n\r\nA.\r\n\r\n## S\r\n\r\nB.", "t");

        Assert.Equal(unix.Chunks, windows.Chunks);
    }

    [Fact]
    public void A_heading_followed_directly_by_text_is_still_a_heading()
    {
        var article = MarkdownChunker.Parse("# Wi-Fi\n## Como pedir\nPede com um dia de antecedência.", "wifi");

        Assert.Equal("Wi-Fi", article.Title);
        var chunk = Assert.Single(article.Chunks);
        Assert.Equal("Wi-Fi — Como pedir", chunk.Heading);
        Assert.Equal("Pede com um dia de antecedência.", chunk.Text);
    }
}
