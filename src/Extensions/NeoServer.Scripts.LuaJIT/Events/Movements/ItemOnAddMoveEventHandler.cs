//using NeoServer.Game.Common.Contracts;
//using NeoServer.Game.Common.Contracts.Items;
//using NeoServer.Game.Common.Contracts.World;
//using NeoServer.Scripts.LuaJIT.Interfaces;

//namespace NeoServer.Scripts.LuaJIT.Events.MoveEvents;

//public class ItemOnAddMoveEventHandler : IGameEventHandler
//{
//    private readonly IMoveEvents _moveEvents;

//    public ItemOnAddMoveEventHandler(
//        IMoveEvents creatureEvents)
//    {
//        _moveEvents = creatureEvents;
//    }

//    public void Execute(IThing thing, ICylinder cylinder)
//        => _moveEvents.OnItemMove(thing as IItem, cylinder.ToTile, true);
//}