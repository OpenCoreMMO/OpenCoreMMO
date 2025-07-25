using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
namespace NeoServer.Web.Shared.IoC.Modules;

public static class AutoMapperInjection
{
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services, Assembly assembly)
    {
        services.AddAutoMapper(assembly);
        return services;
    }
}