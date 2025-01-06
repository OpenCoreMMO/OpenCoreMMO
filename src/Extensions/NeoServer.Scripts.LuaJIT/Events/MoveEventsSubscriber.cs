using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Events.MoveEvents;

namespace NeoServer.Scripts.LuaJIT;

public class MoveEventsSubscriber : ICreatureEventSubscriber, IGameEventSubscriber
{
    private readonly CreatureOnMoveEventHandler _creatureOnMoveEventHandler;
    private readonly CreatureOnEquipEventHandler _creatureOnEquipEventHandler;
    private readonly CreatureOnDeEquipEventHandler _creatureOnDeEquipEventHandler;

    public MoveEventsSubscriber(
        CreatureOnMoveEventHandler creatureOnMoveEventHandler,
        CreatureOnEquipEventHandler creatureOnEquipEventHandler,
        CreatureOnDeEquipEventHandler creatureOnDeEquipEventHandler
    )
    {
        _creatureOnMoveEventHandler = creatureOnMoveEventHandler;

        _creatureOnEquipEventHandler = creatureOnEquipEventHandler;
        _creatureOnDeEquipEventHandler = creatureOnDeEquipEventHandler;
    }

    public void Subscribe(ICreature creature)
    {
        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnCreatureMoved += _creatureOnMoveEventHandler.Execute;
        }

        if (creature is IPlayer player)
        {
            player.OnEquipItem += _creatureOnEquipEventHandler.Execute;
            player.OnDeEquipItem += _creatureOnDeEquipEventHandler.Execute;
            //player.OnMoveItem += _creatureOnMoveEventHandler.Execute;
        }
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnCreatureMoved -= _creatureOnMoveEventHandler.Execute;
        }

        if (creature is IPlayer player)
        {
            player.OnEquipItem -= _creatureOnEquipEventHandler.Execute;
            player.OnDeEquipItem -= _creatureOnDeEquipEventHandler.Execute;
            //player.OnMoveItem += _creatureOnMoveEventHandler.Execute;
        }
    }
}