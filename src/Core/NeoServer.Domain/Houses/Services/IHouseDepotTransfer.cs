using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Houses.Services;

/// <summary>Moves items from a house to the old owner's depot on ownership change.</summary>
public interface IHouseDepotTransfer
{
    void TransferToOwnerDepot(int ownerAccountId, ushort townId, IEnumerable<IItem> items);
}
