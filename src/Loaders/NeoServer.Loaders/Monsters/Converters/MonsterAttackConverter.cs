using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Combat.Attacks;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Effects.Parsers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Creatures.Condition;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.Monsters.Converters;

internal class MonsterAttackConverter
{
    private static HashSet<string> SupportedAttributes = new()
    {
        "name", "attack", "skill", "min", "max", "interval", "length", "radius", "target", "range", "spread", "chance",
        "attributes"
    };

    private static HashSet<string> _supportedAttackNames = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "lifedrain", "manadrain", "field", "firefield", "energyfield", "poisonField", "speed",
        "melee",
        "physical",
        "energy",
        "fire",
        "poison",
        "earth",
        "ice",
        "holy",
        "death"
    };

    private static HashSet<string> _fieldAttacks = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "field", "fireField", "poisonField", "energyField"
    };

    public static IMonsterCombatAttack[] Convert(MonsterData data, ILogger logger)
    {
        if (data.Attacks is null) return [];

        var attacks = new List<IMonsterCombatAttack>();

        AdjustAttackChanceValue(data.Attacks);

        foreach (var attack in data.Attacks)
        {
            
            attack.TryGetValue("name", out string attackName);
            attack.TryGetValue("attack", out ushort attackValue);
            attack.TryGetValue("skill", out int skill);
            attack.TryGetValue("min", out decimal min);
            attack.TryGetValue("max", out decimal max);
            attack.TryGetValue("interval", out ushort interval);
            attack.TryGetValue("length", out byte length);
            attack.TryGetValue("radius", out byte radius);
            attack.TryGetValue("target", out byte target);
            attack.TryGetValue("range", out byte range);
            attack.TryGetValue("spread", out byte spread);
            attack.TryGetValue("needTarget", out byte needTarget);

            if (attack.ContainsKey("needTarget"))
            {
                target = needTarget;
            }

            if (!_supportedAttackNames.Contains(attackName))
            {
                logger.Warning("{Monster} Attack: {AttackName} is not implemented", data.Name, attackName);
            }

            attack.TryGetValue("attributes", out JsonElement attributesElement);

            if (!attack.TryGetValue("chance", out byte chance)) chance = 100;

            var attributes = new Dictionary<string, object>();

            if (attributesElement.ValueKind == JsonValueKind.Array)
                attributes = attributesElement
                    .EnumerateArray()
                    .Select(item =>
                    {
                        var property = item.EnumerateObject().First();
                        return new KeyValuePair<string, object>(property.Name, property.Value.GetString());
                    })
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            attributes.TryGetValue("shootEffect", out string shootEffect);
            attributes.TryGetValue("areaEffect", out string areaEffect);

            var combatAttack = new MonsterCombatAttack
            {
                NeedTarget = target != 0,
                AttackChance = chance >= 100 ? (byte)100 : chance,
                Interval = interval
            };

            combatAttack.CombatParameter = new CombatParameter
            {
                MaxDamage = (ushort)Math.Abs(max),
                MinDamage = (ushort)Math.Abs(min),
                DamageType = DamageTypeParser.Parse(attackName),
                CooldownId = combatAttack.Id,
                Effect = EffectParser.Parse(areaEffect),
                Range = range,
                Spread = spread,
                Length = length,
                Radius = radius,
                ShootType = ShootTypeParser.Parse(shootEffect)
            };

            if (combatAttack.CombatParameter.DamageType is DamageType.Melee)
            {
                combatAttack.CombatParameter.MinDamage = (ushort)Math.Abs(min);
                combatAttack.CombatParameter.MaxDamage = Math.Abs(max) > 0
                    ? (ushort)Math.Abs(max)
                    : MeleeCombatAttack.CalculateMaxDamage(skill, attackValue);

                if (attack.TryGetValue("fire", out ushort value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Fire, 9000);
                }

                if (attack.TryGetValue("poison", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Poison, 4000);
                }

                if (attack.TryGetValue("energy", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Energy, 10_000);
                }

                if (attack.TryGetValue("drown", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Drown, 5_000);
                }

                if (attack.TryGetValue("freeze", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Freezing, 8_000);
                }

                if (attack.TryGetValue("dazzle", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Dazzled, 10000);
                }

                if (attack.TryGetValue("curse", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Cursed, 4000);
                }

                if (attack.TryGetValue("bleed", out value) || attack.TryGetValue("physical", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Bleeding, 4000);
                }

                if (attack.TryGetValue("tick", out ushort tick) &&
                    combatAttack.CombatParameter.DamageType == DamageType.Melee)
                    combatAttack.CombatParameter.Condition.Duration = tick;
            }

            if (range > 1 || radius == 1)
            {
                if (areaEffect != null)
                {
                    var damageType = DamageTypeParser.Parse(areaEffect);

                    combatAttack.CombatParameter.DamageType = damageType == DamageType.Melee
                        ? combatAttack.CombatParameter.DamageType
                        : damageType;
                }
            }

            if (radius > 1)
            {
                combatAttack.CombatParameter.DamageType = DamageTypeParser.Parse(areaEffect);
            }

            if (length > 0)
            {
                combatAttack.CombatParameter.DamageType = DamageTypeParser.Parse(areaEffect);
            }

            if (attackName is "lifedrain" or "manadrain")
            {
                combatAttack.CombatParameter.DamageType =
                    attackName is "lifedrain" ? DamageType.LifeDrain : DamageType.ManaDrain;
            }

            if (attackName == "speed")
            {
                attack.TryGetValue("duration", out int duration);
                attack.TryGetValue("speedchange", out short speedChange);

                combatAttack.CombatParameter.Condition =
                    new CombatParameter.AttackCondition(ConditionType.Paralyze, (uint)Math.Abs(duration))
                    {
                        Value = speedChange
                    };

                combatAttack.CombatParameter.DamageType = DamageType.None;
                combatAttack.CombatParameter.Effect = EffectParser.Parse(areaEffect);
            }


            if (_fieldAttacks.Contains(attackName))
            {
                combatAttack.CombatParameter.FieldAttack = true;

                attack.TryGetValue("damageType", out string damageType);

                damageType = string.IsNullOrEmpty(damageType)
                    ? attackName.Replace("field", string.Empty)
                    : damageType;

                combatAttack.CombatParameter.DamageType = DamageTypeParser.Parse(damageType);
            }

            attacks.Add(combatAttack);
        }

        return attacks.ToArray();
    }

    private static void AdjustAttackChanceValue(List<Dictionary<string, object>> attacks)
    {
        (bool, int) GetChance(Dictionary<string, object> attack)
        {
            if (!attack.TryGetValue("chance", out string chance)) return (true, 100);

            return !int.TryParse(chance, out var value) ? (false, default) : (true, value);
        }

        var maxChance = 0;
        foreach (var attack in attacks)
        {
            var (hasChance, chance) = GetChance(attack);
            if (!hasChance) continue;

            maxChance = chance > maxChance ? chance : maxChance;
        }

        if (maxChance == 100) return;

        foreach (var attack in attacks)
        {
            var (hasChance, chance) = GetChance(attack);
            if (!hasChance) continue;

            attack["chance"] = Math.Round(chance * 100d / maxChance).ToString(CultureInfo.InvariantCulture);
        }
    }

    private static decimal ParseDecimalSafely(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0m;

        var sanitizedInput = input.Replace("--", "-").Trim();

        return decimal.TryParse(sanitizedInput, out var result) ? result : 0m;
    }
}