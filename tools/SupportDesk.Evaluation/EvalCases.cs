using System.Text.Json;
using SupportDesk.Web.Models;

namespace SupportDesk.Evaluation;

// Os casos de teste, nos ficheiros eval/*.json (escritos à mão: o que esperamos que a IA faça).
public record ClassificationCase(string Title, string Description, TicketCategory Category, int Priority);

public record RetrievalCase(string Query, string Article);

public record RagCase(string Title, string Description, string Article);

public static class EvalCases
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static IReadOnlyList<T> Load<T>(string path) =>
        JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), Json)
        ?? throw new InvalidOperationException($"O ficheiro {path} está vazio.");
}
