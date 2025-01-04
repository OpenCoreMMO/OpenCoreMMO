using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnLoginEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public PlayerOnLoginEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(IPlayer player)
        => _creatureEvents.PlayerLogin(player);
}