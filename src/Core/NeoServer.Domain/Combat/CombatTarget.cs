using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Combat;

public class CombatTarget
{
    public CombatTarget(ICombatActor creature)
    {
        Creature = creature;
    }

    public ICombatActor Creature { get; }
    public Direction[] PathToCreature { get; private set; }
    public bool CanReachCreature { get; private set; } = true;
    public bool HasSightClear { get; private set; }
    public LinkedListNode<CombatTarget> Node { get; set; }

    public bool IsInRange(IMonster monster)
    {
        return Creature.Location.GetSqmDistance(monster.Location) <=
               monster.Metadata.MaxRangeDistanceAttack;
    }

    private void SetAsUnreachable()
    {
        CanReachCreature = false;
    }

    public void SetAsReachable(Direction[] path)
    {
        CanReachCreature = true;
        PathToCreature = path;
    }

    public void SetAsHasSightClear()
    {
        HasSightClear = true;
    }

    private void SetAsNoSightClear()
    {
        HasSightClear = false;
    }

    public void ResetFlags()
    {
        SetAsUnreachable();
        SetAsNoSightClear();
    }
}