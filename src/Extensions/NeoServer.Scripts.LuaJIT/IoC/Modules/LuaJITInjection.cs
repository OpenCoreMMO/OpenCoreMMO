using Microsoft.Extensions.DependencyInjection;
using NeoServer.Scripts.LuaJIT.Functions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Scripts.LuaJIT.Services;

namespace NeoServer.Scripts.LuaJIT.IoC.Modules;

public static class LuaJitInjection
{
    public static IServiceCollection Register(this IServiceCollection builder)
    {
        builder.AddSingleton<ILuaStartup, LuaStartup>();
        builder.AddSingleton<IConfigManager, ConfigManager>();
        builder.AddSingleton<IReloadManager, ReloadManager>();
        builder.AddSingleton<ILuaEnvironment, LuaEnvironment>();
        builder.AddSingleton<IScripts, Scripts>();
        builder.AddSingleton<IActions, Actions>();
        builder.AddSingleton<ICreatureEvents, CreatureEvents>();
        builder.AddSingleton<IGlobalEvents, GlobalEvents>();
        builder.AddSingleton<IMoveEvents, MoveEvents>();
        builder.AddSingleton<INpcs, Npcs>();
        builder.AddSingleton<ITalkActions, TalkActions>();
        builder.AddSingleton<IEventsCallbacks, EventsCallbacks>();
        builder.AddSingleton<LuaHelperService>();

        builder.AddSingleton<IActionFunctions, ActionFunctions>();
        builder.AddSingleton<IConditionFunctions, ConditionFunctions>();
        builder.AddSingleton<IConfigFunctions, ConfigFunctions>();
        builder.AddSingleton<IContainerFunctions, ContainerFunctions>();
        builder.AddSingleton<ICreatureFunctions, CreatureFunctions>();
        builder.AddSingleton<ICreatureEventFunctions, CreatureEventFunctions>();
        builder.AddSingleton<IDBFunctions, DBFunctions>();
        builder.AddSingleton<IEnumFunctions, EnumFunctions>();
        builder.AddSingleton<IGameFunctions, GameFunctions>();
        builder.AddSingleton<IGlobalFunctions, GlobalFunctions>();
        builder.AddSingleton<IGlobalEventFunctions, GlobalEventFunctions>();
        builder.AddSingleton<IGroupFunctions, GroupFunctions>();
        builder.AddSingleton<IItemFunctions, ItemFunctions>();
        builder.AddSingleton<IItemTypeFunctions, ItemTypeFunctions>();
        builder.AddSingleton<ILoggerFunctions, LoggerFunctions>();
        builder.AddSingleton<IMonsterFunctions, MonsterFunctions>();
        builder.AddSingleton<IMoveEventFunctions, MoveEventFunctions>();
        builder.AddSingleton<INpcFunctions, NpcFunctions>();
        builder.AddSingleton<INpcTypeFunctions, NpcTypeFunctions>();
        builder.AddSingleton<IPlayerFunctions, PlayerFunctions>();
        builder.AddSingleton<IResultFunctions, ResultFunctions>();
        builder.AddSingleton<IPositionFunctions, PositionFunctions>();
        builder.AddSingleton<ITalkActionFunctions, TalkActionFunctions>();
        builder.AddSingleton<ITeleportFunctions, TeleportFunctions>();
        builder.AddSingleton<ITileFunctions, TileFunctions>();
        builder.AddSingleton<IBankFunctions, BankFunctions>();
        builder.AddSingleton<IGuildFunctions, GuildFunctions>();

        builder.AddSingleton<ISpellFunctions, SpellFunctions>();
        builder.AddSingleton<ICombatFunctions, CombatFunctions>();
        builder.AddSingleton<IVariantFunctions, VariantFunctions>();
        builder.AddSingleton<IMonsterTypeFunctions, MonsterTypeFunctions>();
        builder.AddSingleton<ITownFunctions, TownFunctions>();
        builder.AddSingleton<IEventCallbackFunctions, EventCallbackFunctions>();

        builder.AddSingleton<LuaCombatService>();
        builder.AddSingleton<NonAggressiveCombatService>();
        return builder;
    }
}