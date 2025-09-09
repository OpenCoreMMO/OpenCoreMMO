using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Combat.Monster;
using NeoServer.Domain.Combat.Player;
using NeoServer.Domain.Combat.Validations;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Inspection;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Monster.Services;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Items.Services;
using NeoServer.Domain.Items.Services.ItemTransform;
using NeoServer.Domain.Mail;
using NeoServer.Domain.Party;
using NeoServer.Domain.Quest;
using NeoServer.Domain.SafeTrade;
using NeoServer.Domain.SafeTrade.Operations;
using NeoServer.Domain.Services;
using NeoServer.Domain.Spells;
using NeoServer.Domain.World.Services;
using NeoServer.Networking.EventHandlers.Creature;
using NeoServer.Server.Commands.Player.UseItem;
using NeoServer.Server.Services;

namespace NeoServer.Server.Standalone.IoC.Modules;

/// <summary>
///     Contains the registration of various game services and operations for the dependency injection container.
/// </summary>
public static class ServiceInjection
{
    /// <summary>
    ///     Registers various game services and operations with the dependency injection container.
    /// </summary>
    /// <param name="builder">The container builder instance.</param>
    /// <returns>The container builder instance with the registered services and operations.</returns>
    public static IServiceCollection AddServices(this IServiceCollection builder)
    {
        //Game services
        builder.AddSingleton<IDealTransaction, DealTransaction>();
        builder.AddSingleton<ICoinTransaction, CoinTransaction>();
        builder.AddSingleton<ISharedExperienceConfiguration, SharedExperienceConfiguration>();
        builder.AddSingleton<IPartyInviteService, PartyInviteService>();
        builder.AddSingleton<ISummonService, SummonService>();
        builder.AddSingleton<IToMapMovementService, ToMapMovementService>();
        builder.AddSingleton<IMapService, MapService>();
        builder.AddSingleton<IMapTool, MapTool>();
        builder.AddSingleton<IPlayerUseService, PlayerUseService>();
        builder.AddSingleton<IItemMovementService, ItemMovementService>();
        builder.AddSingleton<IItemService, ItemService>();
        builder.AddSingleton<IStaticToDynamicTileService, StaticToDynamicTileService>();
        builder.AddSingleton<SafeTradeSystem>();
        builder.AddSingleton<IItemRequirementService, ItemRequirementService>();
        builder.AddSingleton<IExperienceSharingService, ExperienceSharingService>();
        builder.AddSingleton<ICreatureDeathService, CreatureDeathService>();
        builder.AddSingleton<IPlayerSkullService, PlayerSkullService>();
        builder.AddSingleton<ILootService, LootService>();
        builder.AddSingleton<IMailService, MailService>();
        builder.AddSingleton<QuestService>();
        builder.AddSingleton<AreaCalculationService>();
        builder.AddSingleton<MonsterStateService>();
        builder.AddSingleton<TargetDetectorService>();

        //Operations
        builder.AddSingleton<TradeItemExchanger>();

        //Items
        builder.AddSingleton<IDecayService, DecayService>();
        builder.AddSingleton<IItemTransformService, ItemTransformService>();
        builder.AddSingleton<IItemRemoveService, ItemRemoveService>();
        builder.AddSingleton<IItemAbilityApplierService, ItemAbilityApplierService>();
        builder.AddSingleton<ItemUseValidation>();

        builder.AddSingleton<MagicFieldService>();

        //game builders
        builder.RegisterAssemblyTypes<IInspectionTextBuilder>(Container.AssemblyCache);

        //application services
        builder.AddSingleton<HotkeyService>();
        builder.AddSingleton<PlayerLocationResolver>();

        builder.AddSingleton<IEventAggregator, EventAggregator>();

        Assembly.GetAssembly(typeof(PlayerConditionChangedEventHandler));
        builder.RegisterAssembliesByInterface(typeof(IApplicationEventHandler<>));

        builder.AddSingleton<IAttackService, AttackService>();
        builder.AddSingleton<AttackValidation>();
        builder.AddSingleton<SingleTargetAttackService>();
        builder.AddSingleton<AreaAttackService>();
        builder.AddSingleton<BloodPoolService>();
        builder.AddSingleton<ConditionAttackService>();

        builder.AddSingleton<MonsterCombatService>();
        builder.AddSingleton<PlayerCombatService>();

        //spells
        builder.AddSingleton<SpellService>();
        builder.AddSingleton<SpellCastValidation>();
        
        return builder;
    }
}