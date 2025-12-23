using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Networking.Packets.Outgoing.Map;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Creature;

public class CreatureMovedUpPacket : OutgoingPacket
{
    private readonly ICreature _creature;
    private readonly Location _fromLocation;
    private readonly IMap _map;
    private readonly Location _toLocation;

    public CreatureMovedUpPacket(Location fromLocation, Location toLocation, IMap map, ICreature creature)
    {
        _fromLocation = fromLocation;
        _toLocation = toLocation;
        _map = map;
        _creature = creature;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.FloorChangeUp);

        var skip = -1;
        var x = (ushort)(_fromLocation.X - MapViewPort.MaxClientViewPortX);
        var y = (ushort)(_fromLocation.Y - MapViewPort.MaxClientViewPortY);

        //going from surface to underground
        if (_toLocation.Z == 7)
            for (var i = 5; i >= 0; --i)
                message.AddBytes(MapDescriptionBuilder.GetFloorDescription(_map, _creature, x, y, (byte)i,
                    (byte)MapViewPort.MaxClientViewPortX * 2 + 2,
                    (byte)MapViewPort.MaxClientViewPortY * 2 + 2, 8 - i, ref skip).ToArray());

        if (_toLocation.Z > 7)
            message.AddBytes(MapDescriptionBuilder.GetFloorDescription(_map, _creature,
                (ushort)(_fromLocation.X - MapViewPort.MaxClientViewPortX),
                (ushort)(_fromLocation.Y - MapViewPort.MaxClientViewPortY),
                (byte)(_fromLocation.Z - 3), (byte)MapViewPort.MaxClientViewPortX * 2 + 2,
                (byte)MapViewPort.MaxClientViewPortY * 2 + 2, 3, ref skip).ToArray());

        if (skip >= 0)
        {
            message.AddByte((byte)skip);
            message.AddByte(0xFF);
        }

        //west
        message.AddByte((byte)GameOutgoingPacketType.MapSliceWest);
        message.AddBytes(MapDescriptionBuilder.GetDescription(_map, _creature, x, (ushort)(y + 1), _toLocation.Z, 1).ToArray());

        //north
        message.AddByte((byte)GameOutgoingPacketType.MapSliceNorth);
        message.AddBytes(MapDescriptionBuilder
            .GetDescription(_map, _creature, x, y, _toLocation.Z, (byte)MapViewPort.MaxClientViewPortX * 2 + 2, 1).ToArray());
    }
}