using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Common.Contracts.Items.Types;

public interface IMagicField:IThing
{
    IItemType Metadata { get; }
    MinMax Damage { get; }
    byte DamageCount { get; }
    DamageType DamageType { get; }
    int Interval { get; }

    void CauseDamage(ICreature toCreature);
}