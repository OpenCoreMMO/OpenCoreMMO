using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class CreatureEvents(ILogger logger) : ICreatureEvents
{
    private readonly Dictionary<string, CreatureEvent> _creatureEvents = new();
    private readonly Dictionary<uint, IList<CreatureEvent>> _mappedCreatureEvents = new();

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

    public bool PlayerAdvance(IPlayer player, SkillType skill, int oldLevel, int newLevel)
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
            logger.Information("Duplicate registered GlobalEvent with name: {CreatureEventName}", creatureEvent.Name);
            return false;
        }

        var oldEvent = GetEventByName(creatureEvent.Name, false);
        if (oldEvent is not null)
        {
            // if there was an event with the same that is not loaded
            //(happens when reloading), it is reused
            if (!oldEvent.Loaded && oldEvent.EventType == creatureEvent.EventType)
            {
                oldEvent.CopyEvent(creatureEvent);
            }

            return false;
        }

        // if not, register it normally
        _creatureEvents.Add(creatureEvent.Name, creatureEvent);
        return true;
    }

    public void RemoveInvalidEvents()
    {
        var eventsToRemove = _creatureEvents.Values.Where(c => c.GetScriptId() == 0);

        foreach (var creatureEvent in eventsToRemove)
            _creatureEvents.Remove(creatureEvent.Name);
    }

    public void Clear()
    {
        foreach (var (_, creatureEvent) in _creatureEvents)
        {
            creatureEvent.ClearEvent();
        }
    }

    public IEnumerable<CreatureEvent> GetCreatureEvents(uint creatureId, CreatureEventType eventType)
        => _mappedCreatureEvents.FirstOrDefault(c => c.Key == creatureId).Value?.Where(c => c.EventType == eventType) ?? new List<CreatureEvent>();

    private int _scriptEventsBitField = 0;

    public bool HasEventRegistered(CreatureEventType eventType)
        => (0 != (_scriptEventsBitField & (1 << (int)eventType)));

    public bool RegisterCreatureEvent(uint creatureId, CreatureEvent creatureEvent)
    {
        IList<CreatureEvent> mappedCreatureEvents = null;

        if (HasEventRegistered(creatureEvent.EventType) &&
            _mappedCreatureEvents.TryGetValue(creatureId, out mappedCreatureEvents) &&
            mappedCreatureEvents.Any(v => v.Name.Equals(creatureEvent.Name)))
            return false;
        
        _scriptEventsBitField |= 1 << (int)creatureEvent.EventType;

        mappedCreatureEvents ??= new List<CreatureEvent>();

        mappedCreatureEvents.Add(creatureEvent);

        _mappedCreatureEvents.AddOrUpdate(creatureId, mappedCreatureEvents);

        return true;
    }

    public bool UnregisterCreatureEvent(uint creatureId, CreatureEvent creatureEvent)
    {
        if (!HasEventRegistered(creatureEvent.EventType))
            return false;

        var resetTypeBit = true;

        var mappedCreatureEvents = new List<CreatureEvent>();

        for (var i = 0; i < mappedCreatureEvents.Count; i++)
        {
            var curEvent = mappedCreatureEvents.ElementAt(i);

            if (curEvent == creatureEvent)
            {
                mappedCreatureEvents.Remove(curEvent);
                continue;
            }

            if (curEvent.EventType == creatureEvent.EventType)
                resetTypeBit = false;
        }

        if (resetTypeBit)
            _scriptEventsBitField &= ~((1) << (int)creatureEvent.EventType);

        _mappedCreatureEvents.AddOrUpdate(creatureId, mappedCreatureEvents);

        return true;
    }
}
