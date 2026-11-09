using System.Text;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Infrastructure;

namespace SupportDesk.Web.Ai;

// O texto que vai para o modelo: o pedido e, depois, os comentários mais recentes que couberem no orçamento.
public record Transcript(string Text, int IncludedComments, int OmittedComments);

public static class ConversationBuilder
{
    public static Transcript Build(string title, string description, IReadOnlyList<CommentResponse> comments, int maxCharacters)
    {
        // O cabeçalho entra sempre: o título e a descrição dizem o problema original.
        var header = $"Título: {title}\nDescrição: {description}";

        // Uma linha por comentário, com a hora de Lisboa que o técnico vê na página.
        // (No curso, as notas internas levam «(nota interna)»; este projeto ainda não tem notas internas.)
        var lines = comments
            .Select(c => $"[{DisplayFormat.DateTime(c.CreatedAt)}] {c.Author}: {c.Text}")
            .ToList();

        // Do fim para o princípio: o que aconteceu por último é o que mais interessa a quem pega no pedido.
        var budget = maxCharacters - header.Length;
        var kept = new List<string>();
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            // Cada linha conta o seu tamanho mais 1, a mudança de linha. O break para no primeiro comentário
            // que não cabe: o modelo lê o fim da conversa seguido, sem buracos.
            if (lines[i].Length + 1 > budget)
            {
                break;
            }
            kept.Insert(0, lines[i]);   // repõe a ordem cronológica
            budget -= lines[i].Length + 1;
        }
        var omitted = lines.Count - kept.Count;

        var text = new StringBuilder(header).Append("\n\nConversa:\n");
        if (omitted > 0)
        {
            // Sem esta linha, o modelo podia concluir que antes daquilo não aconteceu nada.
            text.Append($"[{omitted} comentários mais antigos omitidos]\n");
        }
        text.AppendJoin('\n', kept);
        return new Transcript(text.ToString(), kept.Count, omitted);
    }
}
