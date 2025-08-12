using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;
using System.Linq;

namespace NeoServer.Networking.Packets.Outgoing.Item;

public class AddAtStackPositionPacket(IMap map, ICreature creature, byte stackPosition, IPlayer player) : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        if (stackPosition >= 10)
        {
            message.AddByte((byte)GameOutgoingPacketType.TileUpdate);
            message.AddLocation(creature.Location);

            if (creature.Tile != null && player != null)
            {
                message.AddBytes(map.GetTileDescription(creature.Tile, player).ToArray());

                message.AddByte(0x00);
                message.AddByte(0xFF);
            }
            else
            {
                message.AddByte(0x01);
                message.AddByte(0xFF);
            }
        }
        else
        {
            message.AddByte((byte)GameOutgoingPacketType.AddAtStackPos);
            message.AddLocation(creature.Location);
            message.AddByte(stackPosition);
        }
    }
}

public class UpdateTilePacket(Location location, ITile tile = null) : OutgoingPacket
{
    public override void WriteToMessage(INetworkMessage message)
    {
        message.AddByte((byte)GameOutgoingPacketType.TileUpdate);
        message.AddLocation(location);

        if (tile != null)
        {
            message.AddByte(0x00);
            message.AddByte(0xFF);
        }
        else
        {
            message.AddByte(0x01);
            message.AddByte(0xFF);
        }
    }
}