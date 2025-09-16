using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Spells.Entities;
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
            return Result.Fail(InvalidOperation.PlayerNotFound);

        var newLocation = caster.Location.GetNextLocation(caster.Direction);

        player.TeleportTo(newLocation);
        return Result.Success;
    }
}