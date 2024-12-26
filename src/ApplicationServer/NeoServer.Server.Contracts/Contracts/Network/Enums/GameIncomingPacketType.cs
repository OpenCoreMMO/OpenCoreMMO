namespace NeoServer.Server.Common.Contracts.Network.Enums;

public enum GameIncomingPacketType : byte
{
    None = 0x00,
    PlayerLoginRequest = 0x01,
    PlayerLogIn = 0x0A,
    PlayerLogOut = 0x14, // logout
    Ping = 0x1E, // keep alive / ping response
    AutoMove = 0x64, // move with autowalk
    WalkNorth = 0x65, // move north
    WalkEast = 0x66, // move east
    WalkSouth = 0x67, // move south
    WalkWest = 0x68, // move west
    CancelAutoWalk = 0x69, // stop-autowalk
    WalkNorteast = 0x6A,
    WalkSoutheast = 0x6B,
    WalkSouthwest = 0x6C,
    WalkNorthwest = 0x6D,
    TurnNorth = 0x6F, // turn north
    TurnEast = 0x70, // turn east
    TurnSouth = 0x71, // turn south
    TurnWest = 0x72, // turn west
    ItemThrow = 0x78, // throw item
    PlayerPurchase = 0x7A,
    PlayerSale = 0x7B,
    CloseShop = 0x7C, //close shop
    TradeRequest = 0x7D, // Request trade
    TradeLook = 0x7E, // Look at an item in trade
    TradeAccept = 0x7F, // Accept trade
    TradeCancel = 0x80, // Close/cancel trade
    ItemUse = 0x82, // use item
    ItemUseOn = 0x83, // use item
    ItemUseOnCreature = 0x84, // battle window
    ItemRotate = 0x85, // rotate item
    ContainerClose = 0x87, // close container
    ContainerUp = 0x88, // "up-arrow" - container
    WindowText = 0x89,
    WindowHouse = 0x8A,
    LookAt = 0x8C, // look at item
    LookInBattleList = 0x8D,
    JoinAgression = 0x8E,
    Speech = 0x96, // say something
    ChannelListRequest = 0x97, // request Channels
    ChannelOpen = 0x98, // open Channel
    ChannelClose = 0x99, // close Channel
    ChannelOpenPrivate = 0x9A, // open priv
    ReportProcess = 0x9B, // process report
    ReportClose = 0x9C, // gm closes report
    ReportCancel = 0x9D, // player cancels report
    NpcChannelClose = 0x9E, // player cancels report
    ChangeModes = 0xA0, // set attack and follow mode
    Attack = 0xA1, // attack
    Follow = 0xA2, // follow
    PartyInvite = 0xA3,
    PartyJoin = 0xA4,
    PartyRevoke = 0xA5,
    PartyPassLeadership = 0xA6,
    PartyLeave = 0xA7,
    EnableSharedExp = 0xA8,
    ChannelCreatePrivate = 0xAA,
    ChannelInvite = 0xAB,
    ChannelExclude = 0xAC,
    StopAllActions = 0xBE, // cancel move
    ResendTile = 0xC9, // client request to resend the tile

    ResentContainer =
        0xCA, // client request to resend the container (happens when you store more than container maxsize)
    OutfitChangeRequest = 0xD2, // request outfit
    OutfitChangeCompleted = 0xD3, // set outfit
    AddVip = 0xDC,
    RemoveVip = 0xDD,
    ReportBug = 0xE6,
    ReportViolation = 0xE7,
    ReportDebugAssertion = 0xE8,
    NewPing = 0x40,
    Any = 0xFF // Do not send
}