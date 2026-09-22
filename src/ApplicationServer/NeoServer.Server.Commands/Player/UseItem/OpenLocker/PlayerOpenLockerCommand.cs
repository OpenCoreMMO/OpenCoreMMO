using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Locker;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player.UseItem.OpenLocker;

public class PlayerOpenLockerCommand(
    IPlayerUseService playerUseService,
    LockerLoaderService lockerLoaderService)
    : ICommand
{
    public void Execute(IPlayer player, Locker locker, UseItemPacket useItemPacket)
    {
        var playerLocker = lockerLoaderService.EnsureLocker(player, locker);

        playerLocker.SetNewLocation(useItemPacket.Location);

        playerUseService.Use(player, playerLocker, useItemPacket.Index);
    }
}