using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnLogoutEventHandler(ICreatureEvents creatureEvents) : IApplicationEventHandler<PlayerLogoutEvent>
{
    public void Handle(PlayerLogoutEvent @event) => creatureEvents.PlayerLogout(@event.Player);
}