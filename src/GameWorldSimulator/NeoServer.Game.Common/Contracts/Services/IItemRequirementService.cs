using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Common.Contracts.Services;

public interface IItemRequirementService
{
    Result PlayerCanUseItem(IPlayer player, IItem item, Requirement requirement);
    Result PlayerCanEquipItem(IPlayer player, IItem item, Requirement requirement);
}

public readonly ref struct Requirement
{
    public uint MinLevel { get; init; }
    public byte[] RequiredVocations { get; init; }
    public Slot Slot { get; init; }
    public bool RequirePremiumTime { get; init; }
    public uint MinMagicLevel { get; init; }
}