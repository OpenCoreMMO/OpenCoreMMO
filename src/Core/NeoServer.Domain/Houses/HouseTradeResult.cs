namespace NeoServer.Domain.Houses;

public enum HouseTradeResult : byte
{
    NoError,
    TradePlayerFarAway,
    YouDontOwnThisHouse,
    TradePlayerAlreadyOwnsAHouse,
    TradePlayerHighestBidder,
    YouCannotTradeThisHouse
}
