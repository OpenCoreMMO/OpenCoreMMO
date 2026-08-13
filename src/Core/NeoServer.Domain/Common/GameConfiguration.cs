using NeoServer.Domain.Combat;

namespace NeoServer.Domain.Common;

public record GameConfiguration(
    decimal ExperienceRate = 1,
    decimal LootRate = 1,
    int LogoutBlockDuration = 60 * 1000,
    int ProtectionZoneBlockDuration = 60 * 1000,
    int LogoutCooldownSeconds = 0,
    bool InfiniteRuneCharges = false,
    bool RemovePotionCharges = true,
    bool StaminaEnabled = true,
    Dictionary<string, double> SkillsRate = null,
    DeathConfiguration Death = null,
    PvPConfiguration PvP = null,
    CombatConfiguration Combat = null,
    ReportConfiguration Report = null,
    YellConfiguration Yell = null,
    HouseConfiguration House = null
);

public record CombatConfiguration(
    bool InfiniteAmmo = false,
    bool InfiniteThrowingWeapon = false,
    bool CanAttackOwnSummon = false,
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

public record YellConfiguration(int YellCooldownSeconds = 30, int YellMinimumLevel = 2, bool YellAllowedPremium = true);

public record ReportConfiguration(uint ReportMaxTime = 60);

public record HouseConfiguration(
    bool TransferItemsToDepotOnOwnershipChange = true,
    bool RequirePremiumAccount = true,
    bool RequirePremiumForSubOwners = true,
    int MaxAccessListLength = 1999,
    int MaxAccessListLines = 100,
    int MaxSubOwnerCount = 10,
    int PricePerSqm = 1000,
    string RentPeriod = "monthly"
)
{
    /// <summary>
    ///     Seconds in one rent period. daily/weekly/monthly/yearly match the usual
    ///     house rent cycle; any other value (including "never") is 0 and skips PaidUntil updates.
    /// </summary>
    public uint RentPeriodSeconds
    {
        get
        {
            if (string.IsNullOrEmpty(RentPeriod))
            {
                return 0;
            }

            return RentPeriod.ToLowerInvariant() switch
            {
                "daily" => 24 * 60 * 60,
                "weekly" => 24 * 60 * 60 * 7,
                "monthly" => 24 * 60 * 60 * 30,
                "yearly" => 24 * 60 * 60 * 365,
                _ => 0
            };
        }
    }
}