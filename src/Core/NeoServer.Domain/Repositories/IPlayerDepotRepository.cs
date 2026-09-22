using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Repositories;

/// <summary>
///     Repository for player depot items, returning domain types.
///     Implemented in the Data layer; consumed by domain services.
/// </summary>
public interface IPlayerDepotRepository
{
    /// <summary>Loads the depot contents for the given player as a container.</summary>
    Task LoadDepotChest(IContainer chest, Location depotLocation, uint playerId);

    /// <summary>Saves the complete depot chest contents, replacing any existing data.</summary>
    Task Save(IPlayer player, IContainer depotChest);
}
