using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.MoveEvents;

public class CreatureOnMoveEventHandler : IGameEventHandler
{
    private readonly IMoveEvents _moveEvents;

    public CreatureOnMoveEventHandler(
        IMoveEvents creatureEvents)
    {
        _moveEvents = creatureEvents;
    }

    public void Execute(
        IWalkableCreature creature,
        Location fromLocation,
        Location toLocation,
        ICylinderSpectator[] spectators)
    {
        _moveEvents.OnCreatureMove(creature, fromLocation, toLocation, MoveEventType.MOVE_EVENT_STEP_OUT);
        _moveEvents.OnCreatureMove(creature, fromLocation, toLocation, MoveEventType.MOVE_EVENT_STEP_IN);
    }
}