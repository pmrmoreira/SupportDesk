using SupportDesk.Web.Ai;
using SupportDesk.Web.Contracts;

namespace SupportDesk.UnitTests;

// Aula 16, exercício B: o orçamento da conversa, sem modelo nenhum. Corre em milissegundos.
public class ConversationBuilderTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);

    // O comentário id é criado às 09:00 UTC mais id minutos.
    private static CommentResponse Comment(int id, string author, string text) =>
        new(id, author, text, Start.AddMinutes(id));

    [Fact]
    public void Everything_fits_in_a_large_budget()
    {
        CommentResponse[] comments = [Comment(1, "Ana", "Já reiniciei."), Comment(2, "Técnico", "Vou ver.")];

        var transcript = ConversationBuilder.Build("Impressora", "Não imprime.", comments, maxCharacters: 6000);

        Assert.Equal(2, transcript.IncludedComments);
        Assert.Equal(0, transcript.OmittedComments);
        Assert.StartsWith("Título: Impressora\nDescrição: Não imprime.", transcript.Text);
        // 09:01 UTC é 10:01 em Lisboa a 12 de outubro (hora de verão, UTC+1): a hora que o técnico vê na página.
        Assert.Contains("[12/10/2026 10:01] Ana: Já reiniciei.", transcript.Text);
        Assert.DoesNotContain("omitidos", transcript.Text);
    }

    [Fact]
    public void A_small_budget_keeps_the_newest_comments()
    {
        var comments = Enumerable.Range(1, 10).Select(i => Comment(i, "Ana", $"Mensagem {i} " + new string('x', 80))).ToArray();

        var transcript = ConversationBuilder.Build("Impressora", "Não imprime.", comments, maxCharacters: 400);

        // Não se fixa quantos cabem (depende do tamanho das linhas); fixam-se as regras.
        Assert.Equal(10, transcript.IncludedComments + transcript.OmittedComments);
        Assert.True(transcript.OmittedComments > 0);
        Assert.Contains($"[{transcript.OmittedComments} comentários mais antigos omitidos]", transcript.Text);
        Assert.Contains("Mensagem 10 ", transcript.Text);
        // O espaço no fim conta: sem ele, "Mensagem 1" também apanhava a "Mensagem 10".
        Assert.DoesNotContain("Mensagem 1 ", transcript.Text);
    }

    // No curso, este teste confirma a marca «(nota interna)»; este projeto ainda não tem notas internas.
    // Em vez disso, confirma que o ciclo do fim para o princípio, com Insert(0, …), repõe a ordem cronológica.
    [Fact]
    public void The_kept_comments_stay_in_chronological_order()
    {
        CommentResponse[] comments = [Comment(1, "Ana", "Primeiro."), Comment(2, "Técnico", "Segundo."), Comment(3, "Ana", "Terceiro.")];

        var transcript = ConversationBuilder.Build("Monitor", "Pisca.", comments, maxCharacters: 6000);

        var first = transcript.Text.IndexOf("Primeiro.", StringComparison.Ordinal);
        var second = transcript.Text.IndexOf("Segundo.", StringComparison.Ordinal);
        var third = transcript.Text.IndexOf("Terceiro.", StringComparison.Ordinal);
        Assert.True(first >= 0 && first < second && second < third);
    }

    [Fact]
    public void The_ticket_is_always_there_even_without_room_for_comments()
    {
        CommentResponse[] comments = [Comment(1, "Ana", "Olá.")];

        var transcript = ConversationBuilder.Build("Monitor", "Pisca.", comments, maxCharacters: 10);

        Assert.StartsWith("Título: Monitor", transcript.Text);
        Assert.Equal(0, transcript.IncludedComments);
        Assert.Equal(1, transcript.OmittedComments);
    }
}
