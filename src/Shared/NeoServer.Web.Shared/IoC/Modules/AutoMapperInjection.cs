using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace NeoServer.Web.API.IoC.Modules;

public static class AutoMapperInjection
{
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services, Assembly assembly)
    {
        services.AddAutoMapper(assembly);
        return services;
    }
}