using Microsoft.Extensions.DependencyInjection;
using NeoServer.Modules.Movement.Creature.Follow;
using NeoServer.Modules.Movement.Creature.Walk;

namespace NeoServer.Server.Standalone.IoC.Modules;

public class MovementModule: IModuleRegister
{
    public IServiceCollection Register(IServiceCollection builder)
    {
        builder.AddSingleton<WalkService>();
        builder.AddSingleton<FollowRoutine>();
        return builder;
    }
}

