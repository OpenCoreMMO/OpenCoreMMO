using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items;

public class Ground : Item, IGround
{
    public Ground(IItemType type, Location location) : base(type, location)
    {
    }

    public event CreatureWalkedThroughGround OnCreatureWalkedThrough;
    public ushort StepSpeed => (Metadata?.Speed ?? 0) != 0 ? Metadata.Speed : (ushort)150;
    public byte MovementPenalty => Metadata.Attributes.GetAttribute<byte>(ItemTypeAttribute.Waypoints);

    public void CreatureEntered(ICreature creature)
    {
        OnCreatureWalkedThrough?.Invoke(creature, this);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.Ground;
    }
}