using SupportDesk.Web.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportDesk.UnitTests;

public class CreateTicketRequestTests
{
    [Fact]
    public void A_valid_request_has_no_errors()
    {
        var request = new CreateTicketRequest { Title = "Impressora sem ligação", Priority = 1 };

        var errors = ModelValidation.Validate(request);

        Assert.Empty(errors);
    }
    [Fact]
    public void Spaces_around_the_title_do_not_count()
    {
        var request = new CreateTicketRequest { Title = "   ab   " };

        var errors = ModelValidation.Validate(request);

        var message = Assert.Single(errors.ErrorsFor(nameof(CreateTicketRequest.Title)));
        Assert.Equal("O título deve ter pelo menos 5 caracteres, sem contar os espaços nas pontas.", message);
    }
    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public void Short_or_empty_titles_are_rejected(string title)
    {
        var request = new CreateTicketRequest { Title = title };

        var errors = ModelValidation.Validate(request);

        Assert.NotEmpty(errors.ErrorsFor(nameof(CreateTicketRequest.Title)));
    }

    [Fact]
    public void A_title_longer_than_120_characters_is_rejected()
    {
        var request = new CreateTicketRequest { Title = new string('x', 121) };

        var errors = ModelValidation.Validate(request);

        Assert.Equal(["O título deve ter entre 5 e 120 caracteres."], errors.ErrorsFor(nameof(CreateTicketRequest.Title)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Priority_must_be_between_1_and_3(int priority)
    {
        var request = new CreateTicketRequest { Title = "Impressora sem ligação", Priority = priority };

        var errors = ModelValidation.Validate(request);

        Assert.Equal(["A prioridade tem de ser 1, 2 ou 3."], errors.ErrorsFor(nameof(CreateTicketRequest.Priority)));
    }
}
