using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures.Player.Vocation;
using NeoServer.Loaders.Converts;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.Vocations;

public class VocationLoader(
    ILogger logger,
    ServerConfiguration serverConfiguration,
    IVocationStore vocationStore,
    GameConfiguration gameConfiguration)
{
    public void Load()
    {
        logger.Step("Loading vocations...", "{n} vocations loaded", () =>
        {
            vocationStore.Clear();
            var vocations = GetVocations();

            foreach (var vocation in vocations)
            {
                vocation.AttackSpeed = (ushort)Math.Round(vocation.AttackSpeed /
                                                          Math.Max(gameConfiguration.Combat.AttackSpeedMultiplier, 1));
                vocationStore.AddOrUpdate(vocation.VocationType, vocation);
            }

            return [vocations.Count];
        });
    }

    public void Reload()
    {
        logger.Step("Reloading vocations...", "{n} vocations reloaded", () =>
        {
            var vocations = GetVocations();
            AddOrUpdateVocation(vocations);
            return [vocations.Count];
        });
    }

    private void AddOrUpdateVocation(List<Vocation> vocations)
    {
        foreach (var vocation in vocations)
        {
            if (vocationStore.TryGetValue(vocation.VocationType, out var existingVocation))
            {
                UpdateVocation(existingVocation, vocation);

                continue;
            }

            vocationStore.AddOrUpdate(vocation.VocationType, vocation);
        }
    }

    private void UpdateVocation(Vocation existingVocation, Vocation vocation)
    {
        existingVocation.Clientid = vocation.Clientid;
        existingVocation.Description = vocation.Description;

        UpdateFormula(existingVocation, vocation);

        existingVocation.Id = vocation.Id;
        existingVocation.Name = vocation.Name;

        UpdateSkills(existingVocation, vocation);

        existingVocation.AttackSpeed =
            (ushort)Math.Round(vocation.AttackSpeed / Math.Max(gameConfiguration.Combat.AttackSpeedMultiplier, 1));
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

    private static void UpdateFormula(Vocation existingVocation, Vocation vocation)
    {
        if (vocation.Formula is null) return;

        existingVocation.Formula ??= new VocationFormula();

        existingVocation.Formula.Armor = (float)vocation.Formula?.Armor;
        existingVocation.Formula.Defense = (float)vocation.Formula?.Defense;
        existingVocation.Formula.DistDamage = (float)vocation.Formula?.DistDamage;
        existingVocation.Formula.MeleeDamage = (float)vocation.Formula?.MeleeDamage;
    }

    private static void UpdateSkills(Vocation existingVocation, Vocation vocation)
    {
        if (vocation.Skills is null) return;

        existingVocation.Skills ??= new Dictionary<SkillType, float>();
        foreach (var (key, value) in vocation.Skills) existingVocation.Skills.AddOrUpdate(key, value);
    }

    private List<Vocation> GetVocations()
    {
        var basePath = $"{serverConfiguration.Data}";
        var jsonString = File.ReadAllText(Path.Combine(basePath, "vocations.json"));
        var vocations = JsonSerializer.Deserialize<List<VocationData>>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new SkillConverter()
            }
        });

        return vocations.Select(x => new Vocation
        {
            FromVoc = x.FromVoc,
            GainCap = x.GainCap,
            GainHp = x.GainHp,
            GainHpAmount = x.GainHpAmount,
            GainHpTicks = x.GainHpTicks,
            GainMana = x.GainMana,
            GainManaAmount = x.GainManaAmount,
            GainManaTicks = x.GainManaTicks,
            GainSoulTicks = x.GainSoulTicks,
            Id = x.Id,
            Inspect = x.Inspect,
            Name = x.Name,
            Description = x.Description,
            SoulMax = x.SoulMax,
            AttackSpeed = x.AttackSpeed,
            BaseSpeed = x.BaseSpeed,
            Clientid = x.Clientid,
            Formula = x.Formula,
            Skills = x.Skills
        }).ToList();
    }
}