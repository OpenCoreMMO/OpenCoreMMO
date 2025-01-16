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

public class CreatePlayerCommand (IPlayerRepository playerRepository, IOptions<PlayerConfig> config) :  IRequestHandler<CreatePlayerRequest, OutputResponse>
{
    public async Task<OutputResponse> Handle(CreatePlayerRequest request, CancellationToken cancellationToken)
    {
        var playerAlreadyExist = await playerRepository.GetPlayer(request.Name);
        
        if (playerAlreadyExist is not null)
            return new OutputResponse(ErrorMessage.PlayerAlreadyExist);

        var player = new PlayerEntity
        {
            AccountId = request.AccountId,
            Level = config.Value.Level,
            Capacity = config.Value.Capacity,
            Experience = config.Value.Experience,
            Gender = (Gender)request.Sex,
            WorldId = config.Value.WorldId,
            Health = config.Value.Health,
            MaxHealth = config.Value.MaxHealth,
            Mana = config.Value.Mana,
            MaxMana = config.Value.MaxMana,
            Soul = config.Value.Soul,
            Speed = config.Value.Speed,
            Name = request.Name,
            FightMode = config.Value.FightMode,
            LookType = config.Value.LookType,
            LookBody = config.Value.LookBody,
            LookFeet = config.Value.LookFeet,
            LookHead = config.Value.LookHead,
            LookLegs = config.Value.LookLegs,
            MagicLevel = config.Value.MagicLevel,
            Vocation = (byte)request.Vocation,
            ChaseMode = config.Value.ChaseMode,
            MaxSoul = config.Value.MaxSoul,
            Group = config.Value.Group,
            PosX = request.PosX,
            PosY = request.PosY,
            PosZ = request.PosZ,
            SkillAxe = config.Value.SkillAxe,
            SkillDist = config.Value.SkillDist,
            SkillClub = config.Value.SkillClub,
            SkillSword = config.Value.SkillSword,
            SkillShielding = config.Value.SkillShielding,
            SkillFist = config.Value.SkillFist,
            TownId = request.Town,
            SkillFishing = config.Value.SkillFishing
        };

        await playerRepository.Add(player);
        
        return new OutputResponse(player.Id);
    }
}