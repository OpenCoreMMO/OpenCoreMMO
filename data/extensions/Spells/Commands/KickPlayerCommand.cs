using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class KickPlayerCommand : CommandSpell
{
    public override bool OnCast(ICombatActor caster, string words, out InvalidOperation error)
    {
        var commands = words.Split("/kick");

        if (string.IsNullOrWhiteSpace(commands[1]))
        {
            error = InvalidOperation.NotPossible;
            return false;
        }

        var ctx = IoC.GetInstance<IGameCreatureManager>();

        if (!ctx.TryGetPlayer(commands[1], out var player))
        {
            error = InvalidOperation.NotPossible;
            return false;
        }

        if (player is null || player.CreatureId == caster.CreatureId)
        {
            error = InvalidOperation.NotPossible;
            return false;
        }

        var playerLogOutCommand = IoC.GetInstance<PlayerLogOutCommand>();
        playerLogOutCommand.Execute(player, true);
        error = InvalidOperation.None;
        return true;
    }
}