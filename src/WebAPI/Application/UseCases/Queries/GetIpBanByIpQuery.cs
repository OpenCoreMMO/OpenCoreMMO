using MediatR;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response.IpBans;

namespace NeoServer.Web.API.Application.UseCases.Queries;

public class GetIpBanByIpQuery(IIpBansRepository ipBansRepository)
    : IRequestHandler<GetIpBanByIpRequest, IpBanResponseViewModel>
{
    public async Task<IpBanResponseViewModel> Handle(GetIpBanByIpRequest request, CancellationToken cancellationToken)
    {
        return await ipBansRepository.ExistBan(request.ip);
    }
}