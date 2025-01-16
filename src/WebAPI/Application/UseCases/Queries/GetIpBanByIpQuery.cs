using AutoMapper;
using MediatR;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response.IpBans;

namespace NeoServer.Web.API.Application.UseCases.Queries;

public class GetIpBanByIpQuery(IMapper mapper, IIpBansRepository ipBansRepository) : IRequestHandler<GetIpBanByIpRequest, IpBanResponseViewModel>
{
    public async Task<IpBanResponseViewModel> Handle(GetIpBanByIpRequest request, CancellationToken cancellationToken)
    {
        var ipban = await ipBansRepository.GetBan(request.ip);
        var response = mapper.Map<IpBanResponseViewModel>(ipban);
        return response;
    }
}