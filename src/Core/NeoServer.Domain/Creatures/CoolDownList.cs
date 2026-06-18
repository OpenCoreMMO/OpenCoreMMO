using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures.Monsters;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;

namespace NeoServer.Domain.Creatures;

public class CooldownList
{
    public IDictionary<CooldownType, CooldownTime> Cooldowns { get; } =
        new Dictionary<CooldownType, CooldownTime>();

    private Dictionary<ulong, CooldownTime> CustomCooldowns { get; } = new();

    private Dictionary<Guid, CooldownTime> GuidCooldowns { get; } = new();
    private Dictionary<string, CooldownTime> SpellGroupCooldowns { get; } = new();
    private Dictionary<string, CooldownTime> SummonCooldowns { get; set; }

    /// <summary>
    ///     Add cooldown
    /// </summary>
    /// <param name="type"></param>
    /// <param name="duration">milliseconds</param>
    public bool Start(CooldownType type, uint duration)
    {
        if (Expired(type)) Cooldowns.Remove(type);
        return Cooldowns.TryAdd(type, new CooldownTime(DateTime.UtcNow, duration));
    }

    public bool Start(Guid id, uint duration)
    {
        // Start new cooldown only if previous has expired
        if (!Expired(id)) return false;

        GuidCooldowns.Remove(id);
        GuidCooldowns.TryAdd(id, new CooldownTime(DateTime.UtcNow, duration));

        return true;
    }

    public bool Start(IHasCooldown spell)
    {
        // Start new cooldown only if previous has expired
        if (!Expired(spell)) return false;

        GuidCooldowns.Remove(spell.CooldownId);
        GuidCooldowns.TryAdd(spell.CooldownId, new CooldownTime(DateTime.UtcNow, spell.Cooldown));

        if (spell.HasAnyCooldownGroup) return true;

        // Handle group cooldowns

        if (!string.IsNullOrWhiteSpace(spell.PrimaryGroup.Name) && GroupExpired(spell.PrimaryGroup.Name))
        {
            SpellGroupCooldowns.Remove(spell.PrimaryGroup.Name);
            SpellGroupCooldowns.TryAdd(spell.PrimaryGroup.Name,
                new CooldownTime(DateTime.UtcNow, spell.PrimaryGroup.Cooldown));
        }

        if (!string.IsNullOrWhiteSpace(spell.SecondaryGroup.Name) && GroupExpired(spell.SecondaryGroup.Name))
        {
            SpellGroupCooldowns.Remove(spell.SecondaryGroup.Name);
            SpellGroupCooldowns.TryAdd(spell.SecondaryGroup.Name,
                new CooldownTime(DateTime.UtcNow, spell.SecondaryGroup.Cooldown));
        }

        return true;
    }

    public bool Start(ulong id, uint duration)
    {
        if (Expired(id)) CustomCooldowns.Remove(id);
        return CustomCooldowns.TryAdd(id, new CooldownTime(DateTime.UtcNow, duration));
    }

    public bool Start(IMonsterSummon summon)
    {
        SummonCooldowns ??= new Dictionary<string, CooldownTime>();

        if (Expired(summon)) SummonCooldowns.Remove(summon.Name);
        return SummonCooldowns.TryAdd(summon.Name, new CooldownTime(DateTime.UtcNow, summon.Interval));
    }

    public bool Expired(CooldownType type)
    {
        return !Cooldowns.TryGetValue(type, out var cooldown) || cooldown.Expired;
    }

    public bool Expired(IMonsterSummon summon)
    {
        SummonCooldowns ??= new Dictionary<string, CooldownTime>();

        return !SummonCooldowns.TryGetValue(summon.Name, out var cooldown) || cooldown.Expired;
    }


    public bool Expired(Guid id)
    {
        // Check individual spell cooldown first
        var spellExpired = !GuidCooldowns.TryGetValue(id, out var cooldown) || cooldown.Expired;
        return spellExpired;
    }

    public bool Expired(IHasCooldown spell)
    {
        // Check individual spell cooldown first
        var spellExpired = !GuidCooldowns.TryGetValue(spell.CooldownId, out var cooldown) || cooldown.Expired;
        if (!spellExpired) return false;

        if (spell.HasAnyCooldownGroup) return true;

        // Check all spell group cooldowns
        var groupsExpired = true;

        if (!string.IsNullOrWhiteSpace(spell.PrimaryGroup.Name) && GroupExpired(spell.PrimaryGroup.Name))
        {
            SpellGroupCooldowns.TryGetValue(spell.PrimaryGroup.Name, out var cooldownTime);
            groupsExpired &= cooldownTime.Expired;
        }

        if (!string.IsNullOrWhiteSpace(spell.SecondaryGroup.Name) && GroupExpired(spell.SecondaryGroup.Name))
        {
            SpellGroupCooldowns.TryGetValue(spell.SecondaryGroup.Name, out var cooldownTime);
            groupsExpired &= cooldownTime.Expired;
        }

        return groupsExpired;
    }

    public bool GroupExpired(string groupName)
    {
        if (SpellGroupCooldowns.TryGetValue(groupName, out var cooldown)) return cooldown.Expired;
        return true;
    }

    public bool Expired(ulong id)
    {
        if (CustomCooldowns.TryGetValue(id, out var cooldown)) return cooldown.Expired;
        return true;
    }

    public TimeSpan Remaining(CooldownType type)
    {
        if (Cooldowns.TryGetValue(type, out var cooldown)) return cooldown.Remaining;
        return TimeSpan.Zero;
    }

    public void Restart(CooldownType type, uint duration)
    {
        Cooldowns[type] = new CooldownTime(DateTime.UtcNow, duration);
    }
}