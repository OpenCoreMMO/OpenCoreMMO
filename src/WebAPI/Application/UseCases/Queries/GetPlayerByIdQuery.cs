using AutoMapper;
using MediatR;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Player;

namespace NeoServer.Web.API.Application.UseCases.Queries;

public class GetPlayerByIdQuery(IMapper mapper, IPlayerRepository playerRepository) : IRequestHandler<GetPlayerByIdRequest, PlayerResponseViewModel>
{
    public async Task<PlayerResponseViewModel> Handle(GetPlayerByIdRequest request, CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetAsync(request.Id);
        var response = mapper.Map<PlayerResponseViewModel>(player);
        return response;
    }
}