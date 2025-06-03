using System;
using System.Linq;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Packets.Outgoing.Map;

public class MapPartialDescriptionPacket : OutgoingPacket
{
    private readonly Direction _direction;
    private readonly Location _fromLocation;
    private readonly IMap _map;
    private readonly IThing _thing;
    private readonly Location _toLocation;

    public MapPartialDescriptionPacket(IThing thing, Location fromLocation, Location toLocation,
        Direction direction, IMap map)
    {
        _thing = thing;
        _toLocation = toLocation;
        _map = map;
        _direction = direction;
        _fromLocation = fromLocation;
    }

    public override void WriteToMessage(INetworkMessage message)
    {
        WriteDirectionMapSlice(message, _direction);
    }

    private void WriteDirectionMapSlice(INetworkMessage message, Direction direction = Direction.None)
    {
        var directionTo = direction == Direction.None ? _fromLocation.DirectionTo(_toLocation, true) : direction;

        switch (directionTo)
        {
            case Direction.East:
                message.AddByte((byte)GameOutgoingPacketType.MapSliceEast);
                break;

            case Direction.West:
                message.AddByte((byte)GameOutgoingPacketType.MapSliceWest);
                break;
            case Direction.North:
                message.AddByte((byte)GameOutgoingPacketType.MapSliceNorth);
                break;
            case Direction.South:
                message.AddByte((byte)GameOutgoingPacketType.MapSliceSouth);
                break;
            case Direction.NorthEast:
                WriteDirectionMapSlice(message, Direction.North);
                WriteDirectionMapSlice(message, Direction.East);
                return;
            case Direction.NorthWest:
                WriteDirectionMapSlice(message, Direction.North);
                WriteDirectionMapSlice(message, Direction.West);
                return;
            case Direction.SouthWest:
                WriteDirectionMapSlice(message, Direction.South);
                WriteDirectionMapSlice(message, Direction.West);
                return;
            case Direction.SouthEast:
                WriteDirectionMapSlice(message, Direction.South);
                WriteDirectionMapSlice(message, Direction.East);
                return;

            default:
                throw new ArgumentException("No direction received");
        }

        message.AddBytes(GetDescription(_thing, _fromLocation, _toLocation, _map, directionTo));
    }

    private byte[] GetDescription(IThing thing, Location fromLocation, Location toLocation, IMap map,
        Direction direction)
    {
        var newLocation = toLocation;

        byte width = 1;
        byte height = 1;

        switch (direction)
        {
            case Direction.East:
                newLocation.X = (ushort)(toLocation.X + MapViewPort.MaxClientViewPortX + 1);
                newLocation.Y = (ushort)(toLocation.Y - MapViewPort.MaxClientViewPortY);
                height = MapConstants.DEFAULT_MAP_WINDOW_SIZE_Y;
                break;
            case Direction.West:
                newLocation.X = (ushort)(toLocation.X - MapViewPort.MaxClientViewPortX);
                newLocation.Y = (ushort)(toLocation.Y - MapViewPort.MaxClientViewPortY);
                height = MapConstants.DEFAULT_MAP_WINDOW_SIZE_Y;
                break;
            case Direction.North:
                newLocation.X = (ushort)(fromLocation.X - MapViewPort.MaxClientViewPortX);
                newLocation.Y = (ushort)(toLocation.Y - MapViewPort.MaxClientViewPortY);
                width = MapConstants.DEFAULT_MAP_WINDOW_SIZE_X;
                break;
            case Direction.South:
                newLocation.X = (ushort)(fromLocation.X - MapViewPort.MaxClientViewPortX);
                newLocation.Y = (ushort)(toLocation.Y + MapViewPort.MaxClientViewPortY + 1);
                width = MapConstants.DEFAULT_MAP_WINDOW_SIZE_X;
                break;
        }

        return
            map.GetDescription(thing, newLocation.X,
                newLocation.Y,
                toLocation.Z, width, height).ToArray();
    }
}