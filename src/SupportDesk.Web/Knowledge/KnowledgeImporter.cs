using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SupportDesk.Web.Data;
using SupportDesk.Web.Models;

namespace SupportDesk.Web.Knowledge;

public record ImportReport(int Added, int Updated, int Unchanged, int Removed);

// Lê os ficheiros .md da pasta dos artigos e acerta a base: artigos novos, alterados, iguais e removidos.
public class KnowledgeImporter
{
    private readonly SupportDeskDbContext _db;
    private readonly string _folder;
    private readonly ILogger<KnowledgeImporter> _logger;

    public KnowledgeImporter(SupportDeskDbContext db, IOptions<KnowledgeOptions> options,
        IWebHostEnvironment environment, ILogger<KnowledgeImporter> logger)
    {
        _db = db;
        _folder = Path.Combine(environment.ContentRootPath, options.Value.Folder);
        _logger = logger;
    }

    public string Folder => _folder;

    public async Task<ImportReport> ImportAsync(CancellationToken ct = default)
    {
        var existing = await _db.KnowledgeArticles
            .Include(a => a.Chunks)
            .ToDictionaryAsync(a => a.Slug, ct);
        int added = 0, updated = 0, unchanged = 0;

        var files = Directory.GetFiles(_folder, "*.md").Order().ToList();
        foreach (var path in files)
        {
            var slug = Path.GetFileNameWithoutExtension(path);
            var markdown = await File.ReadAllTextAsync(path, ct);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(markdown)));

            if (existing.TryGetValue(slug, out var article) && article.ContentHash == hash)
            {
                unchanged++;
                continue;
            }

            var parsed = MarkdownChunker.Parse(markdown, slug);
            if (article is null)
            {
                article = new KnowledgeArticle { Slug = slug };
                _db.KnowledgeArticles.Add(article);
                added++;
            }
            else
            {
                // Trechos antigos fora da lista: o EF Core apaga-os no SaveChanges.
                article.Chunks.Clear();
                updated++;
            }
            article.Title = parsed.Title;
            article.ContentHash = hash;
            article.ImportedAt = DateTimeOffset.UtcNow;
            article.Chunks.AddRange(parsed.Chunks.Select((chunk, position) => new KnowledgeChunk
            {
                Position = position,
                Heading = chunk.Heading,
                Text = chunk.Text
            }));
        }

        // Exercício C: um artigo cujo ficheiro desapareceu sai da base, e os trechos com ele (Cascade).
        // Uma pasta vazia é mais provavelmente um engano do que «apagar tudo»: nesse caso não se remove nada.
        var removed = 0;
        if (files.Count > 0)
        {
            var slugs = files.Select(Path.GetFileNameWithoutExtension).ToHashSet();
            foreach (var article in existing.Values.Where(a => !slugs.Contains(a.Slug)))
            {
                _db.KnowledgeArticles.Remove(article);
                removed++;
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Base de conhecimento: {Added} novos, {Updated} atualizados, {Unchanged} sem alterações, {Removed} removidos",
            added, updated, unchanged, removed);
        return new ImportReport(added, updated, unchanged, removed);
    }
}
