using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Web.Knowledge;

// A secção "Knowledge" do appsettings.json.
public class KnowledgeOptions
{
    public const string Section = "Knowledge";

    // A pasta dos artigos .md, relativa à pasta do projeto (ContentRootPath).
    [Required]
    public string Folder { get; set; } = "Articles";
}
