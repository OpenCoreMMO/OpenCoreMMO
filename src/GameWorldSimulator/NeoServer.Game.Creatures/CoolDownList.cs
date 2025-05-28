using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures.Monsters;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Structs;

namespace NeoServer.Game.Creatures;

public class CooldownList
{
    public IDictionary<CooldownType, CooldownTime> Cooldowns { get; } =
        new Dictionary<CooldownType, CooldownTime>();

    private Dictionary<ulong, CooldownTime> CustomCooldowns { get; } = new();

    private Dictionary<Guid, CooldownTime> SpellCooldowns { get; } = new();
    private Dictionary<int, CooldownTime> SpellGroupCooldowns { get; } = new();
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

    public bool Start(IHasCooldown spell)
    {
        // Start new cooldown only if previous has expired
        if (!Expired(spell)) return false;

        SpellCooldowns.Remove(spell.CooldownId);
        SpellCooldowns.TryAdd(spell.CooldownId, new CooldownTime(DateTime.Now, spell.Cooldown));

        if (spell.HasAnyCooldownGroup) return true;

        // Handle group cooldowns
        var i = 0;


        if (spell.PrimaryGroup.Id > 0 && GroupExpired(spell.PrimaryGroup.Id))
        {
            SpellGroupCooldowns.Remove(spell.PrimaryGroup.Id);
            SpellGroupCooldowns.TryAdd(spell.PrimaryGroup.Id,
                new CooldownTime(DateTime.Now, spell.PrimaryGroup.Cooldown));
        }
        
        if (spell.SecondaryGroup.Id > 0 && GroupExpired(spell.SecondaryGroup.Id))
        {
            SpellGroupCooldowns.Remove(spell.SecondaryGroup.Id);
            SpellGroupCooldowns.TryAdd(spell.SecondaryGroup.Id,
                new CooldownTime(DateTime.Now, spell.SecondaryGroup.Cooldown));
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

    public bool Expired(IHasCooldown spell)
    {
        // Check individual spell cooldown first
        var spellExpired = !SpellCooldowns.TryGetValue(spell.CooldownId, out var cooldown) || cooldown.Expired;
        if (!spellExpired) return false;

        if (spell.HasAnyCooldownGroup) return true;

        // Check all spell group cooldowns
        var groupsExpired = true;

        if (spell.PrimaryGroup.Id > 0 && GroupExpired(spell.PrimaryGroup.Id))
        {
            SpellGroupCooldowns.TryGetValue(spell.PrimaryGroup.Id, out var cooldownTime);
            groupsExpired &= cooldownTime.Expired;
        }
        
        if (spell.SecondaryGroup.Id > 0 && GroupExpired(spell.SecondaryGroup.Id))
        {
            SpellGroupCooldowns.TryGetValue(spell.SecondaryGroup.Id, out var cooldownTime);
            groupsExpired &= cooldownTime.Expired;
        }

        return groupsExpired;
    }

    public bool GroupExpired(int groupId)
    {
        if (SpellGroupCooldowns.TryGetValue(groupId, out var cooldown)) return cooldown.Expired;
        return true;
    }

    public bool Expired(ulong id)
    {
        if (CustomCooldowns.TryGetValue(id, out var cooldown)) return cooldown.Expired;
        return true;
    }
}