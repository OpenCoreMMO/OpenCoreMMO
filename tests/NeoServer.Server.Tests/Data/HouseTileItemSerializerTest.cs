using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NeoServer.Data.Serializers;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.Items.Factories.AttributeFactory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Server;
using Xunit;

namespace NeoServer.Server.Tests.Data;

public class HouseTileItemSerializerTest
{
    [Fact]
    public void HouseTileItemSerializer_round_trips_tile_with_sword()
    {
        // Arrange
        const ushort swordServerId = 2376;
        var location = new Location(100, 100, 7);

        var sword = ItemTestDataBuilder.CreateWeaponItem(swordServerId, name: "sword");

        // Ensure the item type's group is set so the real factory can recreate it
        var swordType = (ItemType)sword.Metadata;
        swordType.SetGroup((byte)ItemGroup.MeleeWeapon);

        var itemTypeStore = ItemTypeStoreTestBuilder.Build(sword.Metadata);
        var chargeableFactory = new ChargeableFactory();
        var factory = new ItemFactory(
            null,
            null,
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null,
            null,
            null,
            null,
            new GenericItemFactory(),
            itemTypeStore,
            null);

        var items = new List<IItem> { sword };

        // Act
        var serialized = HouseTileItemSerializer.Serialize(items);
        var deserialized = HouseTileItemSerializer.Deserialize(serialized, factory, location);

        // Assert
        deserialized.Should().HaveCount(1);
        deserialized[0].ServerId.Should().Be(swordServerId);
    }

    [Fact]
    public void HouseTileItemSerializer_preserves_item_amount_when_serializing_and_deserializing()
    {
        // Arrange
        const ushort itemServerId = 3031;
        const byte expectedAmount = 25;
        var location = new Location(100, 100, 7);

        var item = ItemTestDataBuilder.CreateCumulativeItem(itemServerId, amount: expectedAmount, name: "coin");

        var itemType = (ItemType)item.Metadata;
        itemType.SetGroup((byte)ItemGroup.Cumulative);

        var itemTypeStore = ItemTypeStoreTestBuilder.Build(item.Metadata);
        var chargeableFactory = new ChargeableFactory();
        var factory = new ItemFactory(
            null,
            null,
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null,
            null,
            null,
            new CumulativeFactory(),
            new GenericItemFactory(),
            itemTypeStore,
            null);

        var items = new List<IItem> { item };

        // Act
        var serialized = HouseTileItemSerializer.Serialize(items);
        var deserialized = HouseTileItemSerializer.Deserialize(serialized, factory, location);

        // Assert
        deserialized.Should().HaveCount(1);
        deserialized[0].Amount.Should().Be(expectedAmount);
    }

    [Fact]
    public void HouseTileItemSerializer_preserves_item_charges_when_serializing_and_deserializing()
    {
        // Arrange
        const ushort itemServerId = 2000;
        const ushort expectedCharges = 50;
        var location = new Location(100, 100, 7);

        var item = ItemTestDataBuilder.CreateRegularItem(itemServerId,
            itemAttributes:
            [
                (ItemAttribute.Count, 1),
                (ItemAttribute.Charges, expectedCharges)
            ]);

        var itemTypeStore = ItemTypeStoreTestBuilder.Build(item.Metadata);
        var factory = new ItemFactory(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new GenericItemFactory(),
            itemTypeStore,
            null);

        var items = new List<IItem> { item };

        // Act
        var serialized = HouseTileItemSerializer.Serialize(items);
        var deserialized = HouseTileItemSerializer.Deserialize(serialized, factory, location);

        // Assert
        deserialized.Should().HaveCount(1);
        deserialized[0].Attributes.GetAttribute<ushort>(ItemAttribute.Charges).Should().Be(expectedCharges);
    }

    [Fact]
    public void HouseTileItemSerializer_preserves_decay_attributes_when_serializing_and_deserializing()
    {
        // Arrange
        const ushort itemServerId = 2001;
        const ushort expectedDecayTo = 2002;
        const uint expectedDuration = 30_000;
        const uint expectedElapsed = 5_000;
        var location = new Location(100, 100, 7);

        var item = ItemTestDataBuilder.CreateRegularItem(itemServerId,
            itemAttributes:
            [
                (ItemAttribute.Count, 1),
                (ItemAttribute.DecayTo, expectedDecayTo),
                (ItemAttribute.Duration, expectedDuration),
                (ItemAttribute.DecayState, expectedElapsed)
            ]);

        var itemTypeStore = ItemTypeStoreTestBuilder.Build(item.Metadata);
        var factory = new ItemFactory(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new GenericItemFactory(),
            itemTypeStore,
            null);

        var items = new List<IItem> { item };

        // Act
        var serialized = HouseTileItemSerializer.Serialize(items);
        var deserialized = HouseTileItemSerializer.Deserialize(serialized, factory, location);

        // Assert
        deserialized.Should().HaveCount(1);
        var attrs = deserialized[0].Attributes;
        attrs.GetAttribute<ushort>(ItemAttribute.DecayTo).Should().Be(expectedDecayTo);
        attrs.GetAttribute<uint>(ItemAttribute.Duration).Should().Be(expectedDuration);
        attrs.GetAttribute<uint>(ItemAttribute.DecayState).Should().Be(expectedElapsed);
    }
}
