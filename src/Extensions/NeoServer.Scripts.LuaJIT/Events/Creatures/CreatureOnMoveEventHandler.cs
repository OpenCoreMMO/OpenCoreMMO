using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnMoveEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;
    private readonly INpcs _npcs;
    private readonly ILogger _logger;

    public CreatureOnMoveEventHandler(
        ICreatureEvents creatureEvents,
        INpcs npcs,
        ILogger logger)
    {
        _creatureEvents = creatureEvents;
        _npcs = npcs;
        _logger = logger;
    }

    public void Execute(
        ICreature self,
        ICreature creature,
        Location fromLocation,
        Location toLocation)
    {
        if (self is INpc npc)
        {
            var npcEvent = _npcs.GetEvents(npc.Name);

            if (npcEvent == null ||
                npcEvent.Events == null ||
                npcEvent.Events.Count == 0 ||
                !npcEvent.Events.TryGetValue(NpcsEventType.NPCS_EVENT_MOVE, out var onMoveEvent) ||
                !onMoveEvent.HasValue)
                return;

            // onCreatureMove(self, creature, oldPosition, newPosition)
            var callback = new CreatureCallback(npcEvent.LuaScriptInterface, self, _logger);
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

                    //onPlayerCloseChannel
                    OnPlayerCloseChannel(npc, player, npcEvent);
                }
                else if (npc.CanInteract(toLocation) && npc.IsInteractingWithPlayer(player))
                {
                    npc.TurnTo(player);
                }

                if (npc.CanSee(player))
                {
                    //todo: develop this?
                    //onPlayerAppear
                    //void Npc::onPlayerAppear(const std::shared_ptr<Player> &player) {
                    //if (player->hasFlag(PlayerFlags_t::IgnoredByNpcs) || playerSpectators.contains(player))
                    //{
                    //    return;
                    //}
                    //playerSpectators.emplace(player);
                    //manageIdle();
                    //}
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

        if (!npcEvent.Events.TryGetValue(NpcsEventType.NPCS_EVENT_PLAYER_CLOSE_CHANNEL, out var onCloseChannelEvent) ||
            !onCloseChannelEvent.HasValue)
            return;

        var callback = new CreatureCallback(npcEvent.LuaScriptInterface, npc, _logger);
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