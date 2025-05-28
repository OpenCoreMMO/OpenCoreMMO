using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Results;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class GoToCommand : CommandSpell
{
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (Params?.Length == 0) return Result.NotApplicable;

        var actorPlayer = (IPlayer)caster;

        // just only GOD can teleport to other players
        if (Params.Length == 1)
        {
            var creatureManager = IoC.GetInstance<IGameCreatureManager>();
            creatureManager.TryGetPlayer(Params[0].ToString(), out var targetPlayer);

            if (targetPlayer is null || targetPlayer.CreatureId == actorPlayer.CreatureId)
                return Result.NotApplicable;

            actorPlayer.TeleportTo(targetPlayer.Location);
            return Result.Success;
        }

        if (Params?.Length != 3) return Result.NotApplicable;

        ushort.TryParse(Params[0].ToString(), out var x);
        ushort.TryParse(Params[1].ToString(), out var y);
        byte.TryParse(Params[2].ToString(), out var z);

        caster.TeleportTo(x, y, z);

        return Result.Success;
    }
}