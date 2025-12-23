using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Routines.Items;

public class LiquidPoolRoutine
{
    public static void Execute(ILiquid item, IGameServer game)
    {
        if (item is not { Decay.Expired: true }) return;

        var tile = game.Map[item.Location] as IDynamicTile;
        if (item.Decay.TryDecay()) tile?.ReplaceItemByGroup(item); //todo: need to review this

        if (item.Decay.ShouldDisappear) tile?.ReplaceItemByGroup(item);
    }
}