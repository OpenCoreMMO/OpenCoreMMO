using NeoServer.Networking.Protocols;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Networking.Listeners;

public class GameListener : Listener
{
    public GameListener(GameProtocol protocol, ILogger logger, ServerConfiguration serverConfiguration) : base(
        serverConfiguration.ServerGamePort,
        protocol, logger)
    {
    }
}