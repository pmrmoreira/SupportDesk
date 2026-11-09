using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Pgvector;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Data;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Knowledge;

public record IndexReport(int Indexed, int Batches, long? InputTokens, TimeSpan Elapsed, string? Error);

// Gera os embeddings dos trechos que ainda não os têm, em lotes, e guarda-os na coluna vector.
public class KnowledgeIndexer
{
    // Trechos por pedido ao Ollama: menos pedidos HTTP, sem mandar a base inteira de uma vez.
    public const int BatchSize = 16;

    // Tem de ser igual ao vector(1024) da coluna: é o tamanho dos vetores do bge-m3.
    public const int Dimensions = 1024;

    private readonly SupportDeskDbContext _db;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddings;
    private readonly OllamaOptions _options;
    private readonly ILogger<KnowledgeIndexer> _logger;

    public KnowledgeIndexer(SupportDeskDbContext db, IEmbeddingGenerator<string, Embedding<float>> embeddings,
        IOptions<OllamaOptions> options, ILogger<KnowledgeIndexer> logger)
    {
        _db = db;
        _embeddings = embeddings;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IndexReport> IndexAsync(CancellationToken ct = default)
    {
        // Exercício C: sem vetor, ou com um vetor de outro modelo.
        var model = _options.EmbeddingModel;
        var pending = await _db.KnowledgeChunks
            .Where(c => c.Embedding == null || c.EmbeddingModel != model)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);

        var stopwatch = Stopwatch.StartNew();
        int indexed = 0, batches = 0;
        long? tokens = null;
        try
        {
            foreach (var batch in pending.Chunk(BatchSize))
            {
                var embeddings = await _embeddings.GenerateAsync(batch.Select(EmbeddingText), cancellationToken: ct);
                var wrong = embeddings.FirstOrDefault(e => e.Vector.Length != Dimensions);
                if (wrong is not null)
                {
                    return new IndexReport(indexed, batches, tokens, stopwatch.Elapsed,
                        $"O modelo «{_options.EmbeddingModel}» devolveu vetores com {wrong.Vector.Length} dimensões; " +
                        $"a coluna Embedding espera {Dimensions}.");
                }
                for (var i = 0; i < batch.Length; i++)
                {
                    batch[i].Embedding = new Vector(embeddings[i].Vector);
                    batch[i].EmbeddingModel = model;
                }
                // Um SaveChanges por lote: se o Ollama falhar a meio, os lotes anteriores ficam guardados.
                await _db.SaveChangesAsync(ct);
                indexed += batch.Length;
                batches++;
                tokens = (tokens ?? 0) + (embeddings.Usage?.InputTokenCount ?? 0);
            }
        }
        catch (Exception ex) when (AiErrors.IsExpected(ex))
        {
            _logger.LogWarning(ex, "Embeddings: a chamada ao Ollama falhou ao fim de {Indexed} trechos", indexed);
            return new IndexReport(indexed, batches, tokens, stopwatch.Elapsed,
                AiErrors.Describe(ex, _options, _options.EmbeddingModel));
        }

        _logger.LogInformation("Embeddings: {Indexed} trechos em {Batches} lotes, {InputTokens} tokens, {ElapsedMs} ms",
            indexed, batches, tokens, (int)stopwatch.Elapsed.TotalMilliseconds);
        return new IndexReport(indexed, batches, tokens, stopwatch.Elapsed, null);
    }

    // O texto que vira vetor: o cabeçalho (título — secção) e o texto, para o trecho fazer sentido sozinho.
    public static string EmbeddingText(KnowledgeChunk chunk) => $"{chunk.Heading}\n\n{chunk.Text}";
}
