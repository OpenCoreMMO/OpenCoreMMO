using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Game.Combat.Attacks;
using NeoServer.Game.Combat.Attacks.MeleeAttack;
using NeoServer.Modules.Combat.Attacks;
using NeoServer.Modules.Combat.Attacks.MeleeAttack;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class FeatureInjection
{
    public static IServiceCollection AddFeatures(this IServiceCollection builder)
    {
        AddCombatFeature(builder);
        return builder;
    }

    private static IServiceCollection AddCombatFeature(IServiceCollection builder)
    {
        var applicationAssembly = Assembly.GetAssembly(typeof(MeleeAttackStrategy));

        var gameAssembly = Assembly.GetAssembly(typeof(IAttackValidation));
        
        builder.RegisterAssembliesByInterface(typeof(IAttackValidation));
        builder.RegisterAssembliesByInterface(typeof(IAttackStrategy));
        
        builder.RegisterAssemblyTypes<IAttackStrategy>(gameAssembly, applicationAssembly);

        builder.AddSingleton<AttackCalculation>();
        
        builder.AddSingleton<AttackLibrary>();
        return builder;
    }
}