using NpgsqlTypes;
using Pgvector;

namespace SupportDesk.Web.Models;

// Um trecho de um artigo: o que a pesquisa encontra (e, na aula 19, o que a IA cita).
public class KnowledgeChunk
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public KnowledgeArticle Article { get; set; } = null!;

    // A ordem do trecho no artigo: 0, 1, 2…
    public int Position { get; set; }

    // O título do artigo e o da secção, para o trecho se perceber sozinho.
    public string Heading { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    // Coluna gerada pelo PostgreSQL a partir de Heading e Text: a aplicação nunca a escreve.
    public NpgsqlTsVector SearchVector { get; set; } = null!;

    // Aula 18: o significado do trecho, como um vetor de 1024 números (null até ser gerado).
    public Vector? Embedding { get; set; }

    // Exercício C da aula 18: o modelo que gerou o vetor. Vetores de modelos diferentes não se comparam.
    public string? EmbeddingModel { get; set; }
}
