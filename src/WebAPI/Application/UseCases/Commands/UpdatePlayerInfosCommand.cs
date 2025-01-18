using MediatR;
using NeoServer.Data.Interfaces;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Application.UseCases.Commands;

public class UpdatePlayerInfosCommand (IPlayerRepository playerRepository) :  IRequestHandler<UpdatePlayerInfosRequest, OutputResponse>
{
    public async Task<OutputResponse> Handle(UpdatePlayerInfosRequest request, CancellationToken cancellationToken)
    {
        var entity = await playerRepository.GetAsync(request.Id);

        if (entity is null)
            return new OutputResponse(ErrorMessage.PlayerNotFound);
        
        if (entity.Name != request.Name)
        {
            var alreadyExistWithThisName = await playerRepository.GetPlayer(request.Name);
        
            if (alreadyExistWithThisName is not null)
                return new OutputResponse(ErrorMessage.PlayerNameAlreadyExist);
        }
        
        entity.Soul = request.Soul;
        entity.Health = request.Health;
        entity.MaxHealth = request.MaxHealth;
        entity.Mana = request.Mana;
        entity.MaxMana = request.MaxMana;
        entity.Speed = request.Speed;
        entity.ChaseMode = request.ChaseMode;
        entity.FightMode = request.FightMode;
        entity.Gender = request.Gender;
        entity.Vocation = request.Vocation;
        entity.TownId = request.TownId;
        entity.Level = request.Level;
        entity.MaxSoul = request.MaxSoul;
        entity.Group = request.Group;
        entity.Name = request.Name; 
        
        await playerRepository.Update(entity);
        return new OutputResponse();
    }
}