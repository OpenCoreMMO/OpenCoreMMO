// using System;
// using NeoServer.Server.Common.Contracts;
// using NeoServer.Server.Common.Contracts.Network;
// using NeoServer.Server.Common.Contracts.Network.Enums;
// using NeoServer.Server.Common.Contracts.Tasks;
// using Serilog;
//
// namespace NeoServer.Networking.Handlers.Invalid;
//
// public class NotImplementedPacketHandler : PacketHandler
// {
//     private readonly ILogger _logger;
//     private readonly GameIncomingPacketType _packet;
//     public NotImplementedPacketHandler(GameIncomingPacketType packet, ILogger logger, IGameCreatureManager creatureManager, IDispatcher dispatcher)
//     {
//         _packet = packet;
//         _logger = logger;
//     }
//
//     public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
//     {
//         var enumText = Enum.GetName(_packet);
//
//         enumText = string.IsNullOrWhiteSpace(enumText) ? _packet.ToString("x") : enumText;
//         _logger.Error("Incoming Packet not handled: {Packet}", enumText);
//     }
// }

