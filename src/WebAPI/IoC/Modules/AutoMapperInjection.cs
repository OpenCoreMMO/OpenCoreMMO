namespace NeoServer.Web.API.IoC.Modules;

public static class AutoMapperInjection
{
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(NeoServer.Web.API.Program).Assembly);
        return services;
    }
}