using Microsoft.Extensions.DependencyInjection;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Game.Common.Contracts.DataStores;

namespace NeoServer.Server.Login.IoC.Modules;

public static class DataStoreInjection
{
    public static IServiceCollection AddDataStores(this IServiceCollection builder)
    {
        builder.AddSingleton<IItemTypeStore, ItemTypeStore>();

        builder.AddSingleton<IChatChannelStore, ChatChannelStore>();

        builder.AddSingleton<IGuildStore, GuildStore>();

        builder.AddSingleton<INpcStore, NpcStore>();

        builder.AddSingleton<IVocationStore, VocationStore>();

        builder.AddSingleton<ICoinTypeStore, CoinTypeStore>();

        builder.AddSingleton<IAreaEffectStore, AreaEffectStore>();

        builder.AddSingleton<IPlayerOutFitStore, PlayerOutFitStore>();

        builder.AddSingleton<IQuestDataStore, QuestDataDataStore>();

        builder.AddSingleton<IItemClientServerIdMapStore, ItemClientServerIdMapStore>();

        return builder;
    }
}