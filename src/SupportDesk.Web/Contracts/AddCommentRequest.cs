using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Web.Contracts;

public class AddCommentRequest
{
    [Required(ErrorMessage = "O autor é obrigatório.")]
    [StringLength(80, ErrorMessage = "O autor poder ter no máximo 80 caracteres.")]
    public string Author { get; set; } = string.Empty;
    [Required(ErrorMessage = "O comentário não pode estar vazio.")]
    [StringLength(2000, ErrorMessage = "O comentário pode ter no máximo 2000 caracteres.")]
    public string Text { get; set; } = string.Empty;

}
