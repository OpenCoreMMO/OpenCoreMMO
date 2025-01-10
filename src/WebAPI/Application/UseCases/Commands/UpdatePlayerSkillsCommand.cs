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

public class UpdatePlayerSkillsCommand (IPlayerRepository playerRepository) :  IRequestHandler<UpdatePlayerSkillsRequest, OutputResponse>
{
    public async Task<OutputResponse> Handle(UpdatePlayerSkillsRequest request, CancellationToken cancellationToken)
    {
        var entity = await playerRepository.GetAsync(request.Id);

        if (entity is null)
            return new OutputResponse(ErrorMessage.PlayerNotFound);
        
        entity.SkillAxe = request.SkillAxe;
        entity.SkillDist = request.SkillDist;
        entity.SkillClub = request.SkillClub;
        entity.SkillSword = request.SkillSword;
        entity.SkillShielding = request.SkillShielding;
        entity.SkillFist = request.SkillFist;
        entity.SkillFishing = request.SkillFishing;
        
        await playerRepository.Update(entity);
        return new OutputResponse();
    }
}