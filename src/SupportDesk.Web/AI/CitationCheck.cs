using System.Text.RegularExpressions;

namespace SupportDesk.Web.Ai;

public record CitationReport(string Text, IReadOnlyList<int> Cited, IReadOnlyList<int> Invalid);

// As citações do texto ([1], [2] ou [1, 2]), confirmadas contra o número de fontes que foram enviadas ao modelo.
public static partial class CitationCheck
{
    public static CitationReport Check(string text, int sourceCount)
    {
        var cited = new SortedSet<int>();
        var invalid = new SortedSet<int>();
        var clean = Marker().Replace(text, match =>
        {
            var numbers = match.Groups["numbers"].Value.Split(',').Select(n => int.Parse(n.Trim())).ToList();
            var valid = numbers.Where(n => n >= 1 && n <= sourceCount).ToList();
            cited.UnionWith(valid);
            invalid.UnionWith(numbers.Except(valid));
            // Uma fonte que não existe sai do texto (com o espaço antes dela); as outras ficam.
            return valid.Count == 0 ? "" : $"{match.Groups["space"].Value}[{string.Join(", ", valid)}]";
        });
        return new CitationReport(clean, cited.ToList(), invalid.ToList());
    }

    [GeneratedRegex(@"(?<space>[ \t]*)\[(?<numbers>\d{1,3}(?:\s*,\s*\d{1,3})*)\]")]
    private static partial Regex Marker();
}
