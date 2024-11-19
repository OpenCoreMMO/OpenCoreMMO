using FluentAssertions;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Services;
using NeoServer.Game.Item.Services;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.World.Map;
using NeoServer.Game.World.Models.Tiles;
using NeoServer.Modules.Trading;
using NeoServer.Modules.Trading.TradeExchange;
using NeoServer.Modules.Trading.Validations;

namespace NeoServer.Game.Systems.Tests.SafeTrade;

public class PreTradeValidationTests
{
    [Fact]
    public void Trade_fails_when_player_tries_to_trade_with_himself()
    {
        //arrange
        var map = new Map(new World.World());
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();
        var item = ItemTestData.CreateWeaponItem(1);

        //act
        var result = tradeSystem.Request(player, player, item);

        //assert
        result.Should().Be(SafeTradeError.BothPlayersAreTheSame);
    }

    [Fact]
    public void Trade_fails_when_player_is_already_trading()
    {
        //arrange
        var map = new Map(new World.World());
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build();

        var item = ItemTestData.CreateWeaponItem(1);

        tradeSystem.Request(player, secondPlayer, item);

        //act
        var result = tradeSystem.Request(player, secondPlayer, item);

        //assert
        result.Should().Be(SafeTradeError.PlayerAlreadyTrading);
    }

    [Fact]
    public void Trade_fails_when_player_trades_an_item_from_another_player()
    {
        //arrange
        var map = new Map(new World.World());
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();

        var inventory = InventoryTestDataBuilder.GenerateInventory();
        var secondPlayer = PlayerTestDataBuilder.Build(inventoryMap: inventory, capacity: 1000);

        //act
        var result = tradeSystem.Request(player, secondPlayer, secondPlayer.Inventory.Weapon);

        //assert
        result.Should().Be(SafeTradeError.PlayerCannotTradeItem);
    }

    [Fact]
    public void Trade_fails_when_it_has_more_than_255_items()
    {
        //arrange
        var map = new Map(new World.World());
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build();

        var backpack1 = ItemTestData.CreatePickupableContainer(255);
        var backpack2 = ItemTestData.CreatePickupableContainer(255);

        Enumerable.Range(1, 200).ToList().ForEach(x => backpack1.AddItem(ItemTestData.CreateWeaponItem((ushort)x)));
        backpack1.AddItem(backpack2);

        Enumerable.Range(1, 200).ToList().ForEach(x => backpack2.AddItem(ItemTestData.CreateWeaponItem((ushort)x)));

        //act
        var result = tradeSystem.Request(player, secondPlayer, backpack1);

        //assert
        result.Should().Be(SafeTradeError.MoreThan255Items);
    }

    [Fact]
    public void Trade_fails_when_item_is_already_being_traded()
    {
        //arrange
        var map = new Map(new World.World());
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build();
        var thirdPlayer = PlayerTestDataBuilder.Build();

        var item = ItemTestData.CreateWeaponItem(1);

        tradeSystem.Request(player, secondPlayer, item);

        //act
        var result = tradeSystem.Request(thirdPlayer, secondPlayer, item);

        //assert
        result.Should().Be(SafeTradeError.ItemAlreadyBeingTraded);
    }

    [Fact]
    public void Trade_fails_when_player_is_not_close_to_item()
    {
        //arrange
        var map = new Map(new World.World());
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var tile = MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var tile2 = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 102, 7));

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build();

        var item = ItemTestData.CreateWeaponItem(1);

        tile.AddItem(item);
        tile2.AddCreature(player);

        //act
        var result = tradeSystem.Request(player, secondPlayer, item);

        //assert
        result.Should().Be(SafeTradeError.PlayerNotCloseToItem);
    }

    [Fact]
    public void Trade_fails_when_player_is_not_close_enough_to_second_player()
    {
        //arrange
        var map = new Map(new World.World());
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var tile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var tile2 = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 103, 7));

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build(name: "Player2");

        var item = ItemTestData.CreateWeaponItem(1);

        tile.AddCreature(player);
        tile2.AddCreature(secondPlayer);

        //act
        var result = tradeSystem.Request(player, secondPlayer, item);

        //assert
        result.Should().Be(SafeTradeError.PlayersNotCloseToEachOther);
    }

    [Fact]
    public void Trade_fails_when_there_is_a_wall_in_middle_of_two_players()
    {
        //arrange
        var tile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var middleTile = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 101, 7));
        var tile2 = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 102, 7));

        var map = MapTestDataBuilder.Build(tile, middleTile, tile2);
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build(name: "Player2");

        var item = ItemTestData.CreateWeaponItem(1);
        var wall = ItemTestData.CreateTopItem(2, 1);

        wall.Metadata.Flags.Add(ItemFlag.Unpassable);
        wall.Metadata.Flags.Add(ItemFlag.BlockProjectTile);

        middleTile.AddItem(wall);

        tile.AddCreature(player);
        tile2.AddCreature(secondPlayer);

        //act
        var result = tradeSystem.Request(player, secondPlayer, item);

        //assert
        result.Should().Be(SafeTradeError.HasNoSightClearToPlayer);
    }

    [Fact]
    public void Trade_fails_when_second_player_is_already_trading()
    {
        //arrange
        var map = new Map(new World.World());
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build();
        var thirdPlayer = PlayerTestDataBuilder.Build();

        var item = ItemTestData.CreateWeaponItem(1);
        var item2 = ItemTestData.CreateWeaponItem(1);

        tradeSystem.Request(secondPlayer, thirdPlayer, item);

        //act
        var result = tradeSystem.Request(player, secondPlayer, item2);

        //assert
        result.Should().Be(SafeTradeError.SecondPlayerAlreadyTrading);
    }

    [Fact]
    public void Trade_fails_when_item_traded_is_not_pickupable()
    {
        //arrange
        var map = new Map(new World.World());
        var tradeSystem = new SafeTradeSystem(new TradeItemExchanger(new ItemRemoveService(map)), map);

        var player = PlayerTestDataBuilder.Build();
        var secondPlayer = PlayerTestDataBuilder.Build();

        var item = ItemTestData.CreateUnpassableItem(1);

        var error = string.Empty;
        OperationFailService.OnOperationFailed += (_, message, _) => error = message;

        //act
        var result = tradeSystem.Request(player, secondPlayer, item);

        //assert
        result.Should().Be(SafeTradeError.NonPickupableItem);
    }
}