using Microsoft.AspNetCore.Html;

namespace SupportDesk.Web.Knowledge;

public record HighlightPart(string Text, bool IsMatch);

// O excerto do ts_headline traz as palavras encontradas entre [[ e ]]. Aqui viram <mark>, e o resto do
// texto é codificado como HTML: nada que venha do artigo se transforma em marcação da página.
public static class Highlight
{
    public static IHtmlContent ToHtml(string snippet)
    {
        var html = new HtmlContentBuilder();
        foreach (var part in Parts(snippet))
        {
            if (part.IsMatch)
            {
                html.AppendHtml("<mark>").Append(part.Text).AppendHtml("</mark>");
            }
            else
            {
                html.Append(part.Text);
            }
        }
        return html;
    }

    public static IReadOnlyList<HighlightPart> Parts(string snippet)
    {
        var parts = new List<HighlightPart>();
        var rest = snippet;
        while (rest.Length > 0)
        {
            var start = rest.IndexOf("[[", StringComparison.Ordinal);
            var end = start < 0 ? -1 : rest.IndexOf("]]", start + 2, StringComparison.Ordinal);
            if (start < 0 || end < 0)
            {
                parts.Add(new HighlightPart(rest, false));
                break;
            }
            if (start > 0)
            {
                parts.Add(new HighlightPart(rest[..start], false));
            }
            parts.Add(new HighlightPart(rest[(start + 2)..end], true));
            rest = rest[(end + 2)..];
        }
        return parts;
    }
}
