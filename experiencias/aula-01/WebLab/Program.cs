var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/ola", () => "Hello World!");

app.MapGet("/eco/{numero:int}", (int numero) => new
{
    valor = numero,
    dobro = numero * 2
});
app.Run();
