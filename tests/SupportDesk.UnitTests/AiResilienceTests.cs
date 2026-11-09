using Microsoft.AspNetCore.Http;
using Polly.CircuitBreaker;
using Polly.Timeout;
using SupportDesk.Web.Ai;

namespace SupportDesk.UnitTests;

// Exercício B da aula 21: que pedidos contam para o limite, e que mensagem dá cada falha nova.
public class AiResilienceTests
{
    private static HttpRequest Request(string method, string path, string query = "")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(query);
        return context.Request;
    }

    [Theory]
    [InlineData("POST", "/Ai/Lab", "", true)]
    [InlineData("GET", "/Ai/Lab", "", false)]
    [InlineData("POST", "/Tickets/Details/3", "?handler=Suggest", true)]
    [InlineData("POST", "/Tickets/Details/3", "?handler=Comment", false)]
    [InlineData("POST", "/Tickets/Reply/3", "?handler=Generate", true)]
    [InlineData("GET", "/Knowledge", "?q=vpn", true)]
    [InlineData("GET", "/Knowledge", "", false)]
    public void Only_requests_that_call_a_model_count(string method, string path, string query, bool expected)
    {
        Assert.Equal(expected, AiRequests.IsAiRequest(Request(method, path, query)));
    }

    [Fact]
    public void The_new_failures_are_expected_and_explained()
    {
        var options = new OllamaOptions { TimeoutSeconds = 5 };
        Exception[] failures = [new TimeoutRejectedException(), new BrokenCircuitException(), new AiDisabledException()];

        Assert.All(failures, ex => Assert.True(AiErrors.IsExpected(ex)));
        Assert.StartsWith("O Ollama não respondeu em 5 s.", AiErrors.Describe(failures[0], options));
        Assert.StartsWith("O Ollama falhou várias vezes seguidas", AiErrors.Describe(failures[1], options));
        Assert.Equal("A IA está desligada nesta instalação (AI:Ollama:Enabled = false).", AiErrors.Describe(failures[2], options));
    }

    [Fact]
    public void A_cancelled_request_is_still_not_expected()
    {
        Assert.False(AiErrors.IsExpected(new OperationCanceledException()));
    }
}
