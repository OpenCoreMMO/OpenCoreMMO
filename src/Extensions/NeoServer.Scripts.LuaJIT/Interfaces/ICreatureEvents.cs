using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface ICreatureEvents
{
    public bool PlayerLogin(IPlayer player);

    public bool PlayerLogout(IPlayer player);

    public bool PlayerAdvance(IPlayer player, ISkill skill, int oldLevel, int newLevel);

    public CreatureEvent GetEventByName(string eventName, bool forceLoaded = true);

    public IEnumerable<CreatureEvent> GetCreatureEvents(CreatureEventType eventType);

    public bool RegisterLuaEvent(CreatureEvent creatureEvent);

    public void RemoveInvalidEvents();

    public void Clear();
}
