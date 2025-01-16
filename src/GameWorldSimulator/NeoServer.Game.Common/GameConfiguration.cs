using System.Collections.Generic;

namespace NeoServer.Game.Common;

public record GameConfiguration(
    decimal ExperienceRate = 1,
    decimal LootRate = 1,
    int LogoutBlockDuration = 60 * 1000,
    int ProtectionZoneBlockDuration = 60 * 1000,
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
    string PvpMode,
    bool SkullSystemEnabled,
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