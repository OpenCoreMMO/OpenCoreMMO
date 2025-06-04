namespace NeoServer.Domain.SafeTrade.Validations;

public enum SafeTradeError
{
    None,
    InvalidParameters,
    BothPlayersAreTheSame,
    PlayerAlreadyTrading,
    TradeHasNoItems,
    NonPickupableItem,
    MoreThan255Items,
    ItemAlreadyBeingTraded,
    PlayerNotCloseToItem,
    PlayersNotCloseToEachOther,
    HasNoSightClearToPlayer,
    SecondPlayerAlreadyTrading,
    PlayerDoesNotHaveEnoughCapacity,
    PlayerDoesNotHaveEnoughRoomToCarry,
    PlayerCannotTradeItem
}