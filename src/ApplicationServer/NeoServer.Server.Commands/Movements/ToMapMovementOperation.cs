using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Location;
using NeoServer.Networking.Packets.Incoming;

namespace NeoServer.Server.Commands.Movements;

public class ToMapMovementOperation
{
    public static void Execute(IPlayer player, ItemThrowPacket itemThrow, IToMapMovementService toMapMovementService)
    {
        var movementParams = new MovementParams(itemThrow.FromLocation, itemThrow.ToLocation, itemThrow.Count);
        toMapMovementService.Move(player, movementParams);
    }

    public static bool IsApplicable(ItemThrowPacket itemThrowPacket)
    {
        return itemThrowPacket.ToLocation.Type == LocationType.Ground;
    }
}