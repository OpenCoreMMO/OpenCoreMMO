using MediatR;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Player;

namespace NeoServer.Web.API.Requests.Queries;

public class GetPlayersRequest : IRequest<BasePagedResponseViewModel<IEnumerable<PlayerResponseViewModel>>>
{
    public int? AccountId { get; set; }
    public string Name { get; set; }
    public byte? Group { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 5;
    public bool? Online { get; set; }
    public byte? Vocation { get; set; }
    public byte? Gender { get; set; }
}