using NeoServer.Networking.Handlers;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Protocols;

public class LoginProtocol : Protocol
{
    private readonly PacketHandlerRouter _packetHandlerRouter;

    public LoginProtocol(PacketHandlerRouter packetHandlerRouter)
    {
        _packetHandlerRouter = packetHandlerRouter;
    }

    public override bool KeepConnectionOpen => false;

    public override void ProcessMessage(object sender, IConnectionEventArgs args)
    {
        var handler = _packetHandlerRouter.Create(args.Connection);
        handler?.HandleMessage(args.Connection.InMessage, args.Connection);
    }

    public override string ToString()
    {
        return "Login Protocol";
    }
}