using MediatR;
using NeoServer.Web.API.Response;

namespace NeoServer.Web.API.Requests.Commands;

public class CreateAccountRequest : IRequest<OutputResponse>, ICommandBase
{
    
    public string Password { get; set; }
    public string Email { get; set; }
    public string AccountName { get; set; }
}