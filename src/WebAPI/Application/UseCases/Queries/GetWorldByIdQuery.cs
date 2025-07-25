using MediatR;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response.World;

namespace NeoServer.Web.API.Application.UseCases.Queries;

public class GetWorldByIdQuery(IWorldRepository worldRepository)
    : IRequestHandler<GetWorldByIdRequest, WorldResponseViewModel>
{
    public async Task<WorldResponseViewModel> Handle(GetWorldByIdRequest request, CancellationToken cancellationToken)
        => await worldRepository.GetAsync(request.Id);
}