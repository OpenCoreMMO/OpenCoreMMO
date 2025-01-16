using NeoServer.Web.API.IoC.Configs;

namespace NeoServer.Web.API.IoC.Modules;

public static class DefaultValuesInjection
{
    public static void AddDefaultValuesInjection(this  WebApplicationBuilder builder)
    {
        builder.Services.Configure<PlayerConfig>(
            builder.Configuration.GetSection("Defaults:PlayerDefault"));
    }
}