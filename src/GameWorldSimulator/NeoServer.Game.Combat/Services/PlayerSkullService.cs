using System;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;

namespace NeoServer.Game.Combat.Services;

public class PlayerSkullService(GameConfiguration gameConfiguration) : IPlayerSkullService
{
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
        if (aggressor.GetSkull(victim) is Skull.Yellow)
        {
            aggressor.SetSkull(Skull.Yellow);
            return;
        }

        //when both players don't have any skull
        if (aggressor.HasSkull is false && victim.HasSkull is false)
        {
            aggressor.SetSkull(Skull.White, whiteSkullEndingDate);
        }
    }

    public void UpdatePlayerSkull(IPlayer aggressor)
    {
        if (!gameConfiguration.PvP?.SkullSystemEnabled ?? false) return;

        if (aggressor.Skull is Skull.Black) return;

        var pvpConfiguration = gameConfiguration.PvP;

        var backSkullEndingDate = DateTime.Now.AddDays(pvpConfiguration.BlackSkullDurationDays);
        var redSkullEndingDate = DateTime.Now.AddDays(pvpConfiguration.RedSkullDurationDays);
        var whiteSkullEndingDate = DateTime.Now.AddDays(pvpConfiguration.WhiteSkullDurationMinutes);

        if (aggressor.NumberOfUnjustifiedKillsLastDay >= pvpConfiguration.DayKillsToBlackSkull)
        {
            aggressor.SetSkull(Skull.Black, backSkullEndingDate);
            return;
        }

        if (aggressor.NumberOfUnjustifiedKillsLastDay >= pvpConfiguration.DayKillsToRedSkull)
        {
            aggressor.SetSkull(Skull.Red, redSkullEndingDate);
            return;
        }

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

        if (aggressor.NumberOfUnjustifiedKillsLastDay > 0)
        {
            aggressor.SetSkull(Skull.White, whiteSkullEndingDate);
            return;
        }
    }
}