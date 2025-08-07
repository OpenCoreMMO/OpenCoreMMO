using MediatR;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response.Player;

namespace NeoServer.Web.API.Application.UseCases.Queries;

public class GetPlayerByIdQuery(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerByIdRequest, PlayerResponseViewModel>
{
    public async Task<PlayerResponseViewModel> Handle(GetPlayerByIdRequest request, CancellationToken cancellationToken)
    {
        return await playerRepository.GetAsync(request.Id);
    }
}