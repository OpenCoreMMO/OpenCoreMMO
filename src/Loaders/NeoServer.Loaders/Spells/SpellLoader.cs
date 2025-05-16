using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Loaders.Extensions;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.Spells;

public class SpellLoader(
    ServerConfiguration serverConfiguration,
    IVocationStore vocationStore,
    SpellListManager spellListManager,
    ILogger logger)
{
    public void Load()
    {
        LoadSpells();
    }

    private void LoadSpells()
    {
        logger.Step("Loading spells...", "{n} spells loaded", () =>
        {
            var path = Path.Combine(serverConfiguration.Data, "spells", "spells.json");
            var jsonString = File.ReadAllText(path);
            var spells = JsonSerializer.Deserialize<List<IDictionary<string, JsonElement>>>(jsonString)?.ToList() ??
                         [];
            var types = ScriptSearch.All.Where(x => typeof(ISpell).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface).ToList();

            foreach (var spellType in types)
            {
                if (spellType is null) continue;

                var spell = spells.FirstOrDefault(x => spellType.Name == x["script"].ToString());
                //if (spell is null) continue;

                if (CreateSpell(spellType) is not ISpell spellInstance) continue;

                if (spellInstance.Enabled is false) continue;

                spellInstance.Name ??= spell["name"].GetStringFromJson();
                spellInstance.Cooldown = spellInstance.Cooldown > 0 ? spellInstance.Cooldown : spell["cooldown"].GetUInt32FromJson();
                spellInstance.Mana = spellInstance.Mana > 0 ? spellInstance.Mana : spell["mana"].GetUInt16FromJson();
                spellInstance.MinLevel = spellInstance.MinLevel > 0 ? spellInstance.MinLevel : spell["level"].GetUInt16FromJson();
                spellInstance.VocationIds = (spellInstance.Vocations?.Length ?? 0) > 0 ? LoadVocations(spellInstance.Vocations) : LoadVocations(spell);
                spellListManager.Add(spellInstance.Words ?? spell["words"].GetStringFromJson(), spellInstance);
            }

            return [spells.Count];
        });
    }

    private byte[] LoadVocations(IDictionary<string, JsonElement> spell)
    {
        if (!spell.ContainsKey("vocations")) return null;

        var vocationArray = spell["vocations"].EnumerateArray();

        return vocationArray.Select(vocationJToken =>
        {
            var vocationValue = vocationJToken.GetStringFromJson();
            if (vocationValue is null) return (byte)0;

            if (byte.TryParse(vocationValue, out var vocation)) return vocation;

            return vocationStore.All.FirstOrDefault(x =>
                x.Name
                    .Replace(" ", string.Empty)
                    .Equals(vocationValue
                            .Replace(" ", string.Empty),
                        StringComparison.InvariantCultureIgnoreCase))?.VocationType ?? 0;
        }).ToArray();
    }
    
    private byte[] LoadVocations(string[] vocations)
    {
        if (vocations == null || vocations.Length == 0) return [];
    
        // Create a lookup dictionary for faster searching
        var vocationLookup = vocationStore.All.ToDictionary(
            x => x.Name.Replace(" ", string.Empty),
            x => x.VocationType,
            StringComparer.InvariantCultureIgnoreCase
        );

        return vocations.Select(vocation =>
        {
            var normalizedVocation = vocation.Replace(" ", string.Empty);
            return vocationLookup.TryGetValue(normalizedVocation, out byte vocationType) 
                ? vocationType 
                : (byte)0;
        }).ToArray();
    }

    private static object CreateSpell(Type type)
    {
        var constructorExpression = Expression.New(type);
        var lambdaExpression = Expression.Lambda<Func<object>>(constructorExpression);
        var createHeadersFunc = lambdaExpression.Compile();
        return createHeadersFunc();
    }
}