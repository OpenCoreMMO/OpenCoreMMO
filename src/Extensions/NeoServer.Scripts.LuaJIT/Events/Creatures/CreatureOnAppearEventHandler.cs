using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnAppearEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;
    private readonly INpcs _npcs;
    private readonly ILogger _logger;

    public CreatureOnAppearEventHandler(
        ICreatureEvents creatureEvents,
        INpcs npcs,
        ILogger logger)
    {
        _creatureEvents = creatureEvents;
        _npcs = npcs;
        _logger = logger;
    }

    public void Execute(ICreature self, ICreature creature)
    {
        if (self is INpc npc)
        {
            var npcEvent = _npcs.GetEvents(npc.Name);

            if (npcEvent == null ||
                npcEvent.Events == null ||
                npcEvent.Events.Count == 0 ||
                !npcEvent.Events.TryGetValue(NpcsEventType.NPCS_EVENT_APPEAR, out var onAppearEvent) ||
                !onAppearEvent.HasValue)
                return;

            // onCreatureAppear(self, creature)
            var callback = new CreatureCallback(npcEvent.LuaScriptInterface, self, _logger);
            if (callback.StartScriptInterface(onAppearEvent.Value))
            {
                callback.PushSpecificCreature(self);
                callback.PushCreature(creature);

                if (callback.PersistLuaState())
                    return;
            }
        }
    }
}