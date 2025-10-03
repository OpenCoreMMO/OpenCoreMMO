using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.Containers;

namespace NeoServer.Domain.Repositories;

public interface IPlayerMailRepository
{
    /// <summary>
    /// Get total numbers of items in the player's root inbox.
    /// </summary>
    Task<int> GetInboxItemCount(int playerId);

    Task AddParcelToInbox(int playerId, Parcel parcel);
    Task AddLetterToInbox(int playerId, Letter letter);
}