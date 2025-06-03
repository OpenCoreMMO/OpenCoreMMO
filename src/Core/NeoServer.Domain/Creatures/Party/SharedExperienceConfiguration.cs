using NeoServer.Domain.Common.Contracts.Services;

namespace NeoServer.Domain.Creatures.Party;

public class SharedExperienceConfiguration : ISharedExperienceConfiguration
{
    public SharedExperienceConfiguration(
        bool? isSharedExperienceAlwaysOn = null,
        bool? requirePartyProximity = null,
        int? maximumPartyDistanceToReceiveExperienceSharing = null,
        int? maximumPartyVerticalDistanceToReceiveExperienceSharing = null,
        bool? requirePartyMemberLevelProximity = null,
        bool? requirePartyMemberParticipation = null,
        uint? minimumMonsterExperienceToBeShared = null,
        double[] uniqueVocationBonusExperienceFactor = null,
        double? lowestLevelSupportedMultipler = null
    )
    {
        IsSharedExperienceAlwaysOn = isSharedExperienceAlwaysOn ?? false;
        RequirePartyProximity = requirePartyProximity ?? true;
        MaximumPartyDistanceToReceiveExperienceSharing = maximumPartyDistanceToReceiveExperienceSharing ?? 30;
        MaximumPartyVerticalDistanceToReceiveExperienceSharing =
            maximumPartyVerticalDistanceToReceiveExperienceSharing ?? 1;
        RequirePartyMemberLevelProximity = requirePartyMemberLevelProximity ?? true;
        RequirePartyMemberParticipation = requirePartyMemberParticipation ?? true;
        MinimumMonsterExperienceToBeShared = minimumMonsterExperienceToBeShared ?? 20;
        LowestLevelSupportedMultipler = lowestLevelSupportedMultipler ?? 3 / 2;
        UniqueVocationBonusExperienceFactor = uniqueVocationBonusExperienceFactor ?? new[] { 0.0, 0.2, 0.6, 1.0 };
    }

    public bool IsSharedExperienceAlwaysOn { get; }
    public bool RequirePartyProximity { get; }
    public int MaximumPartyDistanceToReceiveExperienceSharing { get; }
    public int MaximumPartyVerticalDistanceToReceiveExperienceSharing { get; }
    public bool RequirePartyMemberLevelProximity { get; }
    public bool RequirePartyMemberParticipation { get; }
    public uint MinimumMonsterExperienceToBeShared { get; }
    public double LowestLevelSupportedMultipler { get; }
    public int SecondsBetweenHealsToBeConsideredActive { get; }
    public double[] UniqueVocationBonusExperienceFactor { get; }
}