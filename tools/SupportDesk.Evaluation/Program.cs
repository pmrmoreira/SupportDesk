using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pgvector.EntityFrameworkCore;
using SupportDesk.Evaluation;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Data;
using SupportDesk.Web.Knowledge;

// Avaliação da IA. Corre na raiz do repositório:
//   dotnet run --project tools/SupportDesk.Evaluation                  → as três avaliações
//   dotnet run --project tools/SupportDesk.Evaluation -- retrieval     → só uma (classification, retrieval, rag)
var builder = Host.CreateApplicationBuilder(args);

// A configuração da aplicação: o appsettings.json dela e os user-secrets dela (a ligação ao PostgreSQL).
builder.Configuration
    .AddJsonFile(Path.Combine("src", "SupportDesk.Web", "appsettings.json"), optional: false)
    .AddUserSecrets(typeof(SupportDeskDbContext).Assembly, optional: true)
    // A consola é para o relatório: só avisos e erros (o appsettings.json da aplicação pede Information).
    .AddInMemoryCollection(new Dictionary<string, string?> { ["Logging:LogLevel:Default"] = "Warning" })
    // Por último: o que se escreve na linha de comandos ganha (ex.: --AI:Ollama:ChatModel=…).
    .AddCommandLine(args);

var connectionString = builder.Configuration.GetConnectionString("SupportDesk")
    ?? throw new InvalidOperationException("Falta a ligação 'SupportDesk' nos user-secrets da aplicação (aula 6).");
builder.Services.AddDbContext<SupportDeskDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));
builder.Services.AddOllamaAi(builder.Configuration);
builder.Services.AddScoped<TicketClassifier>();
builder.Services.AddScoped<KnowledgeSearch>();
builder.Services.AddScoped<ReplyDrafter>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;
var ollama = services.GetRequiredService<IOptions<OllamaOptions>>().Value;

// O primeiro argumento que não começa por -- escolhe a avaliação; sem nenhum, correm todas.
var suite = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "all";
var report = new StringBuilder()
    .AppendLine("# Avaliação da IA — SupportDesk")
    .AppendLine()
    .AppendLine($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm} · chat: {ollama.ChatModel} · embeddings: {ollama.EmbeddingModel}")
    .AppendLine();

if (suite is "all" or "classification")
{
    report.Append(await ClassificationSuite.RunAsync(services, EvalCases.Load<ClassificationCase>("eval/classification.json"), default));
}
if (suite is "all" or "retrieval")
{
    report.Append(await RetrievalSuite.RunAsync(services, EvalCases.Load<RetrievalCase>("eval/retrieval.json"), default));
}
if (suite is "all" or "rag")
{
    report.Append(await RagSuite.RunAsync(services, EvalCases.Load<RagCase>("eval/rag.json"), default));
}

// Exercício A: um relatório por par de modelos, para os comparar (ex.: eval/report-gemma3-4b-bge-m3.md).
var name = $"report-{ollama.ChatModel}-{ollama.EmbeddingModel}".Replace(':', '-').Replace('/', '-');
var path = Path.Combine("eval", $"{name}.md");
await File.WriteAllTextAsync(path, report.ToString());
Console.WriteLine(report);
Console.WriteLine($"Relatório gravado em {path}");
