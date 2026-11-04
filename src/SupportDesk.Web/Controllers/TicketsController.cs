using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Web.Authorization;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Infrastructure;
using SupportDesk.Web.Services;

namespace SupportDesk.Web.Controllers;

[ApiController]
[Route("api/tickets")]
// Aula 10, ciclo 3: toda a API pede um token Bearer (o cookie das páginas não conta).
[Authorize(Policy = "Api")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;
    private readonly IAuthorizationService _authorization;

    public TicketsController(ITicketService tickets, IAuthorizationService authorization)
    {
        _tickets = tickets;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TicketListItem>>> Search([FromQuery] TicketQuery query, CancellationToken ct)
    {
        return Ok(await _tickets.SearchAsync(query, User.OwnerFilter(), ct));
    }

    // Aula 10: o cliente conta só os seus pedidos; o técnico conta todos.
    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken ct)
    {
        return Ok(new { total = await _tickets.CountAsync(User.OwnerFilter(), ct) });
    }

    // Exercício B da aula 6.
    [HttpGet("stats")]
    public async Task<IActionResult> Stats(CancellationToken ct)
    {
        return Ok(await _tickets.CountByStatusAsync(User.OwnerFilter(), ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDetails>> GetById(int id, CancellationToken ct)
    {
        var ticket = await FindReadableAsync(id, ct);
        if (ticket is null)
        {
            return NotFound();
        }
        return Ok(ticket);
    }

    [HttpGet("{id:int}/title")]
    public async Task<IActionResult> GetTitleById(int id, CancellationToken ct)
    {
        var ticket = await FindReadableAsync(id, ct);
        if (ticket is null)
        {
            return NotFound();
        }
        return Ok(new { id = ticket.Id, title = ticket.Title });
    }

    [HttpPost]
    public async Task<ActionResult<TicketDetails>> Create([FromBody] CreateTicketRequest request, CancellationToken ct)
    {
        var ticket = await _tickets.CreateAsync(
            request.Title.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.Priority,
            User.GetUserId(),
            ct);

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        var ticket = await FindReadableAsync(id, ct);
        if (ticket is null)
        {
            return NotFound();
        }
        if (!await CanAsync(ticket, TicketOperations.ChangeStatus))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var newStatus = request.Status!.Value;
        var result = await _tickets.ChangeStatusAsync(id, newStatus, ct);

        return result switch
        {
            StatusChangeResult.Changed => NoContent(),
            StatusChangeResult.NotFound => NotFound(),
            StatusChangeResult.Conflict => ConcurrencyConflict(),
            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Mudança de estado não permitida",
                detail: $"Este pedido não pode passar para {newStatus}.")
        };
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentResponse>> AddComment(int id, [FromBody] AddCommentRequest request, CancellationToken ct)
    {
        var ticket = await FindReadableAsync(id, ct);
        if (ticket is null)
        {
            return NotFound();
        }
        if (!await CanAsync(ticket, TicketOperations.Comment))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var (result, comment) = await _tickets.AddCommentAsync(id, User.GetUserId(), request.Text.Trim(), ct);

        return result switch
        {
            AddCommentResult.Added => CreatedAtAction(nameof(GetComments), new { id }, comment),
            AddCommentResult.TicketNotFound => NotFound(),
            AddCommentResult.Conflict => ConcurrencyConflict(),
            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Pedido resolvido",
                detail: "Não é possível comentar um pedido resolvido. Reabre-o primeiro.")
        };
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<CommentResponse[]>> GetComments(int id, CancellationToken ct)
    {
        var ticket = await FindReadableAsync(id, ct);
        if (ticket is null)
        {
            return NotFound();
        }
        return Ok(await _tickets.GetCommentsAsync(id, ct));
    }

    // Aula 7, ciclo 3: o pedido é válido, mas choca com o estado atual (alguém gravou primeiro) → 409.
    private ObjectResult ConcurrencyConflict() => Problem(
        statusCode: StatusCodes.Status409Conflict,
        title: "O pedido mudou entretanto",
        detail: "Outra pessoa alterou este pedido ao mesmo tempo. Volta a carregá-lo e tenta outra vez.");

    // Um pedido que não existe e um pedido de outro cliente dão a mesma resposta: 404.
    private async Task<TicketDetails?> FindReadableAsync(int id, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct);
        if (ticket is null || !await CanAsync(ticket, TicketOperations.Read))
        {
            return null;
        }
        return ticket;
    }

    private async Task<bool> CanAsync(TicketDetails ticket, OperationAuthorizationRequirement operation) =>
        (await _authorization.AuthorizeAsync(User, ticket, operation)).Succeeded;
}
