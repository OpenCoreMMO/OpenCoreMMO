using Serilog;

namespace NeoServer.Modules.Combat.Attacks;

public sealed class AttackLibrary
{
    private readonly IEnumerable<IAttackStrategy> _attacks;
    private readonly ILogger _logger;

    public AttackLibrary(IEnumerable<IAttackStrategy> attacks, ILogger logger)
    {
        _attacks = attacks;
        _logger = logger;
        AddAttacksToMap();
    }

    private Dictionary<string, IAttackStrategy> AllAttacks { get; } = new();

    public IAttackStrategy Get(string name)
    {
        AllAttacks.TryGetValue(name, out var attack);
        return attack;
    }

    public void AddAttack(IAttackStrategy attackStrategy)
    {
        var succeed = AllAttacks.TryAdd(attackStrategy.Name, attackStrategy);
        if (succeed) return;

        _logger.Warning("{AttackName} is duplicated", attackStrategy.Name);
    }

    private void AddAttacksToMap()
    {
        var allAttacks = _attacks.ToList();

        foreach (var attack in allAttacks) AddAttack(attack);
    }
}