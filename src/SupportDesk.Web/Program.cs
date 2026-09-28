var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

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
