using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IItemRequirementService
{
    Result PlayerCanUseItem(IPlayer player, IItem item, Requirement requirement);
    Result PlayerCanEquipItem(IPlayer player, IItem item, Requirement requirement);
}

public readonly ref struct Requirement
{
    public ushort MinLevel { get; init; }
    public byte[] RequiredVocations { get; init; }
    public Slot Slot { get; init; }
    public bool RequirePremiumTime { get; init; }
    public ushort MinMagicLevel { get; init; }
}