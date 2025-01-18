using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Player;

namespace NeoServer.Web.API.Application.UseCases.Queries;

public class GetPlayersQuery(IMapper mapper, IPlayerRepository playerRepository) : IRequestHandler<GetPlayersRequest, BasePagedResponseViewModel<IEnumerable<PlayerResponseViewModel>>>
{
    public async Task<BasePagedResponseViewModel<IEnumerable<PlayerResponseViewModel>>> Handle(GetPlayersRequest request, CancellationToken cancellationToken)
    {
        Expression<Func<PlayerEntity, bool>> expression = item =>
            (request.AccountId == null || item.AccountId == request.AccountId) &&
            (request.Name == null || item.Name.ToLower().Contains(request.Name.ToLower())) &&
            (request.Group == null || item.Group == request.Group) &&
            (request.Online == null || item.Online == request.Online) &&
            (request.Vocation == null || item.Vocation == request.Vocation) && 
            (request.Gender == null || (byte)item.Gender == request.Gender);

        var totalPlayers = await playerRepository.CountAllAsync(expression);
        var players = await playerRepository.GetPaginatedPlayersAsync(expression, request.Page, request.Limit);
        var response = mapper.Map<IEnumerable<PlayerResponseViewModel>>(players);

        var totalPages = (int)Math.Ceiling((double)totalPlayers / request.Limit);

        return new BasePagedResponseViewModel<IEnumerable<PlayerResponseViewModel>>(response, request.Page, request.Limit, totalPlayers, totalPages);
    }
}