using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnLogoutEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public PlayerOnLogoutEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(IPlayer player)
        => _creatureEvents.PlayerLogout(player);
}