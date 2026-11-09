using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Authorization;
using SupportDesk.Web.Data;
using SupportDesk.Web.Infrastructure;
using SupportDesk.Web.Services;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Identity;
using SupportDesk.Web.Models;
using Pgvector.EntityFrameworkCore;
using SupportDesk.Web.Ai;
using SupportDesk.Web.Knowledge;



var builder = WebApplication.CreateBuilder(args);

// Aula 10: todas as páginas da pasta /Tickets pedem sessão iniciada.
// Aula 14: /Admin (página de utilizadores da aula 11) e /Ai só para técnicos (política "Tecnico").
// Aula 17: /Knowledge (base de conhecimento) também só para técnicos.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Tickets");
    options.Conventions.AuthorizeFolder("/Admin", "Tecnico");
    options.Conventions.AuthorizeFolder("/Ai", "Tecnico");
    options.Conventions.AuthorizeFolder("/Knowledge", "Tecnico");
});

var connectionString = builder.Configuration.GetConnectionString("SupportDesk")
    ?? throw new InvalidOperationException(
        "Falta a ligação 'SupportDesk'. Define-a com dotnet user-secrets (aula 6).");

builder.Services.AddDbContext<SupportDeskDbContext>(options =>
{
    // UseVector: o Npgsql passa a conhecer o tipo vector do pgvector (aula 18).
    options.UseNpgsql(connectionString, npgsql => npgsql.UseVector())
        .UseSeeding((context, _) => SeedData.Seed(context))
        .UseAsyncSeeding((context, _, ct) => SeedData.SeedAsync(context, ct));

    // Exercício A da aula 6: valores dos parâmetros no SQL da consola, só em Development.
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddScoped<ITicketService, TicketService>();

// IA: o Ollama local, por trás de IChatClient e IEmbeddingGenerator (aulas 14 e 18).
// Aula 20: o registo passou para AddOllamaAi, que a ferramenta de avaliação também usa.
builder.Services.AddOllamaAi(builder.Configuration);
builder.Services.AddScoped<TicketClassifier>();
builder.Services.AddScoped<ISuggestionService, SuggestionService>();
// Aula 16: o resumo da conversa.
builder.Services.AddScoped<TicketSummarizer>();
builder.Services.AddScoped<ISummaryService, SummaryService>();

// Base de conhecimento: artigos .md importados para o PostgreSQL, com pesquisa textual (aula 17) e semântica (aula 18).
builder.Services.AddOptions<KnowledgeOptions>()
    .Bind(builder.Configuration.GetSection(KnowledgeOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddScoped<KnowledgeImporter>();
builder.Services.AddScoped<KnowledgeSearch>();
builder.Services.AddScoped<KnowledgeIndexer>();
// Aula 19: rascunhos de resposta com fontes (RAG).
builder.Services.AddScoped<ReplyDrafter>();
builder.Services.AddScoped<IDraftService, DraftService>();

// Aula 21: no máximo N pedidos à IA por utilizador e por minuto; os outros pedidos não contam.
var aiRequestsPerMinute = builder.Configuration.GetValue($"{OllamaOptions.Section}:RequestsPerMinute", 10);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        AiRequests.IsAiRequest(context.Request)
            ? RateLimitPartition.GetFixedWindowLimiter(AiRequests.UserKey(context), _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = aiRequestsPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            })
            : RateLimitPartition.GetNoLimiter("sem limite"));
});

// Aula 21: /health diz se a base de dados e o Ollama estão bem.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SupportDeskDbContext>("base de dados")
    .AddCheck<OllamaHealthCheck>("ollama");

// Exercício C da aula 21: os traços das chamadas à IA na consola, só quando Telemetry:Console = true.
if (builder.Configuration.GetValue("Telemetry:Console", false))
{
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing
            .AddSource(OllamaServiceCollectionExtensions.TelemetrySource)
            // O pedido HTTP já tem uma Activity «pai» que ninguém regista; o amostrador por omissão (ParentBased)
            // seguia essa decisão e deitava fora os traços da IA. AlwaysOn regista-os sempre.
            .SetSampler(new AlwaysOnSampler())
            .AddConsoleExporter());
}



builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<SupportDeskDbContext>()
    .AddApiEndpoints();

// Aula 10, ciclo 3: a API prova quem és com um token Bearer; as páginas, com o cookie.
builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Api", policy => policy
        .AddAuthenticationSchemes(IdentityConstants.BearerScheme)
        .RequireAuthenticatedUser())
     .AddPolicy("Tecnico", policy => policy.RequireRole(Roles.Technician));
// Aula 10: quem pode ler, comentar e mudar o estado de cada pedido. Sem estado nem DbContext: Singleton.
builder.Services.AddSingleton<IAuthorizationHandler, TicketAuthorizationHandler>();

// Exercício B da aula 22: as chaves que protegem os cookies de sessão (Data Protection) ficam numa pasta que
// sobrevive ao contentor: no docker compose, o volume keys em /keys. Sem esta configuração, como na tua máquina,
// nada muda: as chaves continuam no teu perfil do Windows.
var keysFolder = builder.Configuration["DataProtection:KeysFolder"];
if (!string.IsNullOrEmpty(keysFolder))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
}

// Aula 7, ciclo 3: erros inesperados em ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();

// Aula 22: num contentor, a base de dados começa vazia. Com Database:MigrateOnStartup = true, a aplicação
// aplica as migrações que faltam antes de aceitar pedidos; o MigrateAsync também corre o UseAsyncSeeding (aula 6).
if (app.Configuration.GetValue("Database:MigrateOnStartup", false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
    await db.Database.MigrateAsync();
}

// O técnico de demonstração: em Development, ou quando Seed:Enabled = true (o docker compose da aula 22).
if (app.Environment.IsDevelopment() || app.Configuration.GetValue("Seed:Enabled", false))
{
    await IdentitySeeder.SeedAsync(app.Services, app.Configuration, app.Logger);
}
// Corre sempre (também em Development): a API responde com ProblemDetails
// (ApiExceptionHandler) e as páginas mostram /Error.
app.UseExceptionHandler("/Error");

// Aula 8: nas páginas, um 404 (ou outro erro sem corpo) mostra a página /StatusCode em vez da do browser.
// A API fica de fora: responde com ProblemDetails ou sem corpo, nunca com HTML.
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseStatusCodePagesWithReExecute("/StatusCode"));

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
// Aula 21: depois da autenticação, porque o limite é por utilizador e é preciso saber quem é.
app.UseRateLimiter();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapGet("/api/health", () => new
{
    application = "SupportDesk Ai",
    status = "ok",
    version = "0.1.0"
});
app.MapGet("/api/info", () => new
{
    name = "SupportDesk AI",
    purpose = "Aplicação de AI para suporte no Desktop"
});
app.MapGet("/api/learning", () => new
{
    lesson = 1,
    topic = "Primeira aplicação web",
    completed = false
});
// Aula 10, ciclo 3: /api/auth/register, /api/auth/login, /api/auth/refresh, ...
app.MapGroup("/api/auth").MapIdentityApi<ApplicationUser>();
// Aula 21: o estado da base de dados e do Ollama (200 Healthy/Degraded, 503 Unhealthy).
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = HealthReportWriter.WriteAsync });
app.MapControllers();
app.Run();
