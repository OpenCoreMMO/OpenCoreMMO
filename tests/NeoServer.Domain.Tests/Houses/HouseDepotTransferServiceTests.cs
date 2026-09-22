using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses.Services;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.Items.Items.Containers.Container;
using NeoServer.Domain.Locker;
using NeoServer.Domain.Repositories;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.House;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Models.Tiles;
using Serilog;

namespace NeoServer.Domain.Tests.Houses;

public class HouseDepotTransferServiceTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void HouseDepotTransfer_packs_items_into_the_shared_depot_when_the_house_has_no_town()
    {
        var sword = ItemTestDataBuilder.CreateWeaponItem(100, name: "sword");
        var axe = ItemTestDataBuilder.CreateWeaponItem(101, name: "axe");
        var tile = CreateTile();
        tile.AddItem(sword);
        tile.AddItem(axe);

        var house = HouseTestDataBuilder.Build(townId: 0, realTiles: [tile]);
        var existing = ItemTestDataBuilder.CreateWeaponItem(900, name: "existing");
        IContainer savedChest = null;

        var repository = CreateDepotRepository(
            onLoad: (chest, _, _) => chest.AddItem(existing),
            onSave: (_, chest) => savedChest = chest);
        var service = CreateService(repository.Object, backpackCapacity: 20);

        service.TransferToOwnerDepot(house, ownerId: 15);

        savedChest.Should().NotBeNull();
        savedChest.Items.Should().Contain(existing);
        var backpack = FindDirectBackpack(savedChest);
        backpack.Should().NotBeNull();
        backpack.Items.Should().Contain(sword);
        backpack.Items.Should().Contain(axe);
        ContainsBackpack(backpack.Items).Should().BeFalse();
        tile.AllItems.Should().NotContain(sword);
        tile.AllItems.Should().NotContain(axe);
        repository.Verify(x => x.Save(15u, savedChest), Times.Once);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void HouseDepotTransfer_adds_the_backpack_to_the_loaded_depot_when_the_locker_is_open()
    {
        var sword = ItemTestDataBuilder.CreateWeaponItem(100, name: "sword");
        var tile = CreateTile();
        tile.AddItem(sword);
        var house = HouseTestDataBuilder.Build(realTiles: [tile]);

        var existing = ItemTestDataBuilder.CreateWeaponItem(900, name: "existing");
        var (lockerManager, chest) = CreateLocker(playerId: 4, depotLoaded: true, existing);
        IContainer savedChest = null;
        var repository = CreateDepotRepository(onSave: (_, saved) => savedChest = saved);
        var service = CreateService(repository.Object, lockerManager);

        service.TransferToOwnerDepot(house, ownerId: 4);

        savedChest.Should().BeSameAs(chest);
        chest.Items.Should().Contain(existing);
        FindDirectBackpack(chest).Items.Should().Contain(sword);
        tile.AllItems.Should().NotContain(sword);
        repository.Verify(
            x => x.LoadDepotChest(It.IsAny<IContainer>(), It.IsAny<Location>(), It.IsAny<uint>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void HouseDepotTransfer_loads_the_live_depot_before_packing_when_it_is_not_loaded()
    {
        var sword = ItemTestDataBuilder.CreateWeaponItem(100, name: "sword");
        var tile = CreateTile();
        tile.AddItem(sword);
        var house = HouseTestDataBuilder.Build(realTiles: [tile]);

        var (lockerManager, chest) = CreateLocker(playerId: 4, depotLoaded: false);
        var repository = CreateDepotRepository();
        var service = CreateService(repository.Object, lockerManager);

        service.TransferToOwnerDepot(house, ownerId: 4);

        repository.Verify(x => x.LoadDepotChest(chest, chest.Location, 4u), Times.Once);
        lockerManager.IsDepotLoaded(4).Should().BeTrue();
        FindDirectBackpack(chest).Items.Should().Contain(sword);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void HouseDepotTransfer_nests_backpacks_when_the_backpack_is_full()
    {
        var items = new List<IItem>();
        var tile = CreateTile();
        for (ushort id = 100; id < 105; id++)
        {
            var weapon = ItemTestDataBuilder.CreateWeaponItem(id, name: $"weapon {id}");
            items.Add(weapon);
            tile.AddItem(weapon);
        }

        var house = HouseTestDataBuilder.Build(realTiles: [tile]);
        IContainer savedChest = null;
        var repository = CreateDepotRepository(onSave: (_, chest) => savedChest = chest);
        var service = CreateService(repository.Object, backpackCapacity: 2);

        service.TransferToOwnerDepot(house, ownerId: 8);

        var backpack = FindDirectBackpack(savedChest);
        backpack.Should().NotBeNull();
        ContainsBackpack(backpack.Items).Should().BeTrue();

        var stored = Flatten(backpack);
        foreach (var item in items)
        {
            stored.Should().Contain(item);
            tile.AllItems.Should().NotContain(item);
        }
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void HouseDepotTransfer_keeps_items_inside_pickupable_containers()
    {
        var coin = ItemTestDataBuilder.CreateWeaponItem(300, name: "coin");
        var houseBackpack = ItemTestDataBuilder.CreateBackpack(id: 50);
        houseBackpack.AddItem(coin);

        var tile = CreateTile();
        tile.AddItem(houseBackpack);
        var house = HouseTestDataBuilder.Build(realTiles: [tile]);

        IContainer savedChest = null;
        var repository = CreateDepotRepository(onSave: (_, chest) => savedChest = chest);
        var service = CreateService(repository.Object);

        service.TransferToOwnerDepot(house, ownerId: 3);

        var depotBackpack = FindDirectBackpack(savedChest);
        depotBackpack.Items.Should().Contain(houseBackpack);
        houseBackpack.Items.Should().Contain(coin);
        tile.AllItems.Should().NotContain(houseBackpack);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void HouseDepotTransfer_moves_contents_of_non_pickupable_containers_and_leaves_the_container()
    {
        var furnitureContent = ItemTestDataBuilder.CreateWeaponItem(300, name: "furniture content");
        var furniture = CreateContainer(id: 2000, capacity: 10, pickupable: false);
        furniture.AddItem(furnitureContent);

        var loose = ItemTestDataBuilder.CreateWeaponItem(301, name: "loose");
        var decoration = ItemTestDataBuilder.CreateMoveableItem(302);

        var tile = CreateTile();
        tile.AddItem(furniture);
        tile.AddItem(loose);
        tile.AddItem(decoration);

        var house = HouseTestDataBuilder.Build(realTiles: [tile]);
        IContainer savedChest = null;
        var repository = CreateDepotRepository(onSave: (_, chest) => savedChest = chest);
        var service = CreateService(repository.Object);

        service.TransferToOwnerDepot(house, ownerId: 6);

        var backpack = FindDirectBackpack(savedChest);
        backpack.Items.Should().Contain(furnitureContent);
        backpack.Items.Should().Contain(loose);
        backpack.Items.Should().NotContain(furniture);
        backpack.Items.Should().NotContain(decoration);
        furniture.Items.Should().NotContain(furnitureContent);
        tile.AllItems.Should().Contain(furniture);
        tile.AllItems.Should().Contain(decoration);
        tile.AllItems.Should().NotContain(loose);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void HouseDepotTransfer_places_the_backpack_inside_an_existing_container_when_the_chest_is_full()
    {
        var sword = ItemTestDataBuilder.CreateWeaponItem(100, name: "sword");
        var tile = CreateTile();
        tile.AddItem(sword);
        var house = HouseTestDataBuilder.Build(realTiles: [tile]);

        var inner = ItemTestDataBuilder.CreateBackpack(id: 60);
        var (lockerManager, chest) = CreateLocker(playerId: 4, depotLoaded: true, chestCapacity: 1);
        chest.AddItem(inner);

        var repository = CreateDepotRepository();
        var service = CreateService(repository.Object, lockerManager);

        service.TransferToOwnerDepot(house, ownerId: 4);

        var backpack = FindDirectBackpack(inner);
        backpack.Should().NotBeNull();
        backpack.Items.Should().Contain(sword);
        chest.Items.Should().Contain(inner);
        ContainsBackpack(chest.Items).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void HouseDepotTransfer_leaves_house_items_when_the_depot_has_no_free_slot()
    {
        var sword = ItemTestDataBuilder.CreateWeaponItem(100, name: "sword");
        var tile = CreateTile();
        tile.AddItem(sword);
        var house = HouseTestDataBuilder.Build(realTiles: [tile]);

        var blocker = ItemTestDataBuilder.CreateWeaponItem(900, name: "blocker");
        var (lockerManager, chest) = CreateLocker(playerId: 4, depotLoaded: true, blocker, chestCapacity: 1);
        var repository = CreateDepotRepository();
        var service = CreateService(repository.Object, lockerManager);

        service.TransferToOwnerDepot(house, ownerId: 4);

        tile.AllItems.Should().Contain(sword);
        chest.Items.Should().Contain(blocker);
        chest.Items.Should().HaveCount(1);
        repository.Verify(x => x.Save(It.IsAny<uint>(), It.IsAny<IContainer>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void HouseDepotTransfer_does_nothing_when_the_house_has_no_transferable_items()
    {
        var decoration = ItemTestDataBuilder.CreateMoveableItem(302);
        var tile = CreateTile();
        tile.AddItem(decoration);
        var house = HouseTestDataBuilder.Build(realTiles: [tile]);

        var repository = CreateDepotRepository();
        var service = CreateService(repository.Object);

        service.TransferToOwnerDepot(house, ownerId: 1);

        tile.AllItems.Should().Contain(decoration);
        repository.Verify(x => x.Save(It.IsAny<uint>(), It.IsAny<IContainer>()), Times.Never);
        repository.Verify(
            x => x.LoadDepotChest(It.IsAny<IContainer>(), It.IsAny<Location>(), It.IsAny<uint>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void HouseDepotTransfer_does_nothing_when_the_owner_id_is_zero()
    {
        var sword = ItemTestDataBuilder.CreateWeaponItem(100, name: "sword");
        var tile = CreateTile();
        tile.AddItem(sword);
        var house = HouseTestDataBuilder.Build(realTiles: [tile]);
        var repository = CreateDepotRepository();
        var service = CreateService(repository.Object);

        service.TransferToOwnerDepot(house, ownerId: 0);

        tile.AllItems.Should().Contain(sword);
        repository.Verify(x => x.Save(It.IsAny<uint>(), It.IsAny<IContainer>()), Times.Never);
    }

    private static HouseDepotTransferService CreateService(
        IPlayerDepotRepository repository,
        LockerManager lockerManager = null,
        byte backpackCapacity = 20)
    {
        return new HouseDepotTransferService(
            lockerManager ?? new LockerManager(),
            repository,
            CreateItemFactory(backpackCapacity),
            new Mock<ILogger>().Object);
    }

    private static IItemFactory CreateItemFactory(byte backpackCapacity)
    {
        var backpackType = CreateContainerType(GameConstants.BACKPACK_SERVER_ID, backpackCapacity);
        var depotType = CreateContainerType(GameConstants.DEPOT_CHEST_SERVER_ID, 30);
        var itemTypeStore = ItemTypeStoreTestBuilder.Build(backpackType, depotType);

        return new ItemFactory(
            null,
            null,
            null,
            new ContainerFactory(),
            null,
            null,
            null,
            null,
            itemTypeStore,
            null);
    }

    private static ItemType CreateContainerType(ushort id, byte capacity)
    {
        var itemType = new ItemType();
        itemType.SetId(id);
        itemType.SetClientId(id);
        itemType.SetName("container");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Capacity, capacity);
        itemType.SetGroup((byte)ItemGroup.Container);
        return itemType;
    }

    private static Container CreateContainer(ushort id, byte capacity, bool pickupable)
    {
        var itemType = CreateContainerType(id, capacity);
        if (pickupable)
        {
            itemType.Flags.Add(ItemFlag.Pickupable);
            itemType.Flags.Add(ItemFlag.Movable);
        }

        return new Container(itemType, Location.Zero);
    }

    private static (LockerManager Manager, Container Chest) CreateLocker(
        uint playerId,
        bool depotLoaded,
        IItem existingItem = null,
        byte chestCapacity = 30)
    {
        var lockerManager = new LockerManager();
        var locker = ItemTestDataBuilder.CreateLocker();
        var mail = ItemTestDataBuilder.CreateMailInbox();
        var chest = CreateContainer(GameConstants.DEPOT_CHEST_SERVER_ID, chestCapacity, pickupable: false);

        if (existingItem is not null)
            chest.AddItem(existingItem);

        locker.AddItem(mail);
        locker.AddItem(chest);
        lockerManager.Load(playerId, locker);

        if (depotLoaded)
            lockerManager.SetDepotAsLoaded(playerId);

        return (lockerManager, chest);
    }

    private static Mock<IPlayerDepotRepository> CreateDepotRepository(
        Action<IContainer, Location, uint> onLoad = null,
        Action<uint, IContainer> onSave = null)
    {
        var repository = new Mock<IPlayerDepotRepository>();
        repository
            .Setup(x => x.LoadDepotChest(It.IsAny<IContainer>(), It.IsAny<Location>(), It.IsAny<uint>()))
            .Callback(onLoad ?? ((_, _, _) => { }))
            .Returns(Task.CompletedTask);
        repository
            .Setup(x => x.Save(It.IsAny<uint>(), It.IsAny<IContainer>()))
            .Callback(onSave ?? ((_, _) => { }))
            .Returns(Task.CompletedTask);
        return repository;
    }

    private static DynamicTile CreateTile()
    {
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);
        return new DynamicTile(new Coordinate(location), TileFlag.None, ground, null, null);
    }

    private static bool ContainsBackpack(List<IItem> items)
    {
        foreach (var item in items)
        {
            if (item is IContainer container && container.ServerId == GameConstants.BACKPACK_SERVER_ID)
                return true;
        }

        return false;
    }

    private static IContainer FindDirectBackpack(IContainer container)
    {
        foreach (var item in container.Items)
        {
            if (item is IContainer child && child.ServerId == GameConstants.BACKPACK_SERVER_ID)
                return child;
        }

        return null;
    }

    private static List<IItem> Flatten(IContainer container)
    {
        var items = new List<IItem>();
        var pending = new Stack<IContainer>();
        pending.Push(container);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            foreach (var item in current.Items)
            {
                items.Add(item);
                if (item is IContainer child)
                    pending.Push(child);
            }
        }

        return items;
    }
}
