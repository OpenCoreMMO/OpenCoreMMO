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
    private Dictionary<string, CooldownTime> SpellGroupCooldowns { get; } =
        new(StringComparer.OrdinalIgnoreCase);
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

    public bool Start(IHasCooldown cooldown)
    {
        // Start new cooldown only if previous has expired
        if (!Expired(cooldown)) return false;

        var start = DateTime.UtcNow;
        GuidCooldowns[cooldown.CooldownId] = new CooldownTime(start, cooldown.Cooldown);

        StartGroupCooldown(cooldown.PrimaryGroup, start);
        StartGroupCooldown(cooldown.SecondaryGroup, start);

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

    public bool Expired(IHasCooldown cooldown)
    {
        // Check individual spell cooldown first
        if (!Expired(cooldown.CooldownId)) return false;
        if (!GroupExpired(cooldown.PrimaryGroup.Name)) return false;

        return GroupExpired(cooldown.SecondaryGroup.Name);
    }

    public bool GroupExpired(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName)) return true;
        if (SpellGroupCooldowns.TryGetValue(groupName, out var cooldown)) return cooldown.Expired;
        return true;
    }

    private void StartGroupCooldown((string Name, uint Cooldown) group, DateTime start)
    {
        if (string.IsNullOrWhiteSpace(group.Name)) return;

        SpellGroupCooldowns[group.Name] = new CooldownTime(start, group.Cooldown);
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
