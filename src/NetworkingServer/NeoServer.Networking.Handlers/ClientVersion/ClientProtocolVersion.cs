using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Networking.Handlers.ClientVersion;

public class ClientProtocolVersion
{
    private readonly ILogger _logger;
    private readonly ServerConfiguration _serverConfiguration;

    public ClientProtocolVersion(ILogger logger, ServerConfiguration serverConfiguration)
    {
        _logger = logger;
        _serverConfiguration = serverConfiguration;
    }

    public bool IsSupported(uint version)
    {
        if (version == _serverConfiguration.Version) return true;

        _logger.Warning("Client protocol version {Version} is not supported", version);
        return false;
    }
}