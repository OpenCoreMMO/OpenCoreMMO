using Autofac;
using NeoServer.Server.Routines.Channels;
using NeoServer.Server.Routines.Creatures;
using NeoServer.Server.Routines.Items;
using NeoServer.Server.Routines.Persistence;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class JobInjection
{
    public static ContainerBuilder AddJobs(this ContainerBuilder builder)
    {
        //todo: inherit these jobs from interface and register by implementation
        builder.RegisterType<GameCreatureRoutine>().SingleInstance();
        builder.RegisterType<GameItemRoutine>().SingleInstance();
        builder.RegisterType<GameChatChannelRoutine>().SingleInstance();
        builder.RegisterType<PlayerPersistenceRoutine>().SingleInstance();
        return builder;
    }
}