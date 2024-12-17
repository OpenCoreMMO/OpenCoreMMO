using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class GoToCommand : CommandSpell
{
    public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;
        if (Params?.Length == 0) return false;

        var actorPlayer = (IPlayer)actor;

        // just only GOD can teleport to other players
        if (Params.Length == 1)
        {
            var creatureManager = IoC.GetInstance<IGameCreatureManager>();
            creatureManager.TryGetPlayer(Params[0].ToString(), out var target);

            if (target is null || target.CreatureId == actorPlayer.CreatureId)
                return false;

            actorPlayer.TeleportTo(target.Location);
            return true;
        }

        if (Params?.Length != 3) return false;

        ushort.TryParse(Params[0].ToString(), out var x);
        ushort.TryParse(Params[1].ToString(), out var y);
        byte.TryParse(Params[2].ToString(), out var z);

        actor.TeleportTo(x, y, z);

        return true;
    }
}