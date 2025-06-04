using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Loaders.Monsters.Converters;

public static class MonsterResistanceConverter
{
    public static IDictionary<DamageType, sbyte> Convert(MonsterData data)
    {
        if (data.Elements is null) return new Dictionary<DamageType, sbyte>(0);

        var immunities = new Dictionary<DamageType, sbyte>(data.Elements.Count);

        foreach (var element in data.Elements)
        {
            var immunity = element.Key.ToLowerInvariant() switch
            {
                var key when key.Contains("energy") => DamageType.Energy,
                var key when key.Contains("energypercentage") => DamageType.Energy,
                var key when key.Contains("holy") => DamageType.Holy,
                var key when key.Contains("holypercentage") => DamageType.Holy,
                var key when key.Contains("earth") => DamageType.Earth,
                var key when key.Contains("earthpercentage") => DamageType.Earth,
                var key when key.Contains("death") => DamageType.Death,
                var key when key.Contains("deathpercentage") => DamageType.Death,
                var key when key.Contains("fire") => DamageType.Fire,
                var key when key.Contains("firepercent") => DamageType.Fire,
                var key when key.Contains("ice") => DamageType.Ice,
                var key when key.Contains("icepercent") => DamageType.Ice,
                var key when key.Contains("drown") => DamageType.Drown,
                var key when key.Contains("drownpercent") => DamageType.Drown,
                var key when key.Contains("lifedrain") => DamageType.LifeDrain,
                var key when key.Contains("lifedrainpercent") => DamageType.LifeDrain,
                var key when key.Contains("manadrain") => DamageType.ManaDrain,
                var key when key.Contains("manadrainpercent") => DamageType.ManaDrain,
                var key when key.Contains("physical") => DamageType.Physical,
                var key when key.Contains("physicalpercent") => DamageType.Physical,
                _ => DamageType.None
            };

            if (immunity == DamageType.None) Console.WriteLine($"{element.Key} not handled for monster {data.Name}");

            immunity = immunity == DamageType.None ? DamageType.Melee : immunity;

            immunities.TryAdd(immunity, element.Value);
        }

        return immunities;
    }
}