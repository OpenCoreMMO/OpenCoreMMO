using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using NeoServer.Game.Combat.Attacks;
using NeoServer.Game.Common.Contracts.Combat.Attacks;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Parsers;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.Monsters.Converters;

internal class MonsterAttackConverter
{
    public static IMonsterCombatAttack[] Convert(MonsterData data, ILogger logger)
    {
        if (data.Attacks is null) return Array.Empty<IMonsterCombatAttack>();

        var attacks = new List<IMonsterCombatAttack>();

        AdjustAttackChanceValue(data.Attacks);
        
        foreach (var attack in data.Attacks)
        {
            var attackName = attack.TryGetValue("name", out JsonElement attackNameElement) ? attackNameElement.GetString() : string.Empty;
            var attackValue  = attack.TryGetValue("attack", out JsonElement attackValueElement) ? ushort.Parse(attackValueElement.GetString())  : (ushort)0;
            var skill = attack.TryGetValue("skill", out JsonElement skillElement) ? ushort.Parse(skillElement.GetString()) : (ushort)0;
            var min = attack.TryGetValue("min", out JsonElement minElement) ?  ParseDecimalSafely(minElement.GetString()) : 0m;
            var max = attack.TryGetValue("max", out JsonElement maxElement) ?  ParseDecimalSafely(maxElement.GetString()): 0m;
            var interval = attack.TryGetValue("interval", out JsonElement intervalElement) ? ushort.Parse(intervalElement.GetString()): (ushort)0;
            var length = attack.TryGetValue("length", out JsonElement lengthElement) ? byte.Parse(lengthElement.GetString()): (byte)0;
            var radius = attack.TryGetValue("radius", out JsonElement radiusElement) ? byte.Parse(radiusElement.GetString()): (byte)0;
            var target = attack.TryGetValue("target", out JsonElement targetElement) ? byte.Parse(targetElement.GetString()): (byte)0;
            var range  = attack.TryGetValue("range", out JsonElement rangeElement) ? byte.Parse(rangeElement.GetString()): (byte)0;
            var spread  = attack.TryGetValue("spread", out JsonElement spreadElement) ? byte.Parse(spreadElement.GetString()): (byte)0;

            attack.TryGetValue("attributes", out JsonElement attributesElement);

            if (!attack.TryGetValue("chance", out byte chance)) chance = 100;

            var attributes = new Dictionary<string, object>();
            
            if (attributesElement.ValueKind == JsonValueKind.Array)
            {
                attributes = attributesElement
                    .EnumerateArray()
                    .Select(item =>
                    {
                        var property = item.EnumerateObject().First();
                        return new KeyValuePair<string, object>(property.Name, property.Value.GetString());
                    })
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            attributes.TryGetValue("shootEffect", out string shootEffect);
            attributes.TryGetValue("areaEffect", out string areaEffect);

            var combatAttack = new MonsterCombatAttack
            {
                Chance = chance >= 100 ? (byte)100 : chance,
                Interval = interval,
                MaxDamage = (ushort)Math.Abs(max),
                MinDamage = (ushort)Math.Abs(min),
                Target = target,
                DamageType = DamageTypeParser.Parse(attackName)
            };

            if (combatAttack.IsMelee)
            {
                combatAttack.MinDamage = (ushort)Math.Abs(min);
                combatAttack.MaxDamage = Math.Abs(max) > 0
                    ? (ushort)Math.Abs(max)
                    : MeleeCombatAttack.CalculateMaxDamage(skill, attackValue);

                combatAttack.CombatAttack = new MeleeCombatAttack();

                if (attack.TryGetValue("fire", out ushort value))
                    combatAttack.CombatAttack = new MeleeCombatAttack(value, value, ConditionType.Fire, 9000);
                else if (attack.TryGetValue("poison", out value))
                    combatAttack.CombatAttack = new MeleeCombatAttack(value, value, ConditionType.Poison, 4000);
                else if (attack.TryGetValue("energy", out value))
                    combatAttack.CombatAttack = new MeleeCombatAttack(value, value, ConditionType.Energy, 10000);
                else if (attack.TryGetValue("drown", out value))
                    combatAttack.CombatAttack = new MeleeCombatAttack(value, value, ConditionType.Drown, 5000);
                else if (attack.TryGetValue("freeze", out value))
                    combatAttack.CombatAttack = new MeleeCombatAttack(value, value, ConditionType.Freezing, 8000);
                else if (attack.TryGetValue("dazzle", out value))
                    combatAttack.CombatAttack = new MeleeCombatAttack(value, value, ConditionType.Dazzled, 10000);
                else if (attack.TryGetValue("curse", out value))
                    combatAttack.CombatAttack = new MeleeCombatAttack(value, value, ConditionType.Cursed, 4000);
                else if (attack.TryGetValue("bleed", out value) || attack.TryGetValue("physical", out value))
                    combatAttack.CombatAttack = new MeleeCombatAttack(value, value, ConditionType.Bleeding, 4000);

                if (attack.TryGetValue("tick", out ushort tick) &&
                    combatAttack.CombatAttack is MeleeCombatAttack melee) melee.ConditionInterval = tick;
            }

            if (range > 1 || radius == 1)
            {
                if (areaEffect != null)
                {
                    var damageType = DamageTypeParser.Parse(areaEffect);

                    combatAttack.DamageType = damageType == DamageType.Melee ? combatAttack.DamageType : damageType;
                }

                combatAttack.CombatAttack = new DistanceCombatAttack(range, ShootTypeParser.Parse(shootEffect));
            }

            if (radius > 1)
            {
                combatAttack.DamageType = DamageTypeParser.Parse(areaEffect);
                combatAttack.CombatAttack =
                    new DistanceAreaCombatAttack(range, radius, ShootTypeParser.Parse(shootEffect));
            }

            if (length > 0)
            {
                combatAttack.DamageType = DamageTypeParser.Parse(areaEffect);
                combatAttack.CombatAttack = new SpreadCombatAttack(length, spread);
            }

            if (attackName == "lifedrain")
            {
                var shootType = ShootTypeParser.Parse(shootEffect);

                combatAttack.CombatAttack = new DrainCombatAttack(range, radius, shootType);
            }

            if (attackName == "manadrain")
            {
                var shootType = ShootTypeParser.Parse(shootEffect);

                combatAttack.CombatAttack = new DrainCombatAttack(range, radius, shootType);
            }

            if (attackName == "speed")
            {
                attack.TryGetValue("duration", out uint duration);
                attack.TryGetValue("speedchange", out short speedChange);

                combatAttack.DamageType = default;
                combatAttack.CombatAttack = new SpeedCombatAttack(duration, speedChange, range,
                    ShootTypeParser.Parse(shootEffect));
            }

            if (combatAttack.CombatAttack is null)
                logger.Warning("{attackName} attack was not created on monster: {name}", attackName, data.Name);

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
    
    static decimal ParseDecimalSafely(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0m;

        var sanitizedInput = input.Replace("--", "-").Trim();

        return decimal.TryParse(sanitizedInput, out var result) ? result : 0m;
    }
}