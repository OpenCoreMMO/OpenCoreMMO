using Microsoft.Extensions.DependencyInjection;
using NeoServer.Game.Common.Contracts.UseCase;
using NeoServer.Game.Creatures.UseCases.Monster;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class UseCaseInjection
{
    public static IServiceCollection AddUseCase(this IServiceCollection builder)
    {
        builder.AddSingleton<ICreateMonsterOrSummonUseCase, CreateMonsterOrSummonUseCase>();
        return builder;
    }
}