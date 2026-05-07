using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.Containers;

namespace NeoServer.Domain.Repositories;

public interface IPlayerMailRepository
{
    /// <summary>
    ///     Get total numbers of items in the player's root inbox.
    /// </summary>
    int GetInboxItemCount(int playerId);

    void AddParcelToInbox(int playerId, Parcel parcel);
    void AddLetterToInbox(int playerId, Letter letter);
}