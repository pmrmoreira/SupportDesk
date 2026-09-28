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
        new Ticket { Id = 2, Title = "Fatura com valor incorreto", Status = "InProgress" }
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
}
