using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Spells;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;
using NeoServer.Server.Services;

namespace NeoServer.Extensions.Spells.Commands;

public class TeleportToTempleCommand : CommandSpell
{
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        var playerName = Params?.Length > 0 ? Params[0].ToString() : caster.Name;
        var gameManager = IoC.GetInstance<IGameCreatureManager>();

        if (!gameManager.TryGetPlayer(playerName, out var player)) return Result.Fail(InvalidOperation.PlayerNotFound);

        var location = new Location(player.Town.Coordinate);
        player.TeleportTo(location);
        EffectService.Send(location, EffectT.BubbleBlue);
        return Result.Success;
    }
}