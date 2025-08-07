using NeoServer.Domain.Combat;

namespace NeoServer.Domain.Common;

public record GameConfiguration(
    decimal ExperienceRate = 1,
    decimal LootRate = 1,
    int LogoutBlockDuration = 60 * 1000,
    int ProtectionZoneBlockDuration = 60 * 1000,
    bool InfiniteRuneCharges = false,
    Dictionary<string, double> SkillsRate = null,
    DeathConfiguration Death = null,
    PvPConfiguration PvP = null,
    CombatConfiguration Combat = null,
    ReportConfiguration Report = null
);

public record CombatConfiguration(
    bool InfiniteAmmo = false,
    bool InfiniteThrowingWeapon = false,
    decimal AttackSpeedMultiplier = 1);

public record DeathConfiguration(
    bool IsDeathListEnabled = true,
    int DeathListRequiredTime = 60_000,
    int DeathAssistCount = 4,
    int MaxDeathRecords = 5);

public record PvPConfiguration(
    PvpType PvpType = PvpType.OpenPvP,
    bool SkullSystemEnabled = true,
    int DayKillsToRedSkull = 1,
    int DayKillsToBlackSkull = 2,
    int WeekKillsToRedSkull = 5,
    int WeekKillsToBlackSkull = 10,
    int MonthKillsToRedSkull = 10,
    int MonthKillsToBlackSkull = 20,
    int WhiteSkullDurationMinutes = 1,
    int RedSkullDurationDays = 30,
    int BlackSkullDurationDays = 45,
    int ProtectionLevel = 20
);

public record ReportConfiguration(uint ReportMaxTime = 60);