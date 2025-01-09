using System;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;

namespace NeoServer.Game.Combat.Services;

public class PlayerSkullService(GameConfiguration gameConfiguration) : IPlayerSkullService
{
    public void UpdatePlayerSkull(IPlayer aggressor)
    {
        if (aggressor.Skull is Skull.Black) return;

        var pvpConfiguration = gameConfiguration.PvP;

        var backSkullEndingDate = DateTime.Now.AddDays(pvpConfiguration.BlackSkullDurationDays);
        var redSkullEndingDate = DateTime.Now.AddDays(pvpConfiguration.RedSkullDurationDays);

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

        if (aggressor.NumberOfUnjustifiedKillsLastYear >= pvpConfiguration.MonthKillsToBlackSkull)
        {
            aggressor.SetSkull(Skull.Black, backSkullEndingDate);
            return;
        }

        if (aggressor.NumberOfUnjustifiedKillsLastYear >= pvpConfiguration.MonthKillsToRedSkull)
        {
            aggressor.SetSkull(Skull.Red, redSkullEndingDate);
        }
    }
}