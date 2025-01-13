using System;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;

namespace NeoServer.Game.Creatures.Experience;

/// <summary>
///     The shared experience bonus applied to members of a party.
/// </summary>
public class SharedExperienceBonus : IExperienceBonus
{
    private readonly ISharedExperienceConfiguration Configuration;

    public SharedExperienceBonus(ISharedExperienceConfiguration config)
    {
        Configuration = config;
    }

    public ExperienceBonusType BonusType => ExperienceBonusType.Standard;
    public string Name => "Party Shared Experience";

    public double GetBonusFactorAmount(IPlayer player, IMonster monster)
    {
        if (player == null || player.PlayerParty.IsInParty == false) return 0.0;
        return GetPartyBonusFactor(player.PlayerParty.Party);
    }

    public bool IsEnabled(IPlayer player, IMonster monster)
    {
        return player != null
               && monster != null
               && player.PlayerParty.IsInParty
               && CanPartyReceiveSharedExperience(player.PlayerParty.Party, monster);
    }

    /// <summary>
    ///     Combines all of the experience sharing requirements into a single check.
    /// </summary>
    /// <param name="party">The party to receive experience sharing bonus.</param>
    /// <param name="monster">The monster killed by the party.</param>
    public bool CanPartyReceiveSharedExperience(IParty party, IMonster monster)
    {
        return IsExperienceSharingTurnedOn(party)
               && DoesMonsterQualify(monster)
               && ArePartyCloseEnoughToEachOther(party)
               && ArePartyLevelsInProperRange(party)
               && IsEveryMemberActive(party, monster);
    }

    /// <summary>
    ///     Checks if the party has experience sharing turned on.
    /// </summary>
    /// <param name="party">The party to receive experience sharing bonus.</param>
    public bool IsExperienceSharingTurnedOn(IParty party)
    {
        return Configuration.IsSharedExperienceAlwaysOn || party.IsSharedExperienceEnabled;
    }

    /// <summary>
    ///     If shared experience is activated, an experience bonus of 20% is granted
    ///     if the defeated creature usually yields at least 20 experience points.
    /// </summary>
    /// <param name="monster">The creature defeated by the party.</param>
    public bool DoesMonsterQualify(IMonster monster)
    {
        return monster.Experience >= Configuration.MinimumMonsterExperienceToBeShared;
    }

    /// <summary>
    ///     Only characters that are of similar levels can share experience when hunting together.
    ///     To be more precise, the lowest character in a party may not have less than two-thirds of the levels of the highest
    ///     character.
    ///     This means a level 40 can share experience with a level 60 but not with a level 20; or a level 200 can share
    ///     experience with a level 300.
    /// </summary>
    /// <param name="party">The party to receive experience sharing bonus.</param>
    public bool ArePartyLevelsInProperRange(IParty party)
    {
        if (Configuration.RequirePartyMemberLevelProximity == false) return true;

        var lowestLevel = int.MaxValue;
        var highestLevel = 0;

        var members = party.Members;
        foreach (var member in members)
        {
            lowestLevel = Math.Min(lowestLevel, member.Level);
            highestLevel = Math.Max(highestLevel, member.Level);
        }

        return highestLevel <= lowestLevel * Configuration.LowestLevelSupportedMultipler;
    }

    /// <summary>
    ///     The distance of all party members to the leader must be smaller or equal to 30 fields,
    ///     this works also if you are one floor up or down.
    /// </summary>
    /// <param name="party">The party to receive experience sharing bonus.</param>
    public bool ArePartyCloseEnoughToEachOther(IParty party)
    {
        if (Configuration.RequirePartyProximity == false) return true;

        var members = party.Members;

        foreach (var memberA in members)
        foreach (var memberB in members)
        {
            if (memberA == memberB) continue;

            var horizontalDistance = memberA.Location.GetSqmDistance(memberB.Location);
            if (horizontalDistance > Configuration.MaximumPartyDistanceToReceiveExperienceSharing) return false;

            var verticalDistance = memberA.Location.GetOffSetZ(memberB.Location);
            if (verticalDistance > Configuration.MaximumPartyVerticalDistanceToReceiveExperienceSharing) return false;
        }

        return true;
    }

    /// <summary>
    ///     Finally, all party members must be actively involved.
    ///     This means they must either have healed another member or attacked an aggressive monster.
    /// </summary>
    /// <param name="party">The party to receive experience sharing bonus.</param>
    /// <param name="monster">The monster killed by the party.</param>
    public bool IsEveryMemberActive(IParty party, IMonster monster)
    {
        if (Configuration.RequirePartyMemberParticipation == false) return true;

        var members = party.Members;
        foreach (var member in members)
        {
            if (party.Heals.TryGetValue(member, out var lastHealedOn) &&
                lastHealedOn.AddSeconds(Configuration.SecondsBetweenHealsToBeConsideredActive) >=
                DateTime.UtcNow) continue;

            var damageDealt = monster.ReceivedDamages.GetCreatureDamage(member)?.Damage ?? 0;
            if (damageDealt > 0) continue;
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Determines how much of an experience bonus the party will receive.
    /// </summary>
    /// <param name="party">The party to receive experience sharing bonus.</param>
    public double GetPartyBonusFactor(IParty party)
    {
        var maxVocationCount = Configuration.UniqueVocationBonusExperienceFactor.Length;
        var vocationCount = Math.Min(maxVocationCount, Math.Max(1, party.Members.GroupBy(x => x.Vocation.Id).Count()));
        return Configuration.UniqueVocationBonusExperienceFactor[vocationCount - 1];
    }
}