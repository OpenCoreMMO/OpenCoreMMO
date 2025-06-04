namespace NeoServer.Domain.Common.Contracts.Services;

public interface ISharedExperienceConfiguration
{
    /// <summary>
    ///     If enabled, removes the need for party leaders to enable experience sharing.
    ///     Party experience sharing will still be subject to other configured restrictions.
    /// </summary>
    bool IsSharedExperienceAlwaysOn { get; }

    /// <summary>
    ///     If enabled, experience sharing is turned off if any members are beyond maximum distance.
    ///     If disabled, party member proximity is no longer required for experience sharing.
    /// </summary>
    bool RequirePartyProximity { get; }

    /// <summary>
    ///     Maximum distance between all party members for any of them to receive shared experience.
    /// </summary>
    int MaximumPartyDistanceToReceiveExperienceSharing { get; }

    /// <summary>
    ///     Maximum floors between all party members for any of them to receive shared experience.
    ///     0 = Same Floor Required, 1 = One Floor up or down allowed, etc.
    /// </summary>
    int MaximumPartyVerticalDistanceToReceiveExperienceSharing { get; }

    /// <summary>
    ///     If disabled, party members will receive experience sharing regardless of member level.
    /// </summary>
    bool RequirePartyMemberLevelProximity { get; }

    /// <summary>
    ///     If everyone did not participate, nobody receives experience.
    /// </summary>
    bool RequirePartyMemberParticipation { get; }

    /// <summary>
    ///     A monster must give this much experience to even be considered for experience sharing.
    ///     A value of 0 would make all monsters experience share, even snakes.
    ///     Keep in mind that shared experience is divided evenly among party members.
    /// </summary>
    uint MinimumMonsterExperienceToBeShared { get; }

    /// <summary>
    ///     For the party to be eligable for shared experience one of the following must be true:
    ///     1) RequirePartyMemberLevelProximity must be false.
    ///     2) This value multiplied by the lowest level in the party must be greater than or equal to the highest level in the
    ///     party.
    /// </summary>
    double LowestLevelSupportedMultipler { get; }

    /// <summary>
    ///     All party members must be active for experience sharing to occur.
    ///     Either they must have attacked the monster or healed in the past X seconds to be considered active.
    /// </summary>
    int SecondsBetweenHealsToBeConsideredActive { get; }

    /// <summary>
    ///     Bonus experience factor based on the number of unique vocations in the party.
    ///     The index corresponds to the number of unique vocations and the value is the bonus experience factor.
    /// </summary>
    double[] UniqueVocationBonusExperienceFactor { get; }
}