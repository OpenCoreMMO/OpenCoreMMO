﻿using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Event;
using NeoServer.Game.Common.Location;
using NeoServer.Networking.Packets.Network;
using NeoServer.Networking.Packets.Network.Enums;

namespace NeoServer.PacketHandler.Modules.Players.Walk;

public class PlayerWalkPacketHandler : PacketHandler
{
    private readonly IGameServer _game;

    public PlayerWalkPacketHandler(IGameServer game)
    {
        _game = game;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var direction = ParseMovementPacket(message.IncomingPacket);

        if (_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player))
            _game.Dispatcher.AddEvent(new Event(() => player.WalkTo(direction)));
    }

    private Direction ParseMovementPacket(GameIncomingPacketType walkPacket)
    {
        var direction = Direction.North;

        switch (walkPacket)
        {
            case GameIncomingPacketType.WalkEast:
                direction = Direction.East;
                break;
            case GameIncomingPacketType.WalkNorth:
                direction = Direction.North;
                break;
            case GameIncomingPacketType.WalkSouth:
                direction = Direction.South;
                break;
            case GameIncomingPacketType.WalkWest:
                direction = Direction.West;
                break;
            case GameIncomingPacketType.WalkNorteast:
                direction = Direction.NorthEast;
                break;
            case GameIncomingPacketType.WalkNorthwest:
                direction = Direction.NorthWest;
                break;
            case GameIncomingPacketType.WalkSoutheast:
                direction = Direction.SouthEast;
                break;
            case GameIncomingPacketType.WalkSouthwest:
                direction = Direction.SouthWest;
                break;
        }

        return direction;
    }
}