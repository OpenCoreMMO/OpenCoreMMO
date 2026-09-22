using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.SafeTrade.Validations;

namespace NeoServer.Domain.Houses.Services;

public class HouseTradeService(
    IHouseService houseService,
    IHouseStore houseStore,
    ITradeService tradeService,
    IItemTypeStore itemTypeStore,
    HouseConfiguration houseConfiguration) : IHouseTradeService
{
    public HouseTradeResult StartTrade(House house, IPlayer seller, IPlayer partner)
    {
        if (house is null || seller is null || partner is null || seller == partner)
        {
            return HouseTradeResult.YouCannotTradeThisHouse;
        }

        if (seller.Location.Z != partner.Location.Z ||
            seller.Location.GetMaxSqmDistance(partner.Location) > 2)
        {
            return HouseTradeResult.TradePlayerFarAway;
        }

        if (house.OwnerGuid != seller.Id)
        {
            return HouseTradeResult.YouDontOwnThisHouse;
        }

        if (houseStore.GetByOwnerGuid(partner.Id) is not null)
        {
            return HouseTradeResult.TradePlayerAlreadyOwnsAHouse;
        }

        if (house.PendingTransfer is not null)
        {
            return HouseTradeResult.YouCannotTradeThisHouse;
        }

        itemTypeStore.TryGetValue(HouseTransferItem.DocumentItemId, out var sourceType);
        var metadata = HouseTransferItem.CreateMetadata(sourceType);
        var transferItem = HouseTransferItem.Create(metadata, house, seller, buyer =>
        {
            houseService.SetOwner(
                house,
                buyer.Id,
                buyer.Name,
                (int)buyer.AccountId,
                updatePaidUntil: false,
                DateTime.UtcNow,
                houseConfiguration.RentPeriodSeconds);
        });

        if (!house.TryAttachTransfer(transferItem))
        {
            return HouseTradeResult.YouCannotTradeThisHouse;
        }

        var tradeResult = tradeService.Request(seller, partner, transferItem);
        if (tradeResult is not SafeTradeError.None)
        {
            house.ResetTransfer();
        }

        return HouseTradeResult.NoError;
    }
}
