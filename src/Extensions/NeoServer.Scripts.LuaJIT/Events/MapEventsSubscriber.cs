using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Scripts.LuaJIT.Events.MoveEvents;

namespace NeoServer.Scripts.LuaJIT;

public class MapEventsSubscriber : IMapEventSubscriber, IGameEventSubscriber
{
    private readonly CreatureOnMoveEventHandler _creatureOnMoveEventHandler;
    private readonly ItemOnAddMoveEventHandler _itemOnAddMoveEventHandler;
    private readonly ItemOnRemoveMoveEventHandler _itemOnRemoveMoveEventHandler;

    public MapEventsSubscriber(
        CreatureOnMoveEventHandler creatureOnMoveEventHandler,
        ItemOnAddMoveEventHandler itemOnAddMoveEventHandler,
        ItemOnRemoveMoveEventHandler itemOnRemoveMoveEventHandler
    )
    {
        _creatureOnMoveEventHandler = creatureOnMoveEventHandler;
        _itemOnAddMoveEventHandler = itemOnAddMoveEventHandler;
        _itemOnRemoveMoveEventHandler = itemOnRemoveMoveEventHandler;
    }

    public void Subscribe(IMap map)
    {
        map.OnCreatureMoved += _creatureOnMoveEventHandler.Execute;
        map.OnThingAddedToTile += _itemOnAddMoveEventHandler.Execute;
        map.OnThingRemovedFromTile += _itemOnRemoveMoveEventHandler.Execute;
    }

    public void Unsubscribe(IMap map)
    {
        map.OnCreatureMoved -= _creatureOnMoveEventHandler.Execute;
        map.OnThingAddedToTile -= _itemOnAddMoveEventHandler.Execute;
        map.OnThingRemovedFromTile -= _itemOnRemoveMoveEventHandler.Execute;
    }
}