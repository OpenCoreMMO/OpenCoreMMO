using MediatR;
using NeoServer.Web.API.Response.IpBans;

namespace NeoServer.Web.API.Requests.Queries;

public record GetIpBanByIpRequest(string ip) : IRequest<IpBanResponseViewModel>;
