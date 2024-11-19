using Microsoft.Extensions.DependencyInjection;

namespace NeoServer.Server.Standalone.IoC.Modules;

public interface IModuleRegister
{
    IServiceCollection Register(IServiceCollection builder);
}