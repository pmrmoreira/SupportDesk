namespace SupportDesk.Web.Models;

// Um artigo da base de conhecimento, importado de um ficheiro .md da pasta Articles.
public class KnowledgeArticle
{
    public int Id { get; set; }

    // O nome do ficheiro sem .md: identifica o artigo de uma importação para a outra.
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    // SHA-256 do ficheiro: se não mudou, a importação não mexe no artigo.
    public string ContentHash { get; set; } = string.Empty;
    public DateTimeOffset ImportedAt { get; set; }

    public List<KnowledgeChunk> Chunks { get; set; } = [];
}
