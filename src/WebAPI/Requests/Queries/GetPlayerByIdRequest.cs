using MediatR;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Player;

namespace NeoServer.Web.API.Requests.Queries;

public class GetPlayerByIdRequest : IRequest<PlayerResponseViewModel>
{
    public int Id { get; set; }
}