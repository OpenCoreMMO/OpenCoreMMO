using MediatR;
using NeoServer.Web.API.Response;

namespace NeoServer.Web.API.Requests.Commands;

public class DeleteWorldCommandRequest : IRequest<OutputResponse>, ICommandBase
{
    public int Id { get; set; }
}