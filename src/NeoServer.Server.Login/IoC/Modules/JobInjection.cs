//using Microsoft.Extensions.DependencyInjection;
//using NeoServer.Server.Routines.Channels;
//using NeoServer.Server.Routines.Creatures;
//using NeoServer.Server.Routines.Items;
//using NeoServer.Server.Routines.Persistence;

//namespace NeoServer.Server.Login.IoC.Modules;

//public static class JobInjection
//{
//    public static IServiceCollection AddJobs(this IServiceCollection builder)
//    {
//        //todo: inherit these jobs from interface and register by implementation
//        builder.AddSingleton<GameCreatureRoutine>();
//        builder.AddSingleton<GameItemRoutine>();
//        builder.AddSingleton<GameChatChannelRoutine>();
//        builder.AddSingleton<PlayerPersistenceRoutine>();
//        return builder;
//    }
//}