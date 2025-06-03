using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Items.Types;

public interface IMagicField
{
    Location.Structs.Location Location { get; }
    IItemType Metadata { get; }

    void CauseDamage(ICreature toCreature);
}