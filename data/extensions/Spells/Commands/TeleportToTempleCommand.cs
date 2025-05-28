using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
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

        if (!gameManager.TryGetPlayer(playerName, out var player))
        {
            return Result.Fail(InvalidOperation.PlayerNotFound);
        }

        var location = new Location(player.Town.Coordinate);
        player.TeleportTo(location);
        EffectService.Send(location, EffectT.BubbleBlue);
        return Result.Success;
    }
}