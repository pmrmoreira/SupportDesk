using Microsoft.AspNetCore.Mvc;
using SupportDesk.Web.Models;
using SupportDesk.Web.Contracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SupportDesk.Web.Services;
using System.Reflection.Metadata.Ecma335;
using static SupportDesk.Web.Services.TicketService;

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
    public ActionResult<Ticket[]> GetAll([FromQuery] TicketStatus? status)
    {
       return Ok(_tickets.GetAll(status));
    }

    [HttpGet("{id:int}")]
    public ActionResult<Ticket> GetById(int id)
    {
       
        
        var ticket = _tickets.GetById(id);
        if (ticket is null)
        {
            return NotFound();

        }
        return Ok(ticket);
        
    }
    [HttpGet("{id:int}/title")]
    public IActionResult GetTitleById(int id)
    {
        
        var ticket = _tickets.GetById(id);
        if (ticket == null)
        {
            return NotFound();

        }
        return Ok(new
        {
            id = ticket.Id,
            title = ticket.Title
        });
        
    }
    [HttpGet("count")]
    public IActionResult CountTickets()
    {
        return Ok(_tickets.Count());
    }
    
    [HttpPost]
    public ActionResult<Ticket> Create([FromBody] CreateTicketRequest request)
    {
        var title = request.Title.Trim();
        if (title.Length < 5)
        {
            ModelState.AddModelError(nameof(request.Title),
                "O título deve ter pelo menos 5 caracteres, sem contar os espaços nas pontas.");
            return ValidationProblem(ModelState);
        }

        var ticket = _tickets.Create(title, request.Description?.Trim() ?? string.Empty);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }
    [HttpPut("{id:int}/status")]
    public IActionResult ChangeStatus(int id, [FromBody] ChangeStatusRequest request) 
    {
        var newStatus = request.Status!.Value;
        return _tickets.ChangeStatus(id, newStatus) switch
        {
            StatusChangeResult.Changed => NoContent(),
            StatusChangeResult.NotFound => NotFound(),
            _ => Problem(statusCode: StatusCodes.Status409Conflict,
                         title: "Mudança de estado não permitida",
                         detail: $"Este pedido não pode passar para {newStatus}.")
        };
    }
    [HttpPost("{id:int}/comments")]
    public ActionResult<Comment> AddComment(int id, [FromBody] AddCommentRequest request)
    {
        var (result, comment) = _tickets.AddComment(id, request.Author.Trim(), request.Text.Trim());
        return result switch
        {
            AddCommentResult.Added => CreatedAtAction(nameof(GetComments), new { id }, comment),
            AddCommentResult.TicketNotFound => NotFound(),
            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Pedido Resolvido",
                detail: "Não é possivel comentar um pedido resolvido. Reabre-o primeiro.")
        };
    }
    [HttpGet("{id:int}/comments")]
    public ActionResult<Comment[]> GetComments(int id)
    {
        if(_tickets.GetById(id) is null)
        {
            return NotFound();
        }
        return Ok(_tickets.GetComments(id));
    }

}
