using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Web.Contracts;

public class CreateTicketRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "O título deve ter entre 5 a 120 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 a 2000 caracteres.")]
    public string? Description { get; set; }

    // Exercício C da aula 3: sem "= 2", um pedido sem prioridade chegava com 0
    // e a base recusava-o (CK_Tickets_Priority) com um erro 500.
    [Range(1, 3, ErrorMessage = "A prioridade deve ser 1, 2 ou 3.")]
    public int Priority { get; set; } = 2;
}
