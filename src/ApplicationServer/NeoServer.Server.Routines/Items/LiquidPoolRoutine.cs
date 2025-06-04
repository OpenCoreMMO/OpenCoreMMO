using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Items.Items;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Routines.Items;

public class LiquidPoolRoutine
{
    public static void Execute(LiquidPool item, IGameServer game)
    {
        if (item is not { Decay.Expired: true }) return;

        var tile = game.Map[item.Location] as IDynamicTile;
        if (item.Decay.TryDecay()) game.Map.CreateBloodPool(item, tile); //todo: need to review this

        if (item.Decay.ShouldDisappear) game.Map.CreateBloodPool(null, tile);
    }
}