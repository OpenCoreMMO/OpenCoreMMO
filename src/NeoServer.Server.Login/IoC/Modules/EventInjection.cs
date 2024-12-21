//using System.Reflection;
//using Microsoft.Extensions.DependencyInjection;
//using NeoServer.Game.Common.Contracts;
//using NeoServer.Game.Common.Contracts.Chats;
//using NeoServer.Game.Common.Contracts.Creatures;
//using NeoServer.Game.Common.Contracts.Items;
//using NeoServer.Server.Events.Creature;
//using NeoServer.Server.Events.Subscribers;

//namespace NeoServer.Server.Login.IoC.Modules;

//public static class EventInjection
//{
//    public static IServiceCollection AddEvents(this IServiceCollection builder)
//    {
//        builder.RegisterServerEvents();
//        builder.RegisterGameEvents();
//        builder.RegisterEventSubscribers();
//        builder.AddSingleton<EventSubscriber>();
//        builder.AddSingleton<FactoryEventSubscriber>();

//        return builder;
//    }

//    private static void RegisterServerEvents(this IServiceCollection builder)
//    {
//        var assembly = Assembly.GetAssembly(typeof(CreatureAddedOnMapEventHandler));
//        builder.RegisterAssemblyTypes(assembly);
//    }

//    private static void RegisterGameEvents(this IServiceCollection builder)
//    {
//        builder.RegisterAssembliesByInterface(typeof(IGameEventHandler));
//    }

//    private static void RegisterEventSubscribers(this IServiceCollection builder)
//    {
//        var types = Container.AssemblyCache;

//        builder
//            .RegisterAssemblyTypes<ICreatureEventSubscriber>(types)
//            .RegisterAssemblyTypes<IItemEventSubscriber>(types)
//            .RegisterAssemblyTypes<IChatChannelEventSubscriber>(types);
//    }
//}