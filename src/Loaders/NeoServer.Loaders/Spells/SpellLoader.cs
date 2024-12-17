using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;
using System.Text.Json;
using NeoServer.Loaders.Extensions;

namespace NeoServer.Loaders.Spells;

public class SpellLoader
{
    private readonly IVocationStore _vocationStore;
    private readonly ILogger logger;
    private readonly ServerConfiguration serverConfiguration;

    public SpellLoader(ServerConfiguration serverConfiguration,
        IVocationStore vocationStore,
        ILogger logger)
    {
        this.serverConfiguration = serverConfiguration;
        _vocationStore = vocationStore;
        this.logger = logger;
    }

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
            var spells =  JsonSerializer.Deserialize<List<IDictionary<string, JsonElement>>>(jsonString)?.ToList() ??
                          [];
            var types = ScriptSearch.All.Where(x => typeof(ISpell).IsAssignableFrom(x)).ToList();

            foreach (var spell in spells)
            {
                if (spell is null) continue;

                var type = types.FirstOrDefault(x => x.Name == spell["script"].ToString());
                if (type is null) continue;

                if (CreateSpell(type) is not ISpell spellInstance) continue;

                spellInstance.Name = spell["name"].GetStringFromJson();
                spellInstance.Cooldown = spell["cooldown"].GetUInt32FromJson();
                spellInstance.Mana = spell["mana"].GetUInt16FromJson();
                spellInstance.MinLevel = spell["level"].GetUInt16FromJson();
                spellInstance.Vocations = LoadVocations(spell);
                SpellList.Add(spell["words"].GetStringFromJson(), spellInstance);
            }

            return new object[] { spells.Count };
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

            return _vocationStore.All.FirstOrDefault(x =>
                x.Name
                    .Replace(" ", string.Empty)
                    .Equals(vocationValue
                            .Replace(" ", string.Empty),
                        StringComparison.InvariantCultureIgnoreCase))?.VocationType ?? 0;
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