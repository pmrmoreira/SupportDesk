using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Authorization;
using SupportDesk.Web.Data;
using SupportDesk.Web.Infrastructure;
using SupportDesk.Web.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using SupportDesk.Web.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;
using SupportDesk.Web.Ai;



var builder = WebApplication.CreateBuilder(args);

// Aula 10: todas as páginas da pasta /Tickets pedem sessão iniciada.
// Aula 14: /Admin (página de utilizadores da aula 11) e /Ai só para técnicos (política "Tecnico").
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Tickets");
    options.Conventions.AuthorizeFolder("/Admin", "Tecnico");
    options.Conventions.AuthorizeFolder("/Ai", "Tecnico");
});

var connectionString = builder.Configuration.GetConnectionString("SupportDesk")
    ?? throw new InvalidOperationException(
        "Falta a ligação 'SupportDesk'. Define-a com dotnet user-secrets (aula 6).");

builder.Services.AddDbContext<SupportDeskDbContext>(options =>
{
    options.UseNpgsql(connectionString)
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

// IA: o Ollama local, por trás da abstração IChatClient (Microsoft.Extensions.AI).
builder.Services.AddOptions<OllamaOptions>()
    .Bind(builder.Configuration.GetSection(OllamaOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHttpClient("Ollama", (services, http) =>
{
    var options = services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    http.BaseAddress = new Uri(options.Url);
    http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
builder.Services.AddChatClient(services =>
{
    var options = services.GetRequiredService<IOptions<OllamaOptions>>().Value;
    var http = services.GetRequiredService<IHttpClientFactory>().CreateClient("Ollama");
    return new OllamaApiClient(http, options.ChatModel);
}, ServiceLifetime.Scoped)
    .UseLogging();
builder.Services.AddScoped<TicketClassifier>();
builder.Services.AddScoped<ISuggestionService, SuggestionService>();


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

// Aula 7, ciclo 3: erros inesperados em ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
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
app.MapControllers();
app.Run();
