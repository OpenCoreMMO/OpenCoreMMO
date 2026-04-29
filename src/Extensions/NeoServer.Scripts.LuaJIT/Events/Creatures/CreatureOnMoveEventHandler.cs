using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnMoveEventHandler(
    INpcs npcs,
    ILogger logger)
    : IApplicationEventHandler<CreatureMoveEvent>
{
    public void Handle(CreatureMoveEvent @event)
    {
        if (@event is null) return;

        var self = @event.Self;
        var creature = @event.Creature;
        var fromLocation = @event.FromLocation;
        var toLocation = @event.ToLocation;

        if (self is INpc npc)
        {
            var npcEvent = npcs.GetEvents(npc.Name);

            if (npcEvent == null ||
                npcEvent.Events == null ||
                npcEvent.Events.Count == 0 ||
                !npcEvent.Events.TryGetValue(NpcEventType.NPCS_EVENT_MOVE, out var onMoveEvent) ||
                !onMoveEvent.HasValue)
                return;

            var callback = new CreatureCallback(npcEvent.LuaScriptInterface, self, logger);
            if (callback.StartScriptInterface(onMoveEvent.Value))
            {
                callback.PushSpecificCreature(self);
                callback.PushCreature(creature);
                callback.PushPosition(fromLocation);
                callback.PushPosition(toLocation);

                if (callback.PersistLuaState())
                    return;

                if (creature is not IPlayer player)
                    return;

                if (!npc.CanInteract(toLocation) && npc.IsInteractingWithPlayer(player))
                {
                    if (npc is IShopperNpc shopperNpc)
                    {
                        shopperNpc.RemovePlayerInteraction(player);
                        player.StopShopping();
                    }

                    OnPlayerCloseChannel(npc, player, npcEvent);
                }
                else if (npc.CanInteract(toLocation) && npc.IsInteractingWithPlayer(player))
                {
                    npc.TurnTo(player);
                }

                if (npc.CanSee(player))
                {
                }
                else
                {
                    npc.RemovePlayerInteraction(player);
                }
            }
        }
    }

    private void OnPlayerCloseChannel(INpc npc, IPlayer player, NpcEvents npcEvent)
    {
        if (!npcEvent.Events.TryGetValue(NpcEventType.NPCS_EVENT_PLAYER_CLOSE_CHANNEL, out var onCloseChannelEvent) ||
            !onCloseChannelEvent.HasValue)
            return;

        var callback = new CreatureCallback(npcEvent.LuaScriptInterface, npc, logger);
        if (callback.StartScriptInterface(onCloseChannelEvent.Value))
        {
            callback.PushSpecificCreature(npc);
            callback.PushCreature(player);

            if (callback.PersistLuaState())
                return;

            npc.RemovePlayerInteraction(player);
        }
    }
}
