using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Web.Contracts;

// Aula 10: o autor deixa de vir do formulário ou do JSON; é quem tem sessão (ou o token).
public class AddCommentRequest
{
    [Required(ErrorMessage = "O comentário não pode estar vazio.")]
    [StringLength(2000, ErrorMessage = "O comentário pode ter no máximo 2000 caracteres.")]
    public string Text { get; set; } = string.Empty;
}
