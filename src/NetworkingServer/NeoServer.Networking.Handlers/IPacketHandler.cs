using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers;

public interface IPacketHandler
{
    void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection);
}