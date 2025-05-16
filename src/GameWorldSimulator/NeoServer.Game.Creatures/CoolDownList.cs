using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures.Monsters;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Structs;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Game.Creatures;

public class CooldownList
{
    public IDictionary<CooldownType, CooldownTime> Cooldowns { get; } =
        new Dictionary<CooldownType, CooldownTime>();

    private Dictionary<ulong, CooldownTime> CustomCooldowns { get; } = new();

    private Dictionary<string, CooldownTime> SpellCooldowns { get; } = new();
    private Dictionary<SpellGroup, CooldownTime> SpellGroupCooldowns { get; } = new();
    private Dictionary<string, CooldownTime> SummonCooldowns { get; set; }

    /// <summary>
    ///     Add cooldown
    /// </summary>
    /// <param name="type"></param>
    /// <param name="duration">milliseconds</param>
    public bool Start(CooldownType type, uint duration)
    {
        if (Expired(type)) Cooldowns.Remove(type);
        return Cooldowns.TryAdd(type, new CooldownTime(DateTime.Now, duration));
    }

    public bool Start(ISpell spell)
    {
        // Start new cooldown only if previous has expired
        if (!Expired(spell)) return false;

        SpellCooldowns.Remove(spell.Name);
        SpellCooldowns.TryAdd(spell.Name, new CooldownTime(DateTime.Now, spell.Cooldown));

        if (spell.Groups is null) return true;

        // Handle group cooldowns
        var i = 0;
        foreach (var group in spell.Groups)
        {
            if (!Expired(group)) continue;

            SpellGroupCooldowns.Remove(group);
            SpellGroupCooldowns.TryAdd(group, new CooldownTime(DateTime.Now, spell.GroupCooldown[i++]));
        }

        return true;
    }

    public bool Start(ulong id, uint duration)
    {
        if (Expired(id)) CustomCooldowns.Remove(id);
        return CustomCooldowns.TryAdd(id, new CooldownTime(DateTime.Now, duration));
    }

    public bool Start(IMonsterSummon summon)
    {
        SummonCooldowns ??= new();

        if (Expired(summon)) SummonCooldowns.Remove(summon.Name);
        return SummonCooldowns.TryAdd(summon.Name, new CooldownTime(DateTime.Now, summon.Interval));
    }

    public bool Expired(CooldownType type) => !Cooldowns.TryGetValue(type, out var cooldown) || cooldown.Expired;

    public bool Expired(IMonsterSummon summon)
    {
        SummonCooldowns ??= new();

        return !SummonCooldowns.TryGetValue(summon.Name, out var cooldown) || cooldown.Expired;
    }

    public bool Expired(ISpell spell)
    {
        // Check individual spell cooldown first
        var spellExpired = !SpellCooldowns.TryGetValue(spell.Name, out var cooldown) || cooldown.Expired;
        if (!spellExpired) return false;

        if (spell.Groups == null || spell.Groups.Length == 0) return true;

        // Check all spell group cooldowns
        var groupsExpired = true;

        foreach (var group in spell.Groups)
        {
            SpellGroupCooldowns.TryGetValue(group, out var cooldownTime);
            groupsExpired &= cooldownTime.Expired;
        }

        return groupsExpired;
    }

    public bool Expired(SpellGroup spellGroup)
    {
        if (SpellGroupCooldowns.TryGetValue(spellGroup, out var cooldown)) return cooldown.Expired;
        return true;
    }

    public bool Expired(ulong id)
    {
        if (CustomCooldowns.TryGetValue(id, out var cooldown)) return cooldown.Expired;
        return true;
    }
}