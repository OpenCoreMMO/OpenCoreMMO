using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Networking.EventHandlers;
using NeoServer.Server.Events.Creature;
using NeoServer.Server.Events.Subscribers;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class EventInjection
{
    extension(IServiceCollection builder)
    {
        public IServiceCollection AddEvents()
        {
            builder.RegisterServerEvents();
            builder.RegisterGameEvents();
            builder.RegisterNetworkEvents();
            builder.RegisterEventSubscribers();
            builder.AddSingleton<EventSubscriber>();
            builder.AddSingleton<FactoryEventSubscriber>();

            return builder;
        }

        private void RegisterServerEvents()
        {
            var assembly = Assembly.GetAssembly(typeof(CreatureChangedVisibilityEventHandler));
            builder.RegisterAssemblyTypes(assembly);
        }

        private void RegisterNetworkEvents()
        {
            builder.RegisterAssembliesByInterface(typeof(INetworkEventHandler<>));
        }

        private void RegisterGameEvents()
        {
            builder.RegisterAssembliesByInterface(typeof(IGameEventHandler));
        }

        private void RegisterEventSubscribers()
        {
            var types = Container.AssemblyCache;

            builder
                .RegisterAssemblyTypes<ICreatureEventSubscriber>(types)
                .RegisterAssemblyTypes<IItemEventSubscriber>(types)
                .RegisterAssemblyTypes<IChatChannelEventSubscriber>(types);
        }
    }
}