using Microsoft.AspNetCore.Mvc;
using SupportDesk.Web.Contracts;
using SupportDesk.Web.Models;
using SupportDesk.Web.Services;

namespace SupportDesk.Web.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;

    public TicketsController(ITicketService tickets)
    {
        _tickets = tickets;
    }

    [HttpGet]
    public async Task<ActionResult<Ticket[]>> GetAll(
        [FromQuery] TicketStatus? status,
        CancellationToken ct)
    {
        return Ok(await _tickets.GetAllAsync(status, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ticket>> GetById(
        int id,
        CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    [HttpGet("{id:int}/title")]
    public async Task<IActionResult> GetTitleById(
        int id,
        CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = ticket.Id,
            title = ticket.Title
        });
    }

    [HttpPost]
    public async Task<ActionResult<Ticket>> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken ct)
    {
        var title = request.Title.Trim();

        if (title.Length < 5)
        {
            ModelState.AddModelError(
                nameof(request.Title),
                "O título deve ter pelo menos 5 caracteres, sem contar os espaços nas pontas.");

            return ValidationProblem(ModelState);
        }

        var ticket = await _tickets.CreateAsync(
            title,
            request.Description?.Trim() ?? string.Empty,
            ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = ticket.Id },
            ticket);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] ChangeStatusRequest request,
        CancellationToken ct)
    {
        var newStatus = request.Status!.Value;

        var result = await _tickets.ChangeStatusAsync(
            id,
            newStatus,
            ct);

        return result switch
        {
            StatusChangeResult.Changed => NoContent(),

            StatusChangeResult.NotFound => NotFound(),

            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Mudança de estado não permitida",
                detail: $"Este pedido não pode passar para {newStatus}.")
        };
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<Comment>> AddComment(
        int id,
        [FromBody] AddCommentRequest request,
        CancellationToken ct)
    {
        var (result, comment) = await _tickets.AddCommentAsync(
            id,
            request.Author.Trim(),
            request.Text.Trim(),
            ct);

        return result switch
        {
            AddCommentResult.Added =>
                CreatedAtAction(
                    nameof(GetComments),
                    new { id },
                    comment),

            AddCommentResult.TicketNotFound =>
                NotFound(),

            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Pedido Resolvido",
                detail: "Não é possível comentar um pedido resolvido. Reabre-o primeiro.")
        };
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<Comment[]>> GetComments(
        int id,
        CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(id, ct);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(await _tickets.GetCommentsAsync(id, ct));
    }
}