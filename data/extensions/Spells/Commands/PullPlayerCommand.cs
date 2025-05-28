using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Results;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class PullPlayerCommand : CommandSpell
{
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (Params?.Length == 0) return Result.NotApplicable;

        var gameManager = IoC.GetInstance<IGameCreatureManager>();

        if (!gameManager.TryGetPlayer(Params[0].ToString(), out var player))
        {
            return Result.Fail(InvalidOperation.PlayerNotFound);
        }

        var newLocation = caster.Location.GetNextLocation(caster.Direction);

        player.TeleportTo(newLocation);
        return Result.Success;
    }
}