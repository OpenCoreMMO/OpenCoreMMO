using System;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;

namespace NeoServer.Game.Combat.Services;

/// <summary>
/// Manage the player skull
/// </summary>
public class PlayerSkullService(GameConfiguration gameConfiguration) : IPlayerSkullService
{
    /// <summary>
    /// Update player skull based on attack
    /// </summary>
    /// <param name="aggressor"></param>
    /// <param name="victim"></param>
    public void UpdateSkullOnAttack(IPlayer aggressor, IPlayer victim)
    {
        if (!(gameConfiguration.PvP?.SkullSystemEnabled ?? false)) return;

        var whiteSkullEndingDate =
            DateTime.Now.AddMinutes(gameConfiguration.PvP?.WhiteSkullDurationMinutes ??
                                    TimeSpan.FromMilliseconds(gameConfiguration.LogoutBlockDuration).TotalMinutes);

        //when aggressor has white skull
        if (aggressor.HasSkull && aggressor.Skull is Skull.White)
        {
            if (aggressor.Skull is Skull.White) aggressor.SetSkull(Skull.White, whiteSkullEndingDate);
            return;
        }

        //when aggressor is yellow skull
        SetYellowSkullIfApplicable(aggressor, victim);

        //when both players don't have any skull
        if (aggressor.HasSkull is false && victim.HasSkull is false)
        {
            aggressor.SetSkull(Skull.White, whiteSkullEndingDate);
        }
    }

    public void SetYellowSkullIfApplicable(IPlayer aggressor, IPlayer victim)
    {
        if (aggressor.HasSkull || !victim.HasSkull) return;
        
        var logoutBlockDuration = gameConfiguration.LogoutBlockDuration;
        var damageRecord = aggressor.ReceivedDamages.GetCreatureDamage(victim);

        if (damageRecord is null)
        {
            aggressor.SetSkull(Skull.Yellow, DateTime.Now.AddMilliseconds(logoutBlockDuration), enemy: victim);
            return;
        }

        if (damageRecord.LastDamageTime >=
            DateTime.Now.Ticks - TimeSpan.FromMilliseconds(logoutBlockDuration).Ticks) return;

        aggressor.SetSkull(Skull.Yellow, DateTime.Now.AddMilliseconds(logoutBlockDuration), victim);
    }

    /// <summary>
    /// Updates the player's skull based on number of kills
    /// </summary>
    /// <param name="aggressor">The player who has engaged in PvP activity.</param>
    public void UpdatePlayerSkull(IPlayer aggressor)
    {
        // Check if the skull system is enabled in the game configuration
        if (!gameConfiguration.PvP?.SkullSystemEnabled ?? false) return;

        // If the player already has a black skull, no further action is needed
        if (aggressor.Skull is Skull.Black) return;

        // Get the PvP configuration
        var pvpConfiguration = gameConfiguration.PvP;

        // Calculate the ending dates for each skull type
        var backSkullEndingDate = DateTime.Now.AddDays(pvpConfiguration.BlackSkullDurationDays);
        var redSkullEndingDate = DateTime.Now.AddDays(pvpConfiguration.RedSkullDurationDays);
        var whiteSkullEndingDate = DateTime.Now.AddDays(pvpConfiguration.WhiteSkullDurationMinutes);

        // Check the player's unjustified kills and set their skull accordingly
        if (aggressor.NumberOfUnjustifiedKillsLastDay >= pvpConfiguration.DayKillsToBlackSkull)
        {
            // Set black skull if daily unjustified kills exceed the threshold
            aggressor.SetSkull(Skull.Black, backSkullEndingDate);
            return;
        }

        if (aggressor.NumberOfUnjustifiedKillsLastDay >= pvpConfiguration.DayKillsToRedSkull)
        {
            // Set red skull if daily unjustified kills exceed the threshold
            aggressor.SetSkull(Skull.Red, redSkullEndingDate);
            return;
        }

        // Repeat the checks for weekly and monthly unjustified kills
        if (aggressor.NumberOfUnjustifiedKillsLastWeek >= pvpConfiguration.WeekKillsToBlackSkull)
        {
            aggressor.SetSkull(Skull.Black, backSkullEndingDate);
            return;
        }

        if (aggressor.NumberOfUnjustifiedKillsLastWeek >= pvpConfiguration.WeekKillsToRedSkull)
        {
            aggressor.SetSkull(Skull.Red, redSkullEndingDate);
            return;
        }

        if (aggressor.NumberOfUnjustifiedKillsLastMonth >= pvpConfiguration.MonthKillsToBlackSkull)
        {
            aggressor.SetSkull(Skull.Black, backSkullEndingDate);
            return;
        }

        if (aggressor.NumberOfUnjustifiedKillsLastMonth >= pvpConfiguration.MonthKillsToRedSkull)
        {
            aggressor.SetSkull(Skull.Red, redSkullEndingDate);
            return;
        }

        // If the player has any unjustified kills, set their skull to white
        if (aggressor.NumberOfUnjustifiedKillsLastDay > 0)
        {
            aggressor.SetSkull(Skull.White, whiteSkullEndingDate);
        }
    }
}