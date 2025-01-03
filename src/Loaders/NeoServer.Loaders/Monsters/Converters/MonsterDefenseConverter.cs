using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NeoServer.Game.Combat.Defenses;
using NeoServer.Game.Common.Contracts.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Helpers.Extensions;

namespace NeoServer.Loaders.Monsters.Converters;

public class MonsterDefenseConverter
{
    public static ICombatDefense[] Convert(MonsterData data, IMonsterDataManager monsters)
    {
        if (data.Defenses is null) return Array.Empty<ICombatDefense>();

        var defenses = new List<ICombatDefense>();

        foreach (var defense in data.Defenses)
        {
            var defenseName = defense.TryGetValue("name", out JsonElement element) ? element.GetString() : string.Empty;
            var chance = defense.TryGetValue("chance", out JsonElement chanceElement) ?  byte.Parse(chanceElement.GetString()!) : (byte)0;
            var interval = defense.TryGetValue("interval", out JsonElement intervalElement) ?  ushort.Parse(intervalElement.GetString()!) : (ushort)0;
            defense.TryGetValue("attributes", out JsonElement attributesElement);
            
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

            attributes.TryGetValue("areaEffect", out string areaEffect);

            if (defenseName!.Equals("healing", StringComparison.InvariantCultureIgnoreCase))
            {
                defense.TryGetValue("min", out decimal min);
                defense.TryGetValue("max", out decimal max);

                defenses.Add(new HealCombatDefense((int)Math.Abs(min), (int)Math.Abs(max),
                    MonsterAttributeParser.ParseAreaEffect(areaEffect))
                {
                    Chance = chance,
                    Interval = interval
                });
            }
            else if (defenseName.Equals("speed", StringComparison.InvariantCultureIgnoreCase))
            {
                defense.TryGetValue("speedchange", out ushort speed);
                defense.TryGetValue("duration", out uint duration);

                defenses.Add(new HasteCombatDefense(duration, speed,
                    MonsterAttributeParser.ParseAreaEffect(areaEffect))
                {
                    Chance = chance,
                    Interval = interval
                });
            }
            else if (defenseName.Equals("invisible", StringComparison.InvariantCultureIgnoreCase))
            {
                defense.TryGetValue("duration", out uint duration);

                defenses.Add(
                    new InvisibleCombatDefense(duration, MonsterAttributeParser.ParseAreaEffect(areaEffect))
                    {
                        Chance = chance,
                        Interval = interval
                    });
            }
            else if (defenseName.Equals("outfit", StringComparison.InvariantCultureIgnoreCase))
            {
                defense.TryGetValue("duration", out uint duration);
                defense.TryGetValue("monster", out string monsterName);

                defenses.Add(new IllusionCombatDefense(duration, monsterName,
                    MonsterAttributeParser.ParseAreaEffect(areaEffect), monsters)
                {
                    Chance = chance,
                    Interval = interval
                });
            }
            else
            {
                Console.WriteLine($"{defenseName} defense was not created on monster: {data.Name}");
            }
        }

        return defenses.ToArray();
    }
}