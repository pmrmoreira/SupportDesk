using System.Text;
using SupportDesk.Web.Contracts;

namespace SupportDesk.Web.Ai;

// Aula 16, exercício A: o resumo, em texto, para ficar guardado no pedido.
public static class SummaryNote
{
    // O limite de um comentário: o [StringLength(2000)] do AddCommentRequest e a coluna Text.
    public const int MaxLength = 2000;

    public static string Format(SummaryResponse summary)
    {
        // «revisto por um técnico»: a nota só existe porque uma pessoa leu o resumo e carregou no botão.
        var text = new StringBuilder("Resumo da conversa (IA, revisto por um técnico)\n");
        text.Append("Problema: ").Append(summary.Problem).Append('\n');
        if (summary.ActionsTaken.Count > 0)
        {
            text.Append("Já feito:\n");
            foreach (var action in summary.ActionsTaken)
            {
                text.Append("- ").Append(action).Append('\n');
            }
        }
        if (summary.MissingInformation.Count > 0)
        {
            text.Append("Falta saber:\n");
            foreach (var missing in summary.MissingInformation)
            {
                text.Append("- ").Append(missing).Append('\n');
            }
        }
        var note = text.ToString().TrimEnd();
        return note.Length <= MaxLength ? note : note[..(MaxLength - 1)] + "…";
    }
}
