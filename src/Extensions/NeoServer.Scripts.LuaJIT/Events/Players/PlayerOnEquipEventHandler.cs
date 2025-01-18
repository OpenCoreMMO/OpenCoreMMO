using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnEquipEventHandler : IGameEventHandler
{
    private readonly IMoveEvents _moveEvents;

    public PlayerOnEquipEventHandler(
        IMoveEvents creatureEvents)
    {
        _moveEvents = creatureEvents;
    }

    public void Execute(
         IPlayer player,
         IItem item,
         bool isCheck)
    {
        _moveEvents.OnPlayerEquip(player, item, item.Metadata.BodyPosition, isCheck);
    }
}