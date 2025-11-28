using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Effects.Parsers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Creatures.Conditions;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Spells;
using NeoServer.Server.Helpers.Extensions;

namespace NeoServer.Loaders.Monsters.Converters;

public class MonsterAttackConverter(SpellListManager spellListManager)
{
    private static readonly HashSet<string> SupportedAttackNames = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "lifedrain", 
        "manadrain",
        "melee",
        "physical",
        "energy",
        "fire",
        "poison",
        "earth",
        "ice",
        "holy",
        "drown",
        "death",
        "effect"
    };

    public MonsterCombatType[] Convert(MonsterData data)
    {
        if (data.Attacks is null) return [];

        var attacks = new List<MonsterCombatType>();

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
            attack.TryGetValue("duration", out int duration);

            if (attack.ContainsKey("needTarget")) target = needTarget;

            if (!attack.ContainsKey("target") &&
                !attack.ContainsKey("needTarget") && radius == 0 && length == 0 && spread == 0)
                target = 1; // Default to no target if not specified

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

            var combatAttack = new MonsterCombatType
            {
                NeedTarget = target != 0,
                AttackChance = Math.Min(chance, (byte)100),
                Interval = interval
            };

            combatAttack.CombatParameter = new CombatParameter
            {
                MaxDamage = (ushort)Math.Abs(max),
                MinDamage = (ushort)Math.Abs(min),
                DamageType = DamageTypeParser.Parse(attackName),
                CooldownId = combatAttack.Id,
                Effect = string.IsNullOrWhiteSpace(areaEffect)
                    ? EffectParser.Parse(attackName)
                    : EffectParser.Parse(areaEffect),
                Range = range,
                Spread = spread,
                Length = length,
                Radius = radius,
                ShootType = ShootTypeParser.Parse(shootEffect)
            };

            if (attack.TryGetValue("damageType", out string damageTypeText))
                combatAttack.CombatParameter.DamageType = DamageTypeParser.Parse(damageTypeText);

            if (attack.TryGetValue("effect", out string effect))
                combatAttack.CombatParameter.Effect = EffectParser.Parse(effect);

            if (attackName.Equals("melee", StringComparison.InvariantCultureIgnoreCase))
            {
                combatAttack.CombatParameter.MinDamage = (ushort)Math.Abs(min);
                combatAttack.CombatParameter.MaxDamage = Math.Abs(max) > 0
                    ? (ushort)Math.Abs(max)
                    : (ushort)Math.Ceiling(skill * (attackValue * 0.05) + attackValue * 0.5);


                if (attack.TryGetValue("fire", out ushort value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Burning,
                            ConditionIntervalMap.Get(ConditionType.Burning));
                }

                if (attack.TryGetValue("poison", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Poisoned,
                            ConditionIntervalMap.Get(ConditionType.Poisoned));
                }

                if (attack.TryGetValue("energy", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Electrified,
                            ConditionIntervalMap.Get(ConditionType.Electrified));
                }

                if (attack.TryGetValue("drown", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Drowning,
                            ConditionIntervalMap.Get(ConditionType.Drowning));
                }

                if (attack.TryGetValue("freeze", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Freezing,
                            ConditionIntervalMap.Get(ConditionType.Freezing));
                }

                if (attack.TryGetValue("dazzle", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Dazzled,
                            ConditionIntervalMap.Get(ConditionType.Dazzled));
                }

                if (attack.TryGetValue("curse", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Cursed,
                            ConditionIntervalMap.Get(ConditionType.Cursed));
                }

                if (attack.TryGetValue("bleed", out value) || attack.TryGetValue("physical", out value))
                {
                    combatAttack.CombatParameter.SetMinMaxDamage(new MinMax(value, value));
                    combatAttack.CombatParameter.Condition =
                        new CombatParameter.AttackCondition(ConditionType.Bleeding,
                            ConditionIntervalMap.Get(ConditionType.Bleeding));
                }

                if (attack.TryGetValue("tick", out ushort tick) &&
                    combatAttack.CombatParameter.DamageType == DamageType.Melee)
                    combatAttack.CombatParameter.Condition.Duration = tick;
            }

            if (attackName.Equals("lifeDrain", StringComparison.InvariantCultureIgnoreCase))
            {
                combatAttack.CombatParameter.DamageType = DamageType.LifeDrain;
                combatAttack.CombatParameter.Effect = EffectT.GlitterRed;
            }

            if (attackName.Equals("manaDrain", StringComparison.InvariantCultureIgnoreCase))
            {
                combatAttack.CombatParameter.DamageType = DamageType.ManaDrain;
                combatAttack.CombatParameter.Effect = EffectT.GlitterRed;
            }

            if (attackName == "speed")
            {
                attack.TryGetValue("speedchange", out short speedChange);

                combatAttack.CombatParameter.Condition =
                    new CombatParameter.AttackCondition(ConditionType.Paralyze, (uint)Math.Abs(duration))
                    {
                        Value = speedChange
                    };

                combatAttack.CombatParameter.DamageType = DamageType.None;
                combatAttack.CombatParameter.Effect = EffectParser.Parse(areaEffect);

                SupportedAttackNames.Add(attackName);
            }

            if (attackName.Equals("drunk", StringComparison.InvariantCultureIgnoreCase))
            {
                combatAttack.CombatParameter.Condition =
                    new CombatParameter.AttackCondition(ConditionType.Drunk,
                        (uint)Math.Abs(duration == 0 ? 10000 : duration));

                SupportedAttackNames.Add(attackName);
            }

            if (attackName.Contains("field"))
            {
                combatAttack.CombatParameter.FieldAttack = true;

                var damageType = string.IsNullOrEmpty(damageTypeText)
                    ? attackName.Replace("field", string.Empty)
                    : damageTypeText;

                combatAttack.CombatParameter.DamageType = DamageTypeParser.Parse(damageType);

                SupportedAttackNames.Add(attackName);
            }

            if (attackName.Contains("condition"))
            {
                var conditionType = string.IsNullOrEmpty(damageTypeText)
                    ? attackName.Replace("condition", string.Empty)
                    : damageTypeText;

                var condition = ConditionTypeParser.Parse(conditionType);

                combatAttack.CombatParameter.Condition =
                    new CombatParameter.AttackCondition(condition,
                        duration == 0 ? ConditionIntervalMap.Get(condition) : (uint)duration);

                SupportedAttackNames.Add(attackName);
            }

            if (attackName.Equals("outfit", StringComparison.InvariantCultureIgnoreCase))
            {
                attack.TryGetValue("monster", out string monsterName);

                combatAttack.CombatParameter.Condition =
                    new CombatParameter.AttackCondition(ConditionType.Outfit,
                        duration == 0 ? 5000 : (uint)duration)
                    {
                        Value = monsterName
                    };

                SupportedAttackNames.Add(attackName);
            }

            var spell = spellListManager.GetByName(attackName);
            combatAttack.Spell = spell;

            attacks.Add(combatAttack);

            if (spell is not null) SupportedAttackNames.Add(attackName);

            //if (!SupportedAttackNames.Contains(attackName))
            //    logger.Warning("{Monster} Attack: {AttackName} is not implemented", data.Name, attackName);
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

        if (maxChance <= 100) return;

        foreach (var attack in attacks)
        {
            var (hasChance, chance) = GetChance(attack);
            if (!hasChance) continue;

            attack["chance"] = Math.Round(chance * 100d / maxChance).ToString(CultureInfo.InvariantCulture);
        }
    }
}