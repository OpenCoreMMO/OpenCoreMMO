using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.World;

namespace NeoServer.Web.API.Application.UseCases.Queries;

public class GetWorldsQuery(IMapper mapper, IWorldRepository worldRepository) : IRequestHandler<GetWorldsRequest, BasePagedResponseViewModel<IEnumerable<WorldResponseViewModel>>>
{
    public async Task<BasePagedResponseViewModel<IEnumerable<WorldResponseViewModel>>> Handle(GetWorldsRequest request, CancellationToken cancellationToken)
    {
        Expression<Func<WorldEntity, bool>> expression = item =>
            (request.Name == null || item.Name.ToLower().Contains(request.Name.ToLower())) &&
            (request.Continent == null || item.Region == request.Continent) &&
            (request.PvpType == null || item.PvpType == request.PvpType) &&
            (request.Mode == null || item.Mode == request.Mode) && 
            (request.TransferEnabled == null || item.TransferEnabled == request.TransferEnabled) && 
            (request.AntiCheatEnabled == null || item.AntiCheatEnabled == request.AntiCheatEnabled) && 
            (request.RequiresPremium == null || item.RequiresPremium == request.RequiresPremium);

        var totalWorlds = await worldRepository.CountAllAsync(expression);
        var players = await worldRepository.GetPaginatedWorldsAsync(expression, request.Page, request.Limit);
        var response = mapper.Map<IEnumerable<WorldResponseViewModel>>(players);

        var totalPages = (int)Math.Ceiling((double)totalWorlds / request.Limit);

        return new BasePagedResponseViewModel<IEnumerable<WorldResponseViewModel>>(response, request.Page, request.Limit, totalWorlds, totalPages);
    }
}