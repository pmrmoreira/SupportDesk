using System.ComponentModel.DataAnnotations;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Contracts;

public class TicketQuery
{
    public TicketStatus? Status { get; set; }

    
    [Range(1, 3, ErrorMessage = "A prioridade tem de ser 1, 2 ou 3.")]
    public int? Priority { get; set; }

    [StringLength(100, ErrorMessage = "A pesquisa pode ter no máximo 100 caracteres.")]
    public string? Search { get; set; }

    
    public TicketSort Sort { get; set; } = TicketSort.Newest;

   
    [Range(1, 1_000_000, ErrorMessage = "A página começa em 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "O tamanho da página tem de estar entre 1 e 50.")]
    public int PageSize { get; set; } = 10;
}
