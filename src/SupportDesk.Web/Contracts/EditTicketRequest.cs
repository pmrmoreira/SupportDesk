namespace SupportDesk.Web.Contracts;

// Aula 9: editar valida como criar, e traz a versão do pedido que o formulário mostrou.
// Se alguém gravar entre o GET e o POST, a versão já não bate e o serviço responde Conflict.
public class EditTicketRequest : CreateTicketRequest
{
    public uint Version { get; set; }
}
