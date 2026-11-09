using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Data;

namespace SupportDesk.Web.Knowledge;

public record KnowledgeHit(int ChunkId, string ArticleSlug, string ArticleTitle, string Heading, string Snippet, double Score);

// Exercício C da aula 18: um vetor com outro tamanho não se compara com os da coluna.
public class EmbeddingDimensionsException(string message) : Exception(message);

public class KnowledgeSearch
{
    // A configuração de pesquisa do PostgreSQL para português: tira palavras vazias e reduz as palavras ao radical.
    public const string TextConfig = "portuguese";

    // ts_headline: um excerto à volta das palavras encontradas, marcadas com [[ e ]] (ver Highlight).
    private const string HeadlineOptions = "StartSel=[[, StopSel=]], MinWords=15, MaxWords=35";

    // Aula 18: na pesquisa semântica, o excerto é o início do trecho (não há palavras para marcar).
    private const int PreviewCharacters = 200;

    private readonly SupportDeskDbContext _db;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddings;
    private readonly string _embeddingModel;

    public KnowledgeSearch(SupportDeskDbContext db, IEmbeddingGenerator<string, Embedding<float>> embeddings,
        IOptions<OllamaOptions> options)
    {
        _db = db;
        _embeddings = embeddings;
        _embeddingModel = options.Value.EmbeddingModel;
    }

    public async Task<IReadOnlyList<KnowledgeHit>> SearchTextAsync(string query, int take = 5, CancellationToken ct = default)
    {
        return await _db.KnowledgeChunks
            .Where(c => c.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(TextConfig, query)))
            .OrderByDescending(c => c.SearchVector.Rank(EF.Functions.WebSearchToTsQuery(TextConfig, query)))
            .Take(take)
            .Select(c => new KnowledgeHit(
                c.Id,
                c.Article.Slug,
                c.Article.Title,
                c.Heading,
                EF.Functions.WebSearchToTsQuery(TextConfig, query).GetResultHeadline(TextConfig, c.Text, HeadlineOptions),
                c.SearchVector.Rank(EF.Functions.WebSearchToTsQuery(TextConfig, query))))
            .ToListAsync(ct);
    }

    // Aula 18: a pergunta vira um vetor com o mesmo modelo, e os trechos mais próximos (cosseno) vêm primeiro.
    public async Task<IReadOnlyList<KnowledgeHit>> SearchSemanticAsync(string query, int take = 5, CancellationToken ct = default)
    {
        var embedding = await _embeddings.GenerateVectorAsync(query, cancellationToken: ct);
        if (embedding.Length != KnowledgeIndexer.Dimensions)
        {
            // Sem esta verificação, o PostgreSQL recusava a consulta: "different vector dimensions".
            throw new EmbeddingDimensionsException(
                $"O modelo «{_embeddingModel}» devolveu vetores com {embedding.Length} dimensões; " +
                $"a coluna Embedding espera {KnowledgeIndexer.Dimensions}.");
        }
        var queryVector = new Vector(embedding);

        return await _db.KnowledgeChunks
            // Exercício C: só os vetores do modelo configurado.
            .Where(c => c.Embedding != null && c.EmbeddingModel == _embeddingModel)
            .OrderBy(c => c.Embedding!.CosineDistance(queryVector))
            .Take(take)
            .Select(c => new KnowledgeHit(
                c.Id,
                c.Article.Slug,
                c.Article.Title,
                c.Heading,
                c.Text.Length > PreviewCharacters ? c.Text.Substring(0, PreviewCharacters) + "…" : c.Text,
                1 - c.Embedding!.CosineDistance(queryVector)))
            .ToListAsync(ct);
    }

    // Exercício A: as duas pesquisas, cada uma com o dobro dos candidatos, fundidas pela posição (RRF).
    public async Task<IReadOnlyList<KnowledgeHit>> SearchHybridAsync(string query, int take = 5, CancellationToken ct = default)
    {
        var text = await SearchTextAsync(query, take * 2, ct);
        var semantic = await SearchSemanticAsync(query, take * 2, ct);
        return RankFusion.Combine(take, text, semantic);
    }
}
