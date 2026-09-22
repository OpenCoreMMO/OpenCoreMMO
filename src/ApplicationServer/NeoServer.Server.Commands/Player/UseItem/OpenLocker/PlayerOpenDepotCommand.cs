using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Locker;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player.UseItem.OpenLocker;

public class PlayerOpenDepotCommand(
    IPlayerUseService playerUseService,
    DepotLoaderService depotLoaderService) : ICommand
{
    public void Execute(IPlayer player, IContainer depotChest, UseItemPacket useItemPacket)
    {
        var playerDepot = depotLoaderService.LoadDepotChest(player, depotChest);

        playerDepot.SetNewLocation(useItemPacket.Location);

        playerUseService.Use(player, playerDepot, useItemPacket.Index);
    }
}
