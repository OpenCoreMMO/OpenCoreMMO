using MediatR;
using Microsoft.Extensions.Options;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Web.API.IoC.Configs;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Application.UseCases.Commands;

public class DeleteWorldCommand (IWorldRepository worldRepository) :  IRequestHandler<DeleteWorldCommandRequest, OutputResponse>
{
    public async Task<OutputResponse> Handle(DeleteWorldCommandRequest request, CancellationToken cancellationToken)
    {
        var worldEntity = await worldRepository.GetAsync(request.Id);
        
        if (worldEntity is null)
            return new OutputResponse(ErrorMessage.WorldNotFound);

        if (worldEntity.DeletedAt is not null)
            return new OutputResponse(ErrorMessage.WorldAlreadyDeleted);

        worldEntity.DeletedAt = DateTime.UtcNow;

        await worldRepository.Update(worldEntity);

        return new();
    }
}