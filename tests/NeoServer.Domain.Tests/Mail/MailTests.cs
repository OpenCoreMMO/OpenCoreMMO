using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.Containers;
using NeoServer.Domain.Locker;
using NeoServer.Domain.Mail;
using NeoServer.Domain.Repositories;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Mail;

public class MailTests
{
    [Fact]
    public void Mail_is_sent_when_parcel_is_valid()
    {
        //arrange

        //create player
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        //create label
        var label = new Label(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LABEL_SERVER_ID),
            Location.Zero);
        label.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Writeable, 1);
        label.Metadata.Attributes.SetAttribute(ItemTypeAttribute.MaxLength, 100);

        label.Write("Recipient", player);

        //create parcel
        var parcel = ItemTestDataBuilder.CreateParcel(id: GameConstants.PARCEL_SERVER_ID);
        parcel.AddItem(label);

        //add parcel to player backpack
        player.Inventory.BackpackSlot.AddItem(parcel);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(1);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(1)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        //act
        var result = service.Move(player, parcel, from.Object, mailboxTile, 1, 0, null, false);

        //result
        result.Succeeded.Should().BeTrue();
        parcel.Metadata.ServerId.Should().Be(GameConstants.STAMPED_PARCEL_SERVER_ID); // New ID assigned when sent
        mailRepository.Verify(r => r.AddParcelToInbox(1, parcel), Times.Once);
    }

    [Fact]
    public void Mail_is_sent_when_letter_is_valid()
    {
        //arrange

        //create player
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        //create label
        var letter = new Letter(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LETTER_SERVER_ID),
            Location.Zero);
        letter.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Writeable, 1);
        letter.Metadata.Attributes.SetAttribute(ItemTypeAttribute.MaxLength, 100);

        letter.Write("Recipient", player);

        //add parcel to player backpack
        player.Inventory.BackpackSlot.AddItem(letter);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(1);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(1)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        //act
        var result = service.Move(player, letter, from.Object, mailboxTile, 1, 0, null, false);

        //result
        result.Succeeded.Should().BeTrue();
        letter.Metadata.ServerId.Should().Be(GameConstants.STAMPED_LETTER_SERVER_ID); // New ID assigned when sent
        mailRepository.Verify(r => r.AddLetterToInbox(1, letter), Times.Once);
    }

    [Fact]
    public void Mail_is_not_sent_when_item_is_not_mailable()
    {
        //arrange

        //create player
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        //create label
        var weapon = ItemTestDataBuilder.CreateWeaponItem(1);

        //add parcel to player backpack
        player.Inventory.BackpackSlot.AddItem(weapon);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(1);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(1)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        //act
        var result = service.Move(player, weapon, from.Object, mailboxTile, 1, 0, null, false);

        //result
        result.Succeeded.Should().BeFalse();
        mailRepository.Verify(r => r.AddLetterToInbox(1, It.IsAny<Letter>()), Times.Never);
        mailRepository.Verify(r => r.AddParcelToInbox(1, It.IsAny<Parcel>()), Times.Never);
    }

    [Fact]
    public void Mail_is_not_sent_when_parcel_has_no_label()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create parcel without label
        var parcel = ItemTestDataBuilder.CreateParcel(id: GameConstants.PARCEL_SERVER_ID);

        // add parcel to player backpack
        player.Inventory.BackpackSlot.AddItem(parcel);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(1);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(1)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, parcel, player.Inventory.BackpackSlot, mailboxTile, 1, 0, null, false);

        // result
        result.Succeeded.Should().BeTrue(); //the parcel is moved to above mailbox but not sent
        mailboxTile.TopDownItemOnStack.Should().Be(parcel);

        mailRepository.Verify(r => r.AddParcelToInbox(1, parcel), Times.Never);
        lockerManager.Get(1).Should().BeNull();
    }

    [Fact]
    public void Mail_is_not_sent_when_parcel_has_multiple_labels()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create parcel with two labels
        var parcel = ItemTestDataBuilder.CreateParcel(id: GameConstants.PARCEL_SERVER_ID);
        var label1 = new Label(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LABEL_SERVER_ID),
            Location.Zero);
        var label2 = new Label(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LABEL_SERVER_ID),
            Location.Zero);
        label1.Write("Recipient1", player);
        label2.Write("Recipient2", player);
        parcel.AddItem(label1);
        parcel.AddItem(label2);

        // add parcel to player backpack
        player.Inventory.BackpackSlot.AddItem(parcel);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient1")).Returns(1);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(1)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, parcel, player.Inventory.BackpackSlot, mailboxTile, 1, 0, null, false);

        // result
        result.Succeeded.Should().BeTrue(); // the parcel is moved to above mailbox but not sent
        mailboxTile.TopDownItemOnStack.Should().Be(parcel);

        mailRepository.Verify(r => r.AddParcelToInbox(1, parcel), Times.Never);
        lockerManager.Get(1).Should().BeNull();
    }


    [Fact]
    public void Mail_is_not_sent_when_parcel_recipient_does_not_exist()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create parcel with one label for a non-existent recipient
        var parcel = ItemTestDataBuilder.CreateParcel(id: GameConstants.PARCEL_SERVER_ID);
        var label = new Label(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LABEL_SERVER_ID),
            Location.Zero);
        label.Write("UnknownRecipient", player);
        parcel.AddItem(label);

        // add parcel to player backpack
        player.Inventory.BackpackSlot.AddItem(parcel);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("UnknownRecipient")).Returns(0);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(0)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, parcel, player.Inventory.BackpackSlot, mailboxTile, 1, 0, null, false);

        // result
        result.Succeeded.Should().BeTrue(); // the parcel is moved to above mailbox but not sent
        mailboxTile.TopDownItemOnStack.Should().Be(parcel);

        mailRepository.Verify(r => r.AddParcelToInbox(0, parcel), Times.Never);
        lockerManager.Get(0).Should().BeNull();
    }

    [Fact]
    public void Mail_is_not_sent_when_destination_mail_inbox_is_full()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create parcel with label
        var parcel = ItemTestDataBuilder.CreateParcel(id: GameConstants.PARCEL_SERVER_ID);
        var label = new Label(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LABEL_SERVER_ID),
            Location.Zero);
        label.Write("Recipient", player);
        parcel.AddItem(label);

        // add parcel to player backpack
        player.Inventory.BackpackSlot.AddItem(parcel);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(1);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(1)).Returns(GameConstants.MAX_NUMBER_OF_ITEMS_ON_INBOX);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, parcel, player.Inventory.BackpackSlot, mailboxTile, 1, 0, null, false);

        // result
        result.Succeeded.Should().BeTrue(); // the parcel is moved to above mailbox but not sent
        mailboxTile.TopDownItemOnStack.Should().Be(parcel);

        mailRepository.Verify(r => r.AddParcelToInbox(1, parcel), Times.Never);
        lockerManager.Get(1).Should().BeNull();
    }

    [Fact]
    public void Letter_is_not_sent_when_has_no_destination()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create letter without destination
        var letter = new Letter(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LETTER_SERVER_ID),
            Location.Zero);

        // add letter to player backpack
        player.Inventory.BackpackSlot.AddItem(letter);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName(It.IsAny<string>())).Returns(0);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(0)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, letter, player.Inventory.BackpackSlot, mailboxTile, 1, 0, null, false);

        // result
        result.Succeeded.Should().BeTrue(); // the letter is moved to above mailbox but not sent
        mailboxTile.TopDownItemOnStack.Should().Be(letter);

        mailRepository.Verify(r => r.AddLetterToInbox(0, letter), Times.Never);
        lockerManager.Get(0).Should().BeNull();
    }

    [Fact]
    public void Letter_is_not_sent_when_recipient_is_does_not_exist()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create letter with non-existent recipient
        var letter = new Letter(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LETTER_SERVER_ID),
            Location.Zero);
        letter.Write("UnknownRecipient", player);

        // add letter to player backpack
        player.Inventory.BackpackSlot.AddItem(letter);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("UnknownRecipient")).Returns(0);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(0)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, letter, player.Inventory.BackpackSlot, mailboxTile, 1, 0, null, false);

        // result
        result.Succeeded.Should().BeTrue(); // the letter is moved to above mailbox but not sent
        mailboxTile.TopDownItemOnStack.Should().Be(letter);

        mailRepository.Verify(r => r.AddLetterToInbox(0, letter), Times.Never);
        lockerManager.Get(0).Should().BeNull();
    }

    [Fact]
    public void Letter_is_not_sent_when_destination_mail_inbox_is_full()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create letter with label
        var letter = new Letter(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LETTER_SERVER_ID),
            Location.Zero);
        letter.Write("Recipient", player);

        // add letter to player backpack
        player.Inventory.BackpackSlot.AddItem(letter);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(1);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(1)).Returns(GameConstants.MAX_NUMBER_OF_ITEMS_ON_INBOX);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, letter, player.Inventory.BackpackSlot, mailboxTile, 1, 0, null, false);

        // result
        result.Succeeded.Should().BeTrue(); // the letter is moved to above mailbox but not sent
        mailboxTile.TopDownItemOnStack.Should().Be(letter);

        mailRepository.Verify(r => r.AddLetterToInbox(1, letter), Times.Never);
        lockerManager.Get(1).Should().BeNull();
    }

    [Fact]
    public void Mail_is_sent_to_locker_mailbox_when_parcel_is_valid()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create label
        var label = new Label(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LABEL_SERVER_ID),
            Location.Zero);
        label.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Writeable, 1);
        label.Metadata.Attributes.SetAttribute(ItemTypeAttribute.MaxLength, 100);
        label.Write("Recipient", player);

        // create parcel
        var parcel = ItemTestDataBuilder.CreateParcel(id: GameConstants.PARCEL_SERVER_ID);
        parcel.AddItem(label);

        // add parcel to player backpack
        player.Inventory.BackpackSlot.AddItem(parcel);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(6);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(6)).Returns(0);

        // create real locker manager and locker
        var lockerManager = new LockerManager();
        var locker = ItemTestDataBuilder.CreateLocker();
        var mailInbox = ItemTestDataBuilder.CreateMailInbox();
        locker.Items.Add(null); // index 0
        locker.Items.Add(mailInbox); // index 1
        lockerManager.Load(6, locker);

        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, parcel, from.Object, mailboxTile, 1, 0, null, false);

        // assert
        mailInbox.Items.Should().Contain(parcel);
        result.Succeeded.Should().BeTrue();
        parcel.Metadata.ServerId.Should().Be(GameConstants.STAMPED_PARCEL_SERVER_ID);
    }

    [Fact]
    public void Mail_is_sent_to_locker_mailbox_when_letter_is_valid()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create letter
        var letter = new Letter(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.LETTER_SERVER_ID),
            Location.Zero);
        letter.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Writeable, 1);
        letter.Metadata.Attributes.SetAttribute(ItemTypeAttribute.MaxLength, 100);
        letter.Write("Recipient", player);

        // add letter to player backpack
        player.Inventory.BackpackSlot.AddItem(letter);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(6);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(6)).Returns(0);

        // create real locker manager and locker
        var lockerManager = new LockerManager();
        var locker = ItemTestDataBuilder.CreateLocker();
        var mailInbox = ItemTestDataBuilder.CreateMailInbox();
        locker.Items.Add(null); // index 0
        locker.Items.Add(mailInbox); // index 1
        lockerManager.Load(6, locker);

        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, letter, from.Object, mailboxTile, 1, 0, null, false);

        // assert
        mailInbox.Items.Should().Contain(letter);
        result.Succeeded.Should().BeTrue();
        letter.Metadata.ServerId.Should().Be(GameConstants.STAMPED_LETTER_SERVER_ID);
    }

    [Fact]
    public void Stamped_parcel_cannot_be_sent()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create stamped parcel
        var parcel = ItemTestDataBuilder.CreateParcel(id: GameConstants.STAMPED_PARCEL_SERVER_ID);

        // add parcel to player backpack
        player.Inventory.BackpackSlot.AddItem(parcel);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(1);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(1)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, parcel, player.Inventory.BackpackSlot, mailboxTile, 1, 0, null, false);

        // assert
        result.Succeeded.Should().BeTrue(); // the parcel is moved to above mailbox but not sent
        mailboxTile.TopDownItemOnStack.Should().Be(parcel);
        mailRepository.Verify(r => r.AddParcelToInbox(1, parcel), Times.Never);
        lockerManager.Get(1).Should().BeNull();
    }

    [Fact]
    public void Stamped_letter_cannot_be_sent()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        player.Inventory.AddItem(ItemTestDataBuilder.CreateBackpack(), Slot.Backpack);

        // create stamped letter
        var letter = new Letter(ItemTestDataBuilder.CreateMoveableItemMetadata(GameConstants.STAMPED_LETTER_SERVER_ID),
            Location.Zero);

        // add letter to player backpack
        player.Inventory.BackpackSlot.AddItem(letter);
        player.SetNewLocation(new Location(100, 100, 7));

        var from = new Mock<IDynamicTile>();
        from.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        var mailBoxMetadata = new ItemType().SetClientId(1);
        mailBoxMetadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "mailbox");
        var mailBox = new Item(mailBoxMetadata, new Location(100, 101, 7));

        var mailboxTile = new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, null, [mailBox]);

        var playerRepository = new Mock<IPlayerRepository>();
        playerRepository.Setup(r => r.GetIdByName("Recipient")).Returns(1);

        var mailRepository = new Mock<IPlayerMailRepository>();
        mailRepository.Setup(r => r.GetInboxItemCount(1)).Returns(0);

        var lockerManager = new LockerManager();
        var itemTypeStore =
            ItemTypeStoreTestBuilder.Build(
                new ItemType().SetId(GameConstants.STAMPED_PARCEL_SERVER_ID),
                new ItemType().SetId(GameConstants.STAMPED_LETTER_SERVER_ID)
            );

        var mailService = new MailService(playerRepository.Object, mailRepository.Object, lockerManager, itemTypeStore);

        var walkTo = new Mock<IWalkToMechanism>().Object;
        var service = new ItemMovementService(walkTo, mailService);

        // act
        var result = service.Move(player, letter, player.Inventory.BackpackSlot, mailboxTile, 1, 0, null, false);

        // assert
        result.Succeeded.Should().BeTrue(); // the letter is moved to above mailbox but not sent
        mailboxTile.TopDownItemOnStack.Should().Be(letter);
        mailRepository.Verify(r => r.AddLetterToInbox(1, letter), Times.Never);
        lockerManager.Get(1).Should().BeNull();
    }
}