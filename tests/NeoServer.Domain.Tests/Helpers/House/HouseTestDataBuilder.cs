using Moq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Guild;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.AccessList;

namespace NeoServer.Domain.Tests.Helpers.House;

public static class HouseTestDataBuilder
{
    public static Domain.Houses.House Build(
        uint id = 1,
        string name = "Test House",
        ushort townId = 1,
        uint rent = 1000,
        uint ownerGuid = 0,
        string ownerName = null,
        int ownerAccountId = 0,
        DateTime? paidUntil = null,
        byte payRentWarnings = 0,
        Location? entryPosition = null,
        List<Mock<IDynamicTile>> tiles = null,
        List<(uint DoorId, Mock<IItem> Item)> doors = null,
        List<Mock<IItem>> beds = null,
        Dictionary<uint, HouseAccessList> accessLists = null)
    {
        var house = new Domain.Houses.House
        {
            Id = id,
            Name = name,
            TownId = townId,
            Rent = rent,
            OwnerGuid = ownerGuid,
            OwnerName = ownerName ?? string.Empty,
            OwnerAccountId = ownerAccountId,
            PaidUntil = paidUntil,
            PayRentWarnings = payRentWarnings,
            EntryPosition = entryPosition
        };

        if (tiles is not null)
        {
            foreach (var tileMock in tiles)
            {
                tileMock.Setup(x => x.Location).Returns(new Location(100, 100, 7));
                house.LinkTile(tileMock.Object);
            }
        }

        if (doors is not null)
        {
            foreach (var (doorId, doorMock) in doors)
                house.LinkDoor(doorId, doorMock.Object);
        }

        if (beds is not null)
        {
            foreach (var bedMock in beds)
                house.LinkBed(bedMock.Object);
        }

        if (accessLists is not null)
        {
            foreach (var (listId, list) in accessLists)
                house.SetAccessList(listId, list);
        }

        return house;
    }

    public static Mock<IDynamicTile> CreateTileMock(List<IPlayer> players = null, List<IItem> items = null)
    {
        var tileMock = new Mock<IDynamicTile>();
        tileMock.Setup(x => x.Location).Returns(new Location(100, 100, 7));
        tileMock.Setup(x => x.Players).Returns(players ?? new List<IPlayer>());
        tileMock.Setup(x => x.AllItems).Returns(items?.ToArray() ?? Array.Empty<IItem>());
        tileMock.Setup(x => x.HasFlag(It.IsAny<TileFlags>())).Returns(false);
        tileMock.SetupProperty(x => x.CanEnterFunction);
        return tileMock;
    }

    public static Mock<IItem> CreateItemMock(bool isPickupable = true, ushort serverId = 100)
    {
        var itemMock = new Mock<IItem>();
        itemMock.Setup(x => x.IsPickupable).Returns(isPickupable);
        itemMock.Setup(x => x.ServerId).Returns(serverId);
        return itemMock;
    }

    public static IPlayer CreatePlayer(uint id = 1, ushort level = 10, uint guildId = 0, GuildRankInfo guildRank = null, string name = "Player")
    {
        var playerMock = new Mock<IPlayer>();
        playerMock.Setup(x => x.Id).Returns(id);
        playerMock.Setup(x => x.Level).Returns(level);
        playerMock.Setup(x => x.Name).Returns(name);
        playerMock.Setup(x => x.HasGuild).Returns(guildId != 0);
        playerMock.Setup(x => x.GuildId).Returns((ushort)guildId);
        playerMock.Setup(x => x.GuildRank).Returns(guildRank);
        playerMock.Setup(x => x.AccountId).Returns(id);
        return playerMock.Object;
    }

    public static IPlayer CreatePlayerWithBank(uint id = 1, ulong bankAmount = 0, string name = "Player")
    {
        var bankMock = new Mock<IBank>();
        bankMock.Setup(x => x.Amount).Returns(bankAmount);

        var playerMock = new Mock<IPlayer>();
        playerMock.Setup(x => x.Id).Returns(id);
        playerMock.Setup(x => x.Name).Returns(name);
        playerMock.Setup(x => x.Bank).Returns(bankMock.Object);
        playerMock.Setup(x => x.BankAmount).Returns(bankAmount);
        playerMock.Setup(x => x.AccountId).Returns(id);
        return playerMock.Object;
    }

    public static HouseAccessList CreateAccessList(params string[] playerNames)
    {
        var list = new HouseAccessList();
        foreach (var name in playerNames)
            list.AddPlayer(name);
        return list;
    }
}
