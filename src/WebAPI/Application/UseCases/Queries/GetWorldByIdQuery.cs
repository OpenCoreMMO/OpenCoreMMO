using AutoMapper;
using MediatR;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response.World;

namespace NeoServer.Web.API.Application.UseCases.Queries;

public class GetWorldByIdQuery(IMapper mapper, IWorldRepository worldRepository) : IRequestHandler<GetWorldByIdRequest, WorldResponseViewModel>
{
    public async Task<WorldResponseViewModel> Handle(GetWorldByIdRequest request, CancellationToken cancellationToken)
    {
        var world = await worldRepository.GetAsync(request.Id);
        var response = mapper.Map<WorldResponseViewModel>(world);
        return response;
    }
}