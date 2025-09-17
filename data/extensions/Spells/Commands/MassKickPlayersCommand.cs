using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Spells.Entities;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class MassKickPlayersCommand : CommandSpell
{
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        var ctx = IoC.GetInstance<IGameCreatureManager>();
        var playerLogOutCommand = IoC.GetInstance<PlayerLogOutCommand>();

        foreach (var player in ctx.GetAllLoggedPlayers())
        {
            if (player is null || player.CreatureId == caster.CreatureId)
                continue;

            playerLogOutCommand.Execute(player, true);
        }

        return Result.Success;
    }
}