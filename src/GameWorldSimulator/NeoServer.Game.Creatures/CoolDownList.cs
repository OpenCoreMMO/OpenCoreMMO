using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Structs;

namespace NeoServer.Game.Creatures;

public class CooldownList
{
    public IDictionary<CooldownType, CooldownTime> Cooldowns { get; } =
        new Dictionary<CooldownType, CooldownTime>();

    private Dictionary<ulong, CooldownTime> CustomCooldowns { get; } = new();

    private Dictionary<string, CooldownTime> Spells { get; } = new();

    /// <summary>
    ///     Add cooldown
    /// </summary>
    /// <param name="type"></param>
    /// <param name="duration">milliseconds</param>
    public bool Start(CooldownType type, int duration)
    {
        if (Expired(type)) Cooldowns.Remove(type);
        return Cooldowns.TryAdd(type, new CooldownTime(DateTime.Now, duration));
    }

    public bool Start(string spell, int duration)
    {
        if (Expired(spell)) Spells.Remove(spell);
        return Spells.TryAdd(spell, new CooldownTime(DateTime.Now, duration));
    }

    public bool Start(ulong id, int duration)
    {
        if (Expired(id)) CustomCooldowns.Remove(id);
        return CustomCooldowns.TryAdd(id, new CooldownTime(DateTime.Now, duration));
    }

    public void Add(string spell, int duration)
    {
        Spells.TryAdd(spell, new CooldownTime(DateTime.Now, duration));
    }

    public bool Expired(CooldownType type)
    {
        if (Cooldowns.TryGetValue(type, out var cooldown)) return cooldown.Expired;
        return true;
    }

    public bool Expired(string spell)
    {
        if (Spells.TryGetValue(spell, out var cooldown)) return cooldown.Expired;
        return true;
    }

    public bool Expired(ulong id)
    {
        if (CustomCooldowns.TryGetValue(id, out var cooldown)) return cooldown.Expired;
        return true;
    }

    public void RestartCoolDown(CooldownType type, int duration)
    {
        if (Cooldowns.ContainsKey(type)) Cooldowns[type].Reset();
    }
}