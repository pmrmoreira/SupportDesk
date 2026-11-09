using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Data;
using SupportDesk.Web.Knowledge;
using SupportDesk.Web.Services;

namespace SupportDesk.Web.Pages.Knowledge;

// Só técnicos (convenção "/Knowledge" em Program.cs): importar os artigos, gerar os embeddings e pesquisar nos trechos.
public class IndexModel : PageModel
{
    private readonly SupportDeskDbContext _db;
    private readonly KnowledgeImporter _importer;
    private readonly KnowledgeSearch _search;
    private readonly KnowledgeIndexer _indexer;
    private readonly OllamaOptions _ollama;
    private readonly IDraftService _drafts;

    public IndexModel(SupportDeskDbContext db, KnowledgeImporter importer, KnowledgeSearch search,
        KnowledgeIndexer indexer, IOptions<OllamaOptions> ollama, IDraftService drafts)
    {
        _db = db;
        _importer = importer;
        _search = search;
        _indexer = indexer;
        _ollama = ollama.Value;
        _drafts = drafts;
    }

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public int ArticleCount { get; private set; }
    public int ChunkCount { get; private set; }
    public int EmbeddedCount { get; private set; }
    public string EmbeddingModel => _ollama.EmbeddingModel;
    public IReadOnlyList<KnowledgeHit> TextHits { get; private set; } = [];
    public IReadOnlyList<KnowledgeHit> SemanticHits { get; private set; } = [];
    public IReadOnlyList<KnowledgeHit> HybridHits { get; private set; } = [];
    public string? SemanticError { get; private set; }
    // Exercício C da aula 19: quanto os técnicos usam os rascunhos da IA.
    public DraftStats DraftStats { get; private set; } = new(0, 0, 0, 0, 0);

    public async Task OnGetAsync(CancellationToken ct)
    {
        ArticleCount = await _db.KnowledgeArticles.CountAsync(ct);
        ChunkCount = await _db.KnowledgeChunks.CountAsync(ct);
        // Exercício C da aula 18: contam os vetores do modelo configurado.
        EmbeddedCount = await _db.KnowledgeChunks
            .CountAsync(c => c.Embedding != null && c.EmbeddingModel == _ollama.EmbeddingModel, ct);
        DraftStats = await _drafts.GetStatsAsync(ct);
        if (string.IsNullOrWhiteSpace(Q))
        {
            return;
        }
        var query = Q.Trim();
        // Exercício A da aula 18: 10 candidatos de cada pesquisa; cada coluna mostra 5, e a híbrida funde os 10 + 10.
        // A textual fica fora do try: não depende do Ollama.
        var text = await _search.SearchTextAsync(query, take: 10, ct);
        TextHits = text.Take(5).ToList();
        try
        {
            var semantic = await _search.SearchSemanticAsync(query, take: 10, ct);
            SemanticHits = semantic.Take(5).ToList();
            HybridHits = RankFusion.Combine(5, text, semantic);
        }
        catch (Exception ex) when (AiErrors.IsExpected(ex))
        {
            // Sem Ollama, a pesquisa textual continua a funcionar.
            SemanticError = AiErrors.Describe(ex, _ollama, _ollama.EmbeddingModel);
        }
        catch (EmbeddingDimensionsException ex)
        {
            SemanticError = ex.Message;
        }
    }

    // Botão "Importar artigos": lê a pasta Articles e acerta a base.
    public async Task<IActionResult> OnPostImportAsync(CancellationToken ct)
    {
        try
        {
            var report = await _importer.ImportAsync(ct);
            TempData["Message"] =
                $"Importação: {report.Added} novos, {report.Updated} atualizados, {report.Unchanged} sem alterações, " +
                $"{report.Removed} removidos.";
        }
        catch (DirectoryNotFoundException)
        {
            TempData["Error"] = $"A pasta dos artigos não existe: {_importer.Folder}";
        }
        return RedirectToPage();
    }

    // Botão "Gerar embeddings" (aula 18): os trechos sem vetor ganham um, em lotes.
    public async Task<IActionResult> OnPostEmbedAsync(CancellationToken ct)
    {
        var report = await _indexer.IndexAsync(ct);
        if (report.Error is not null)
        {
            TempData["Error"] = $"{report.Error} ({report.Indexed} trechos ficaram com embedding.)";
        }
        else if (report.Indexed == 0)
        {
            TempData["Message"] = "Todos os trechos já têm embedding.";
        }
        else
        {
            TempData["Message"] = $"Embeddings: {report.Indexed} trechos em {report.Batches} lotes, " +
                $"{report.InputTokens} tokens, {(int)report.Elapsed.TotalMilliseconds} ms.";
        }
        return RedirectToPage();
    }
}
