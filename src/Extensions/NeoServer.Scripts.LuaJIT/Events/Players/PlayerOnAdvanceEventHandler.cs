using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnAdvanceEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public PlayerOnAdvanceEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(IPlayer player, SkillType skill, int oldValue, int newValue)
    {
        _creatureEvents.PlayerAdvance(player, skill, oldValue, newValue);
    }
}