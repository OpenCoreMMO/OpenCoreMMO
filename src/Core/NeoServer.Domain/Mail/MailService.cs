using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.Containers;
using NeoServer.Domain.Locker;
using NeoServer.Domain.Repositories;

namespace NeoServer.Domain.Mail;

public class MailService(
    IPlayerRepository playerRepository,
    IPlayerMailRepository mailRepository,
    LockerManager lockerManager,
    IItemTypeStore itemTypeStore) : IMailService
{
    public Result Send(IPlayer sender, IItem item)
    {
        if (item is Parcel parcel) return SendParcel(sender, parcel);

        if (item is Letter letter) return SendLetter(sender, letter);

        return Result.NotPossible;
    }

    public Result CanSend(IItem item)
    {
        if (!item.IsMailable) return Result.NotPossible;

        if (item is Parcel parcel)
            return CanSendParcel(parcel, out _) ? Result.Success : Result.Fail(InvalidOperation.ItemCannotBeSend);

        if (item is Letter letter)
            return CanSendLetter(letter, out _) ? Result.Success : Result.Fail(InvalidOperation.ItemCannotBeSend);

        return Result.NotPossible;
    }

    public void Stamp(IItem item)
    {
        if (item is Parcel parcel)
        {
            itemTypeStore.TryGetValue(GameConstants.STAMPED_PARCEL_SERVER_ID, out var stampedParcelType);
            if (stampedParcelType is null) return;

            parcel.UpdateMetadata(stampedParcelType);
        }

        if (item is Letter letter)
        {
            itemTypeStore.TryGetValue(GameConstants.STAMPED_LETTER_SERVER_ID, out var stampedLetterType);
            if (stampedLetterType is null) return;

            letter.UpdateMetadata(stampedLetterType);
        }
    }

    private bool CanSendParcel(Parcel parcel, out int playerId)
    {
        playerId = 0;

        if (parcel is null) return false;


        if (parcel.NumberOfLabels is 0 or > 1) return false;

        if (parcel.Label is not { } label) return false;

        if (!label.HasDestination) return false;

        playerId = playerRepository.GetIdByName(label.Destination).Result;

        if (playerId == 0) return false;

        var numberOfItemsInInbox = mailRepository.GetInboxItemCount(playerId).Result;

        if (numberOfItemsInInbox > GameConstants.MAX_NUMBER_OF_ITEMS_ON_INBOX) return false;

        return true;
    }

    private Result SendParcel(IPlayer sender, Parcel parcel)
    {
        Guard.ThrowIfNull(sender, parcel);

        if (!CanSendParcel(parcel, out var playerId)) return Result.Fail(InvalidOperation.ItemCannotBeSend);

        var locker = lockerManager.Get((uint)playerId);

        Stamp(parcel);

        if (locker?.Items.ElementAtOrDefault(1) is IContainer mailInbox)
        {
            mailInbox.AddItem(parcel);
            return Result.Success;
        }

        mailRepository.AddParcelToInbox(playerId, parcel);
        return Result.Success;
    }

    private bool CanSendLetter(Letter letter, out int playerId)
    {
        playerId = 0;
        if (!letter.HasDestination) return false;

        playerId = playerRepository.GetIdByName(letter.Destination).Result;

        if (playerId == 0) return false;

        var numberOfItemsInInbox = mailRepository.GetInboxItemCount(playerId).Result;

        if (numberOfItemsInInbox > GameConstants.MAX_NUMBER_OF_ITEMS_ON_INBOX) return false;

        return true;
    }

    private Result SendLetter(IPlayer sender, Letter letter)
    {
        Guard.ThrowIfNull(sender, letter);

        if (!CanSendLetter(letter, out var playerId)) return Result.Fail(InvalidOperation.ItemCannotBeSend);

        var locker = lockerManager.Get((uint)playerId);

        Stamp(letter);

        if (locker?.Items.ElementAtOrDefault(1) is IContainer mailInbox)
        {
            mailInbox.AddItem(letter);
            return Result.Success;
        }

        mailRepository.AddLetterToInbox(playerId, letter);
        return Result.Success;
    }
}

public interface IMailService
{
    Result Send(IPlayer sender, IItem item);
    Result CanSend(IItem item);
    void Stamp(IItem item);
}