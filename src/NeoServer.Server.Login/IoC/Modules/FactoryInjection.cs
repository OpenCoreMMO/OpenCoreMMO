using Microsoft.Extensions.DependencyInjection;
using NeoServer.Game.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Creatures.Factories;
using NeoServer.Game.Items.Factories;
using NeoServer.Game.Items.Factories.AttributeFactory;
using NeoServer.Game.World.Factories;
using NeoServer.Networking.Handlers;

namespace NeoServer.Server.Login.IoC.Modules;

public static class FactoryInjection
{
    public static IServiceCollection AddFactories(this IServiceCollection builder)
    {
        builder.AddSingleton<IItemFactory, ItemFactory>();
        builder.AddSingleton<DefenseEquipmentFactory>();
        builder.AddSingleton<WeaponFactory>();
        builder.AddSingleton<ContainerFactory>();
        builder.AddSingleton<GroundFactory>();
        builder.AddSingleton<RuneFactory>();
        builder.AddSingleton<CumulativeFactory>();
        builder.AddSingleton<GenericItemFactory>();

        builder.AddSingleton<ProtectionFactory>();
        builder.AddSingleton<DecayableFactory>();
        builder.AddSingleton<SkillBonusFactory>();
        builder.AddSingleton<ChargeableFactory>();

        builder.AddSingleton<ChatChannelFactory>();

        builder.AddSingleton<ILiquidPoolFactory, LiquidPoolFactory>();
        builder.AddSingleton<ICreatureFactory, CreatureFactory>();
        builder.AddSingleton<IMonsterFactory, MonsterFactory>();
        builder.AddSingleton<INpcFactory, NpcFactory>();
        builder.AddSingleton<ITileFactory, TileFactory>();
        builder.AddSingleton<PacketHandlerRouter>();

        return builder;
    }

    public static IServiceCollection AddFactoriesLogin(this IServiceCollection builder)
    {
        builder.AddSingleton<PacketHandlerRouter>();

        return builder;
    }
}