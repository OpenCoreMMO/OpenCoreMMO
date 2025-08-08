using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Spells;
using NeoServer.Domain.Spells.Entities;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class KickPlayerCommand : CommandSpell
{
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        var commands = Words.Split("/kick");

        if (string.IsNullOrWhiteSpace(commands[1])) return Result.NotPossible;

        var ctx = IoC.GetInstance<IGameCreatureManager>();

        if (!ctx.TryGetPlayer(commands[1], out var player)) return Result.NotPossible;

        if (player is null || player.CreatureId == caster.CreatureId) return Result.NotPossible;

        var playerLogOutCommand = IoC.GetInstance<PlayerLogOutCommand>();
        playerLogOutCommand.Execute(player, true);

        return Result.Success;
    }
}