using Microsoft.Extensions.DependencyInjection;
using NeoServer.Game.Common.Contracts.Inspection;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Creatures.Party;
using NeoServer.Game.Creatures.Services;
using NeoServer.Game.Items.Services;
using NeoServer.Game.Items.Services.ItemTransform;
using NeoServer.Game.Systems.SafeTrade;
using NeoServer.Game.Systems.SafeTrade.Operations;
using NeoServer.Game.Systems.Services;
using NeoServer.Game.World.Services;
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

        //Operations
        builder.AddSingleton<TradeItemExchanger>();

        //Items
        builder.AddSingleton<IDecayService, DecayService>();
        builder.AddSingleton<IItemTransformService, ItemTransformService>();
        builder.AddSingleton<IItemRemoveService, ItemRemoveService>();

        //game builders
        builder.RegisterAssemblyTypes<IInspectionTextBuilder>(Container.AssemblyCache);

        //application services
        builder.AddSingleton<HotkeyService>();
        builder.AddSingleton<PlayerLocationResolver>();

        return builder;
    }
}