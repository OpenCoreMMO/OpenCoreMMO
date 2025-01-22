using MediatR;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Application.UseCases.Commands;

public class BanIpCommand(IIpBansRepository ipBansRepository) : IRequestHandler<BanIpRequest, OutputResponse>
{
    public async Task<OutputResponse> Handle(BanIpRequest request, CancellationToken cancellationToken)
    {
        var ipBan = await ipBansRepository.ExistBan(request.Ip);

        if (ipBan is not null)
            return new OutputResponse(ErrorMessage.IpBanished);

        var entity = new IpBanEntity()
        {
            Ip = request.Ip,
            BannedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(request.Days),
            Reason = request.Reason,
            BannedBy = 1 //  TODO: Get the user id from the request with the token
        };

        await ipBansRepository.Insert(entity);
        return new();
    }
}