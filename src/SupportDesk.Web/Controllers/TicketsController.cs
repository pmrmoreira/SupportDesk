using Microsoft.AspNetCore.Mvc;
using SupportDesk.Web.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SupportDesk.Web.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private static readonly Ticket[] Tickets =
    {
        new Ticket { Id = 1, Title = "Não consigo entrar", Status = "Open" },
        new Ticket { Id = 2, Title = "Fatura com valor incorreto", Status = "InProgress" },
        new Ticket { Id = 3, Title = "Impressora indisponível", Status = "Open" }
    };
    [HttpGet]
    public ActionResult<Ticket[]> GetAll()
    {
        return Ok(Tickets);
    }
    [HttpGet("{id:int}")]
    public ActionResult<Ticket[]> GetById(int id)
    {
        var ticket = Tickets.FirstOrDefault(ticket => ticket.Id == id);
        if (ticket == null)
        {
            return NotFound();

        }
        return Ok(ticket);
    }
    [HttpGet("{id:int}/title")]
    public IActionResult GetTitleById(int id)
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
    [HttpGet("count")]
    public IActionResult CountTickets()
    {
        return Ok(new { total = Tickets.Length });
    }
}
