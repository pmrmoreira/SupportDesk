using Microsoft.AspNetCore.Mvc;
using SupportDesk.Web.Models;
using SupportDesk.Web.Contracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SupportDesk.Web.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private static readonly List<Ticket> Tickets =
    [
        new Ticket { Id = 1, Title = "Não consigo entrar", Status = "Open" },
        new Ticket { Id = 2, Title = "Fatura com valor incorreto", Status = "InProgress" },
        new Ticket { Id = 3, Title = "Impressora indisponível", Status = "Open" }
    ];

    private static readonly Lock TicketsLock = new();
    private static int NextId = 4;

    [HttpGet]
    public ActionResult<Ticket[]> GetAll()
    {
        lock (TicketsLock)
        {
            return Ok(Tickets.ToArray());
        }
    }

    [HttpGet("{id:int}")]
    public ActionResult<Ticket> GetById(int id)
    {
        lock (TicketsLock)
        {
            var ticket = Tickets.FirstOrDefault(ticket => ticket.Id == id);
            if (ticket is null)
            {
                return NotFound();

            }
            return Ok(ticket);
        }
    }
    [HttpGet("{id:int}/title")]
    public IActionResult GetTitleById(int id)
    {
        lock (TicketsLock)
        { 
            var ticket = Tickets.FirstOrDefault(ticket => ticket.Id == id);
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
    }
    [HttpGet("count")]
    public IActionResult CountTickets()
    {
        lock (TicketsLock)
        {
            return Ok(new { total = Tickets.Count });
        }
    }
    [HttpPost]
    public ActionResult<Ticket> Create([FromBody] CreateTicketRequest request)
    {
        var title = request.Title.Trim();
        if (title.Length < 5)
        {
            ModelState.AddModelError(nameof(request.Title), "O título dever ter pelo menos 5 caracteres.");
            return ValidationProblem(ModelState);
        }
        
        Ticket ticket;
        lock (TicketsLock)
        {
            ticket = new Ticket
            {
                Id = NextId++,
                Title = title,
                Description = request.Description?.Trim()
                ?? string.Empty,
                Status = "Open"
            };
            Tickets.Add(ticket);
        }
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }
}
