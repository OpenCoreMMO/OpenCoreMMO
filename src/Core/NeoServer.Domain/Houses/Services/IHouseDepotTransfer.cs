namespace NeoServer.Domain.Houses.Services;

/// <summary>Moves items from a house into the previous owner's shared depot on ownership change.</summary>
public interface IHouseDepotTransfer
{
    /// <summary>
    ///     Packs transferable house items into a new backpack in the depot of <paramref name="ownerId" />.
    ///     The depot is shared across towns.
    /// </summary>
    void TransferToOwnerDepot(House house, uint ownerId);
}
