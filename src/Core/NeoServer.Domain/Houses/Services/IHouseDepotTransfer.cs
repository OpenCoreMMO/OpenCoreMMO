using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Player;

namespace NeoServer.Domain.Houses.Services;

/// <summary>Moves items from a house to the old owner's depot on ownership change.</summary>
public interface IHouseDepotTransfer
{
    void TransferToOwnerDepot(House house, IPlayer player);
}
