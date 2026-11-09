using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SupportDesk.Web.Ai;

namespace SupportDesk.Web.Pages.Ai;

// Só técnicos (convenção "/Ai" em Program.cs). Uma pergunta, uma resposta do modelo local.
public class LabModel : PageModel
{
    private const string SystemPrompt =
        "És o assistente da equipa de suporte da SupportDesk. Responde em português de Portugal, " +
        "em poucas frases e de forma concreta. Se não souberes, diz que não sabes.";

    private readonly IChatClient _chat;
    private readonly OllamaOptions _options;
    private readonly ILogger<LabModel> _logger;

    public LabModel(IChatClient chat, IOptions<OllamaOptions> options, ILogger<LabModel> logger)
    {
        _chat = chat;
        _options = options.Value;
        _logger = logger;
    }

    [BindProperty]
    public LabInput Input { get; set; } = new();

    public LabAnswer? Answer { get; private set; }
    public string? Error { get; private set; }
    public string ChatModel => _options.ChatModel;
    public string OllamaUrl => _options.Url;

    public class LabInput
    {
        [Required(ErrorMessage = "Escreve uma pergunta.")]
        [StringLength(2000, ErrorMessage = "A pergunta pode ter no máximo 2000 caracteres.")]
        public string Prompt { get; set; } = string.Empty;
    }

    public record LabAnswer(string Text, string? Model, long? InputTokens, long? OutputTokens, TimeSpan Elapsed);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        List<ChatMessage> messages =
        [
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, Input.Prompt.Trim())
        ];

        var stopwatch = Stopwatch.StartNew();
        // Aula 14, ciclo 3: as falhas esperadas (Ollama desligado, modelo em falta, tempo esgotado)
        // viram um aviso na página; as outras não passam no filtro do catch e seguem para /Error.
        try
        {
            var response = await _chat.GetResponseAsync(messages, cancellationToken: ct);
            Answer = new LabAnswer(response.Text, response.ModelId,
                response.Usage?.InputTokenCount, response.Usage?.OutputTokenCount, stopwatch.Elapsed);
            _logger.LogInformation("Laboratório de IA: {Model}, {InputTokens} + {OutputTokens} tokens, {ElapsedMs} ms",
                response.ModelId, Answer.InputTokens, Answer.OutputTokens, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex) when (AiErrors.IsExpected(ex))
        {
            _logger.LogWarning(ex, "Laboratório de IA: a chamada ao Ollama falhou ao fim de {ElapsedMs} ms",
                stopwatch.ElapsedMilliseconds);
            Error = AiErrors.Describe(ex, _options);
        }

        return Page();
    }
}
