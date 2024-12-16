using NeoServer.Networking.Handlers;
using NeoServer.Networking.Packets.Security;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Protocols;

public class GameProtocol : Protocol
{
    private readonly PacketHandlerRouter _handlerFactory;

    public GameProtocol(PacketHandlerRouter handlerFactory)
    {
        _handlerFactory = handlerFactory;
    }

    public override bool KeepConnectionOpen => true;

    public override string ToString()
    {
        return "Game Protocol";
    }

    public override void OnAccept(IConnection connection)
    {
        HandlerFirstConnection(connection);
        base.OnAccept(connection);
    }

    private void HandlerFirstConnection(IConnection connection)
    {
        connection.SendFirstConnection();
    }

    public override void ProcessMessage(object sender, IConnectionEventArgs args)
    {
        var connection = args.Connection;

        HandleMessage(args, connection);
    }

    private void HandleMessage(IConnectionEventArgs args, IConnection connection)
    {
        if (connection.IsAuthenticated && !connection.Disconnected)
            Xtea.Decrypt(connection.InMessage, 6, connection.XteaKey);

        if (_handlerFactory.Create(args.Connection) is not { } handler) return;

        handler?.HandleMessage(args.Connection.InMessage, args.Connection);
    }
}