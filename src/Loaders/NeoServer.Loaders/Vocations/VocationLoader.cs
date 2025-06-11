using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures.Player.Vocation;
using NeoServer.Loaders.Converts;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using NeoServer.Server.Helpers.JsonConverters;
using Serilog;

namespace NeoServer.Loaders.Vocations;

public class VocationLoader
{
    public static VocationLoader Instance;

    private readonly ILogger _logger;
    private readonly ServerConfiguration _serverConfiguration;
    private readonly IVocationStore _vocationStore;
    private readonly GameConfiguration _gameConfiguration;

    public VocationLoader(ILogger logger,
        ServerConfiguration serverConfiguration, IVocationStore vocationStore, GameConfiguration gameConfiguration)
    {
        _logger = logger;
        _serverConfiguration = serverConfiguration;
        _vocationStore = vocationStore;
        _gameConfiguration = gameConfiguration;
        Instance = this;
    }

    public void Load()
    {
        _logger.Step("Loading vocations...", "{n} vocations loaded", () =>
        {
            _vocationStore.Clear();
            var vocations = GetVocations();

            foreach (var vocation in vocations)
            {
                vocation.AttackSpeed = (ushort)Math.Round(vocation.AttackSpeed / Math.Max(_gameConfiguration.Combat.AttackSpeedMultiplier, 1));
                _vocationStore.AddOrUpdate(vocation.VocationType, vocation);
            }

            return [vocations.Count];
        });
    }

    public void Reload()
    {
        _logger.Step("Reloading vocations...", "{n} vocations reloaded", () =>
        {
            var vocations = GetVocations();
            AddOrUpdateVocation(vocations);
            return new object[] { vocations.Count };
        });
    }

    private void AddOrUpdateVocation(List<VocationData> vocations)
    {
        foreach (var vocation in vocations)
        {
            if (_vocationStore.TryGetValue(vocation.VocationType, out var existingVocation))
            {
                UpdateVocation(existingVocation, vocation);

                continue;
            }

            _vocationStore.AddOrUpdate(vocation.VocationType, vocation);
        }
    }

    private void UpdateVocation(IVocation existingVocation, IVocation vocation)
    {
        existingVocation.Clientid = vocation.Clientid;
        existingVocation.Description = vocation.Description;

        UpdateFormula(existingVocation, vocation);

        existingVocation.Id = vocation.Id;
        existingVocation.Name = vocation.Name;

        UpdateSkills(existingVocation, vocation);

        existingVocation.AttackSpeed = (ushort) Math.Round(vocation.AttackSpeed / Math.Max(_gameConfiguration.Combat.AttackSpeedMultiplier, 1));
        existingVocation.BaseSpeed = vocation.BaseSpeed;
        existingVocation.FromVoc = vocation.FromVoc;
        existingVocation.GainCap = vocation.GainCap;
        existingVocation.GainHp = vocation.GainHp;
        existingVocation.GainMana = vocation.GainMana;
        existingVocation.GainHpAmount = vocation.GainHpAmount;
        existingVocation.GainHpTicks = vocation.GainHpTicks;
        existingVocation.GainManaAmount = vocation.GainManaAmount;
        existingVocation.GainManaTicks = vocation.GainManaTicks;
        existingVocation.GainSoulTicks = vocation.GainSoulTicks;
        existingVocation.SoulMax = vocation.SoulMax;
    }

    private static void UpdateFormula(IVocation existingVocation, IVocation vocation)
    {
        if (vocation.Formula is null) return;

        existingVocation.Formula ??= new VocationFormula();

        existingVocation.Formula.Armor = (float)vocation.Formula?.Armor;
        existingVocation.Formula.Defense = (float)vocation.Formula?.Defense;
        existingVocation.Formula.DistDamage = (float)vocation.Formula?.DistDamage;
        existingVocation.Formula.MeleeDamage = (float)vocation.Formula?.MeleeDamage;
    }

    private static void UpdateSkills(IVocation existingVocation, IVocation vocation)
    {
        if (vocation.Skills is null) return;

        existingVocation.Skills ??= new Dictionary<SkillType, float>();
        foreach (var (key, value) in vocation.Skills) existingVocation.Skills.AddOrUpdate(key, value);
    }

    private List<VocationData> GetVocations()
    {
        var basePath = $"{_serverConfiguration.Data}";
        var jsonString = File.ReadAllText(Path.Combine(basePath, "vocations.json"));
        var vocations = JsonSerializer.Deserialize<List<VocationData>>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new SkillConverter(),
                new AbstractConverter<VocationFormula, IVocationFormula>()
            }
        });

        return vocations;
    }
}