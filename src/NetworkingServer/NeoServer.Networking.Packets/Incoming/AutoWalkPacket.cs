using System;
using NeoServer.Domain.Common.Location;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Incoming;

public class AutoWalkPacket : IncomingPacket
{
    public AutoWalkPacket(IReadOnlyNetworkMessage message)
    {
        StepDistance = message.GetByte();

        Steps = new Direction[StepDistance];

        for (var i = 0; i < StepDistance; i++) Steps[i] = ParseByte(message.GetByte());
    }

    public byte StepDistance { get; }
    public Direction[] Steps { get; }

    private Direction ParseByte(byte b)
    {
        switch (b)
        {
            case 1: return Direction.East;
            case 2: return Direction.NorthEast;
            case 3: return Direction.North;
            case 4: return Direction.NorthWest;
            case 5: return Direction.West;
            case 6: return Direction.SouthWest;
            case 7: return Direction.South;
            case 8: return Direction.SouthEast;
            default:
                Console.WriteLine($"Invalid direction: {b}");
                return Direction.None;
        }
    }
}