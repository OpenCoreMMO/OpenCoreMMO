using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers;

public abstract class PacketHandler : IPacketHandler
{
    public abstract void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection);
}