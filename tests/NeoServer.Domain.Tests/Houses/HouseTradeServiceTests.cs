using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.AccessList;
using NeoServer.Domain.Houses.Services;
using NeoServer.Domain.Items.Services;
using NeoServer.Domain.Repositories;
using NeoServer.Domain.SafeTrade;
using NeoServer.Domain.SafeTrade.Operations;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.House;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Houses;

public class HouseTradeServiceTests
{
    [Fact]
    [Trait("Category", "Validation")]
    public void StartTrade_when_partner_is_too_far_returns_trade_player_far_away()
    {
        var context = CreateContext();
        ((DynamicTile)context.Map[100, 104, 7]).AddCreature(context.Partner);

        var result = context.TradeService.StartTrade(context.House, context.Seller, context.Partner);

        result.Should().Be(HouseTradeResult.TradePlayerFarAway);
        context.House.PendingTransfer.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void StartTrade_when_partner_is_on_another_floor_returns_trade_player_far_away()
    {
        var context = CreateContext(toZ: 8);
        ((DynamicTile)context.Map[101, 100, 8]).AddCreature(context.Partner);

        var result = context.TradeService.StartTrade(context.House, context.Seller, context.Partner);

        result.Should().Be(HouseTradeResult.TradePlayerFarAway);
        context.House.PendingTransfer.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void StartTrade_when_seller_is_not_owner_returns_you_dont_own_this_house()
    {
        var context = CreateContext();
        PlaceAdjacent(context);
        context.House.OwnerGuid = 99;

        var result = context.TradeService.StartTrade(context.House, context.Seller, context.Partner);

        result.Should().Be(HouseTradeResult.YouDontOwnThisHouse);
        context.House.PendingTransfer.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void StartTrade_when_partner_already_owns_a_house_returns_already_owns_a_house()
    {
        var context = CreateContext();
        PlaceAdjacent(context);
        var otherHouse = HouseTestDataBuilder.Build(id: 2, ownerGuid: context.Partner.Id);
        context.HouseStore.AddOrUpdate(otherHouse.Id, otherHouse);

        var result = context.TradeService.StartTrade(context.House, context.Seller, context.Partner);

        result.Should().Be(HouseTradeResult.TradePlayerAlreadyOwnsAHouse);
        context.House.PendingTransfer.Should().BeNull();
    }

    [Fact]
    [ThreadBlocking]
    [Trait("Category", "HappyPath")]
    public void StartTrade_when_players_are_in_range_opens_trade_and_attaches_transfer()
    {
        var context = CreateContext();
        PlaceAdjacent(context);

        var result = context.TradeService.StartTrade(context.House, context.Seller, context.Partner);

        result.Should().Be(HouseTradeResult.NoError);
        context.House.PendingTransfer.Should().NotBeNull();

        context.SafeTrade.Cancel(context.Seller);
        context.House.PendingTransfer.Should().BeNull();
    }

    [Fact]
    [ThreadBlocking]
    [Trait("Category", "Validation")]
    public void StartTrade_when_a_transfer_is_already_pending_returns_you_cannot_trade_this_house()
    {
        var context = CreateContext();
        PlaceAdjacent(context);
        context.TradeService.StartTrade(context.House, context.Seller, context.Partner);

        var result = context.TradeService.StartTrade(context.House, context.Seller, context.Partner);

        result.Should().Be(HouseTradeResult.YouCannotTradeThisHouse);
        context.SafeTrade.Cancel(context.Seller);
    }

    [Fact]
    [ThreadBlocking]
    [Trait("Category", "Validation")]
    public void StartTrade_when_seller_is_already_trading_returns_no_error_and_clears_pending()
    {
        var context = CreateContext();
        PlaceAdjacent(context);

        var weapon = ItemTestDataBuilder.CreateWeaponItem(1);
        context.Seller.Inventory.AddItem(weapon, Slot.Left);
        context.SafeTrade.Request(context.Seller, context.Partner, weapon);

        var result = context.TradeService.StartTrade(context.House, context.Seller, context.Partner);

        result.Should().Be(HouseTradeResult.NoError);
        context.House.PendingTransfer.Should().BeNull();
        context.SafeTrade.Cancel(context.Seller);
    }

    [Fact]
    [ThreadBlocking]
    [Trait("Category", "HappyPath")]
    public void Completing_trade_transfers_ownership_and_does_not_leave_the_document()
    {
        var paidUntil = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var context = CreateContext(paidUntil: paidUntil, payRentWarnings: 4);
        PlaceAdjacent(context);

        var payment = ItemTestDataBuilder.CreateWeaponItem(20, weight: 10);
        context.Partner.Inventory.AddItem(payment, Slot.Left);

        context.TradeService.StartTrade(context.House, context.Seller, context.Partner);
        context.SafeTrade.Request(context.Partner, context.Seller, payment);
        context.SafeTrade.AcceptTrade(context.Seller);
        var result = context.SafeTrade.AcceptTrade(context.Partner);

        result.Should().Be(NeoServer.Domain.SafeTrade.Validations.SafeTradeError.None);
        context.House.OwnerGuid.Should().Be(context.Partner.Id);
        context.House.OwnerName.Should().Be(context.Partner.Name);
        context.House.PaidUntil.Should().Be(paidUntil);
        context.House.PayRentWarnings.Should().Be(0);
        context.House.PendingTransfer.Should().BeNull();
        context.House.GetAccessList(HouseListId.GuestList).Should().BeNull();
        context.Seller.Inventory[Slot.Left].Should().Be(payment);
        context.Partner.Inventory[Slot.Left].Should().BeNull();
    }

    [Fact]
    [ThreadBlocking]
    [Trait("Category", "HappyPath")]
    public void Cancelling_trade_leaves_ownership_unchanged()
    {
        var context = CreateContext();
        PlaceAdjacent(context);

        context.TradeService.StartTrade(context.House, context.Seller, context.Partner);
        context.SafeTrade.Cancel(context.Seller);

        context.House.OwnerGuid.Should().Be(context.Seller.Id);
        context.House.PendingTransfer.Should().BeNull();
    }

    private static void PlaceAdjacent(TradeContext context)
    {
        ((DynamicTile)context.Map[100, 100, 7]).AddCreature(context.Seller);
        ((DynamicTile)context.Map[101, 100, 7]).AddCreature(context.Partner);
    }

    private static TradeContext CreateContext(
        byte toZ = 7,
        DateTime? paidUntil = null,
        byte payRentWarnings = 0)
    {
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, toZ);
        var seller = PlayerTestDataBuilder.Build(id: 1, name: "Seller", map: map, capacity: 1000);
        var partner = PlayerTestDataBuilder.Build(id: 2, name: "Partner", map: map, capacity: 1000);

        var house = HouseTestDataBuilder.Build(
            id: 1,
            ownerGuid: seller.Id,
            ownerName: seller.Name,
            paidUntil: paidUntil,
            payRentWarnings: payRentWarnings,
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
            });

        var houseStore = new HouseStore();
        houseStore.AddOrUpdate(house.Id, house);

        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(HouseTransferItem.DocumentItemId, HouseTransferItem.CreateMetadata());

        var safeTrade = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);
        var houseService = new HouseService(
            new Mock<IHouseRepository>().Object,
            new Mock<IHouseEviction>().Object,
            new Mock<IHouseBedWaker>().Object,
            new Mock<IHouseDepotTransfer>().Object,
            new Mock<ICreatureGameInstance>().Object,
            new HouseConfiguration());

        var tradeService = new HouseTradeService(
            houseService,
            houseStore,
            safeTrade,
            itemTypeStore,
            new HouseConfiguration());

        return new TradeContext(map, seller, partner, house, houseStore, safeTrade, tradeService);
    }

    private sealed record TradeContext(
        IMap Map,
        IPlayer Seller,
        IPlayer Partner,
        House House,
        HouseStore HouseStore,
        SafeTradeSystem SafeTrade,
        HouseTradeService TradeService);
}
