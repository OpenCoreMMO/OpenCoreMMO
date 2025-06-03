using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Items.Types;

public interface ITeleport
{
    bool HasDestination { get; }
    bool Teleport(IWalkableCreature player);
}