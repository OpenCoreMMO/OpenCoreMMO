using Microsoft.Extensions.DependencyInjection;
using NeoServer.Domain.Chat.Factory;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Creatures.Factories;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.Items.Factories.AttributeFactory;
using NeoServer.Domain.World.Factories;
using NeoServer.Networking.Handlers;

namespace NeoServer.Server.Standalone.IoC.Modules;

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

        builder.AddSingleton<IHouseFactory, HouseFactory>();

        return builder;
    }
}