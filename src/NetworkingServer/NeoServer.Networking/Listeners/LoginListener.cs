using NeoServer.Networking.Protocols;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Networking.Listeners;

public class LoginListener : Listener
{
    public LoginListener(LoginProtocol protocol, ILogger logger, ServerConfiguration serverConfiguration)
        : base(serverConfiguration.ServerLoginPort, protocol, logger)
    {
    }
}