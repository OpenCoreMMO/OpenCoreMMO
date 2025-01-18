//using NeoServer.Game.Common.Contracts;
//using NeoServer.Game.Common.Contracts.Creatures;
//using NeoServer.Game.Common.Contracts.World;
//using NeoServer.Scripts.LuaJIT.Enums;
//using NeoServer.Scripts.LuaJIT.Interfaces;

//namespace NeoServer.Scripts.LuaJIT.Events.MoveEvents;

//public class CreatureOnMoveEventHandler : IGameEventHandler
//{
//    private readonly IMoveEvents _moveEvents;

//    public CreatureOnMoveEventHandler(
//        IMoveEvents creatureEvents)
//    {
//        _moveEvents = creatureEvents;
//    }

//    public void Execute(
//        IWalkableCreature creature, ICylinder cylinder)
//    {
//        _moveEvents.OnCreatureMove(creature, cylinder.FromTile.Location, cylinder.ToTile.Location, MoveEventType.MOVE_EVENT_STEP_OUT);
//        _moveEvents.OnCreatureMove(creature, cylinder.FromTile.Location, cylinder.ToTile.Location, MoveEventType.MOVE_EVENT_STEP_IN);
//    }
//}