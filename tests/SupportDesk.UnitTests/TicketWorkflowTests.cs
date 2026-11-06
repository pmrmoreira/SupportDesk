using SupportDesk.Web.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportDesk.UnitTests;

public class TicketWorkflowTests
{
    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress, true)]
    [InlineData(TicketStatus.Open, TicketStatus.Resolved, false)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved, true)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Open, true)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Open, true)]
    [InlineData(TicketStatus.Resolved, TicketStatus.InProgress, false)]
    [InlineData(TicketStatus.Open, TicketStatus.Open, false)]
    [InlineData(TicketStatus.InProgress, TicketStatus.InProgress, false)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Resolved, false)]
    
    public void CanChange_follows_the_transition_table(TicketStatus from, TicketStatus to, bool expected)
    {
        var allowed = TicketWorkflow.CanChange(from, to);

        Assert.Equal(expected, allowed);
    }
    [Fact]
    public void NextStatuses_of_an_open_ticket_is_only_in_progress()
    {
        var next = TicketWorkflow.NextStatuses(TicketStatus.Open);

        Assert.Equal([TicketStatus.InProgress], next);
    }
    [Fact]
    public void A_resolved_ticket_can_only_be_reopened()
    {
        var next = TicketWorkflow.NextStatuses(TicketStatus.Resolved);

        Assert.Equal([TicketStatus.Open], next);
    }
}
