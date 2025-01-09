using System.Collections.Generic;

namespace NeoServer.Game.Common;

public record GameConfiguration(
    decimal ExperienceRate = 1,
    decimal LootRate = 1,
    Dictionary<string, double> SkillsRate = null,
    DeathConfiguration Death = null,
    PvPConfiguration PvP = null
);

public record DeathConfiguration
{
    public required bool IsDeathListEnabled { get; init; }
    public required int DeathListRequiredTime { get; init; }
    public required int DeathAssistCount { get; init; }
    public required int MaxDeathRecords { get; init; }
}

public record PvPConfiguration(
    int DayKillsToRedSkull,
    int DayKillsToBlackSkull,
    int WeekKillsToRedSkull,
    int WeekKillsToBlackSkull,
    int MonthKillsToRedSkull,
    int MonthKillsToBlackSkull,
    int WhiteSkullDurationMinutes,
    int RedSkullDurationDays,
    int BlackSkullDurationDays,
    int OrangeSkullDurationDays
);