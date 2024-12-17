using System.Collections.Generic;

namespace NeoServer.Game.Common;

public record GameConfiguration(
    decimal ExperienceRate = 1,
    decimal LootRate = 1,
    Dictionary<string, double> SkillsRate = null
);