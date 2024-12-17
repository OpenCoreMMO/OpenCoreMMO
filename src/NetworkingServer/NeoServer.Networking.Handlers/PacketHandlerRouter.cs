using System;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Network.Enums;
using Serilog;

namespace NeoServer.Networking.Handlers;


public class PacketHandlerRouter
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public PacketHandlerRouter(ILogger logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public IPacketHandler Create(IConnection connection)
    {
        var packet = GameIncomingPacketType.PlayerLogOut;

        if (!connection.Disconnected) packet = connection.InMessage.GetIncomingPacketType(connection.IsAuthenticated);

        if (!connection.IsAuthenticated && RequireAuthentication(packet))
        {
            HandleNotAllowedPacket(packet);
            return null;
        }

        if (!InputHandlerMap.Data.TryGetValue(packet, out var handlerType))
        {
            HandleNotImplementedPacket(packet);
            return null;
        }

        var packetHandler = _serviceProvider.GetService(handlerType);

        if (packetHandler is null)
        {
            HandleNotImplementedPacket(packet);
            return null;
        }

        _logger.Debug("{Incoming}: {Packet}", "Incoming Packet", packet);

        return (IPacketHandler)packetHandler;
    }

    private static bool RequireAuthentication(GameIncomingPacketType gameIncomingPacketType)
    {
        return gameIncomingPacketType is not (GameIncomingPacketType.PlayerLogIn
            or GameIncomingPacketType.PlayerLoginRequest);
    }

    private void HandleNotImplementedPacket(GameIncomingPacketType packet)
    {
        var enumText = Enum.GetName(typeof(GameIncomingPacketType), packet);

        enumText = string.IsNullOrWhiteSpace(enumText) ? packet.ToString("x") : enumText;
        _logger.Error("Incoming Packet not handled: {Packet}", enumText);
    }

    private void HandleNotAllowedPacket(GameIncomingPacketType packet)
    {
        var enumText = Enum.GetName(typeof(GameIncomingPacketType), packet);

        enumText = string.IsNullOrWhiteSpace(enumText) ? packet.ToString("x") : enumText;
        _logger.Error("Incoming Packet not allowed: {Packet}", enumText);
    }
}