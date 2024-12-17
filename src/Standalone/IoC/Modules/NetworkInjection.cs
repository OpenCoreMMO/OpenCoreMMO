
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Networking.Handlers.ClientVersion;
using NeoServer.Networking.Listeners;
using NeoServer.Networking.Protocols;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class NetworkInjection
{
    public static IServiceCollection AddNetwork(this IServiceCollection builder)
    {
        builder.AddSingleton<LoginProtocol>();
        builder.AddSingleton<GameProtocol>();
        builder.AddSingleton<LoginListener>();
        builder.AddSingleton<GameListener>();
        builder.AddSingleton<ClientProtocolVersion>();
        return builder;
    }
}