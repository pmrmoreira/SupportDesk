using Microsoft.EntityFrameworkCore;
using SupportDesk.Web.Data;
using SupportDesk.Web.Infrastructure;
using SupportDesk.Web.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

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

// Aula 7, ciclo 3: erros inesperados em ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();

// Corre sempre (também em Development): a API responde com ProblemDetails
// (ApiExceptionHandler) e as páginas mostram /Error.
app.UseExceptionHandler("/Error");

app.UseRouting();

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
app.MapControllers();
app.Run();
