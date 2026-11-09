using System.Text.RegularExpressions;

namespace SupportDesk.Web.Knowledge;

public record ArticleChunk(string Heading, string Text);

public record ParsedArticle(string Title, IReadOnlyList<ArticleChunk> Chunks);

// Divide um artigo em Markdown em trechos: um por secção (##), e mais do que um se a secção passar do máximo.
public static partial class MarkdownChunker
{
    public const int MaxChunkCharacters = 800;

    public static ParsedArticle Parse(string markdown, string fallbackTitle)
    {
        var title = fallbackTitle;
        // A primeira "secção" é o texto antes do primeiro ##: a introdução.
        var sections = new List<(string Heading, List<string> Blocks)> { ("", []) };

        // Um bloco é o texto entre linhas vazias: um parágrafo, uma lista ou um título.
        foreach (var block in BlankLines().Split(markdown.ReplaceLineEndings("\n").Trim()))
        {
            var text = block.Trim();
            // Exercício B: um título acaba no fim da linha; o que vem a seguir no bloco é texto (ou outro título).
            while (text.StartsWith("# ") || text.StartsWith("## "))
            {
                var end = text.IndexOf('\n');
                var line = end < 0 ? text : text[..end];
                if (line.StartsWith("# "))
                {
                    title = line[2..].Trim();
                }
                else
                {
                    sections.Add((line[3..].Trim(), []));
                }
                text = end < 0 ? "" : text[(end + 1)..].Trim();
            }
            if (text.Length > 0)
            {
                sections[^1].Blocks.Add(text);
            }
        }

        var chunks = new List<ArticleChunk>();
        foreach (var (heading, blocks) in sections)
        {
            var fullHeading = heading.Length == 0 ? title : $"{title} — {heading}";
            foreach (var text in Pack(blocks))
            {
                chunks.Add(new ArticleChunk(fullHeading, text));
            }
        }
        return new ParsedArticle(title, chunks);
    }

    // Junta blocos seguidos até ao máximo de caracteres; um bloco maior do que o máximo fica sozinho.
    private static IEnumerable<string> Pack(List<string> blocks)
    {
        var current = "";
        foreach (var block in blocks)
        {
            if (current.Length > 0 && current.Length + 2 + block.Length > MaxChunkCharacters)
            {
                yield return current;
                current = "";
            }
            current = current.Length == 0 ? block : current + "\n\n" + block;
        }
        if (current.Length > 0)
        {
            yield return current;
        }
    }

    [GeneratedRegex(@"\n\s*\n")]
    private static partial Regex BlankLines();
}
