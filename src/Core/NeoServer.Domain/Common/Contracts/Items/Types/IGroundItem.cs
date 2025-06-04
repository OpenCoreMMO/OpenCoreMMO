using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Items.Types;

public delegate void CreatureWalkedThroughGround(ICreature creature, IGround ground);

public interface IGround : IItem, IHasDecay
{
    public ushort StepSpeed { get; }
    byte MovementPenalty { get; }
    void CreatureEntered(ICreature creature);
    event CreatureWalkedThroughGround OnCreatureWalkedThrough;
}