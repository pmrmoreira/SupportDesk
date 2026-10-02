using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupportDesk.Web.Contracts;
public class CreateTicketRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "O título deve ter entre 5 a 120 caracteres.")]
    public string Title { get; set; } = string.Empty;
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 a 2000 caracteres.")]
    public string? Description {  get; set; }
    public int Priority { get; set; }
}
