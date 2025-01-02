using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class CreatureEvents : ICreatureEvents
{
    private readonly Dictionary<string, CreatureEvent> _creatureEvents = new Dictionary<string, CreatureEvent>();

    private ILogger _logger;

    public CreatureEvents(
        ILogger logger)
    {
        _logger = logger;
    }

    public CreatureEvent GetEventByName(string name, bool forceLoaded)
    {
        if (_creatureEvents.TryGetValue(name, out var creatureEvent) && (!forceLoaded || creatureEvent.Loaded))
            return creatureEvent;
        return null;
    }

    public IEnumerable<CreatureEvent> GetCreatureEvents(CreatureEventType eventType)
        => _creatureEvents.Values.Where(c => c.EventType == eventType);

    public bool PlayerLogin(IPlayer player)
    {
        foreach (var creatureEvent in _creatureEvents.Values.Where(c => c.EventType == CreatureEventType.CREATURE_EVENT_LOGIN))
            if (!creatureEvent.ExecuteOnLogin(player))
                return false;

        return true;
    }

    public bool PlayerLogout(IPlayer player)
    {
        foreach (var creatureEvent in _creatureEvents.Values.Where(c => c.EventType == CreatureEventType.CREATURE_EVENT_LOGOUT))
            if (!creatureEvent.ExecuteOnLogout(player))
                return false;

        return true;
    }

    public bool PlayerAdvance(IPlayer player, ISkill skill, int oldLevel, int newLevel)
    {
        foreach (var creatureEvent in _creatureEvents.Values.Where(c => c.EventType == CreatureEventType.CREATURE_EVENT_LOGOUT))
            if (!creatureEvent.ExecuteOnAdvance(player, skill, oldLevel, newLevel))
                return false;

        return true;
    }

    public bool RegisterLuaEvent(CreatureEvent creatureEvent)
    {
        if (creatureEvent.EventType == CreatureEventType.CREATURE_EVENT_NONE)
        {
            _logger.Information($"Duplicate registered globalevent with name: {creatureEvent.Name}");
            return false;
        }

        var oldEvent = GetEventByName(creatureEvent.Name, false);
        if (oldEvent is not null)
        {
            // if there was an event with the same that is not loaded
            //(happens when realoading), it is reused
            if (!oldEvent.Loaded && oldEvent.EventType == creatureEvent.EventType)
            {
                oldEvent.CopyEvent(creatureEvent);
            }

            return false;
        }
        else
        {
            // if not, register it normally
            _creatureEvents.Add(creatureEvent.Name, creatureEvent);
            return true;
        }
    }

    public void RemoveInvalidEvents()
    {
        var eventsToRemove = _creatureEvents.Values.Where(c => c.GetScriptId() == 0);

        foreach (var creatureEvent in eventsToRemove)
            _creatureEvents.Remove(creatureEvent.Name);
    }

    public void Clear()
    {
        foreach (var (name, creatureEvent) in _creatureEvents)
        {
            creatureEvent.ClearEvent();
        }
    }
}
