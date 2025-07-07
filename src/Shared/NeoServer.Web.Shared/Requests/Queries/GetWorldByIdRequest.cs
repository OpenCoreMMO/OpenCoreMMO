using MediatR;
using NeoServer.Web.API.Response.World;

namespace NeoServer.Web.API.Requests.Queries;

public class GetWorldByIdRequest : IRequest<WorldResponseViewModel>
{
    public int Id { get; set; }
}