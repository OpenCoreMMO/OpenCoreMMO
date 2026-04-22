using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Effects.Parsers;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ConditionDamage : BaseCondition
{
    private CooldownTime _cooldown;
    private Queue<ushort> _damageQueue;
    private ushort _maxDamage;
    private ushort _minDamage;

    public ConditionDamage(
        IThing cause,
        ConditionType type,
        uint interval,
        ushort minDamage,
        ushort maxDamage,
        EffectT effect = EffectT.None) : base(0)
    {
        Cause = cause;
        Type = type;
        Interval = interval;
        DamageType = type.ToDamageType();
        _maxDamage = maxDamage;
        _minDamage = minDamage;
        Effect = effect;
    }

    public ConditionDamage(IThing cause, ConditionType type, uint interval, byte amount, ushort damage,
        EffectT effect = EffectT.None) : base(0)
    {
        if (amount == 0) return;

        Cause = cause;
        Type = type;
        Interval = interval;
        DamageType = type.ToDamageType();
        _maxDamage = damage;
        _minDamage = damage;
        Effect = effect;
        Amount = amount;
    }

    public IThing Cause { get; }
    public byte Amount { get; }
    public override ConditionType Type { get; }
    public DamageType DamageType { get; set; }
    public EffectT Effect { get; }

    public uint Interval
    {
        set => _cooldown = new CooldownTime(DateTime.UtcNow, value);
    }

    public override bool HasExpired => _damageQueue.Count <= 0;

    public void Execute(ICombatActor creature)
    {
        if (!_cooldown.Expired) return;

        _cooldown.Reset();
        if (!_damageQueue.TryDequeue(out var damage))
        {
            End();
            return;
        }

        creature.TakeDamage(Cause, new CombatDamage(damage, DamageType, DamageEffectParser.Parse(DamageType)));
    }

    public bool Start(ICreature creature, ushort minDamage, ushort maxDamage)
    {
        if (maxDamage < _maxDamage) return false;

        _minDamage = minDamage;
        _maxDamage = maxDamage;
        
        //End any existing conditions of this type
        if (creature is ICombatActor combatActor)
        {
            combatActor.Conditions.EndConditions(Type);
        }

        Start(creature);
        return true;
    }

    public override bool Start(ICreature creature)
    {
        if (Amount == 0)
            GenerateDamageList();
        else
            GenerateDamageList(Amount);

        base.Start(creature);
        return true;
    }

    public bool Restart(byte amount)
    {
        GenerateDamageList(amount);

        return true;
    }

    private int GetStartDamage(int maxDamage, int amount)
    {
        var startDamage = 0;
        if (startDamage > maxDamage)
        {
            startDamage = maxDamage;
            return startDamage;
        }

        return (int)Math.Max(1, Math.Ceiling(amount / 20.0));
    }

    private void GenerateDamageList(byte amount)
    {
        _damageQueue ??= new Queue<ushort>();
        for (var i = 0; i < amount - _damageQueue.Count; i++) _damageQueue.Enqueue(_maxDamage);
    }

    private void GenerateDamageList()
    {
        _damageQueue ??= new Queue<ushort>();

        int amount = (ushort)GameRandom.Random.Next(_minDamage, maxValue: _maxDamage);
        var start = GetStartDamage(_maxDamage, amount);

        _damageQueue.Clear();

        amount = Math.Abs(amount);
        var sum = 0;
        double x1, x2;

        for (var i = start; i > 0; --i)
        {
            var n = start + 1 - i;
            var med = n * amount / start;

            do
            {
                sum += i;
                _damageQueue.Enqueue((ushort)i);

                x1 = Math.Abs(1.0 - ((float)sum + i) / med);
                x2 = Math.Abs(1.0 - (float)sum / med);
            } while (x1 < x2);
        }
    }
}