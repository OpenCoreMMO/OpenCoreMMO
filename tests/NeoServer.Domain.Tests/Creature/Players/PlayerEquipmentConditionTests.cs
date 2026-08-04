using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerEquipmentConditionTests
{
    [Fact]
    public void Player_gets_magic_shield_condition_when_equipping_ring_with_mana_shield()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithMagicShield = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.ManaShield, 1)
            ]);

        player.Inventory[Slot.Ring].Should().BeNull();
        player.IsManaShieldEnabled.Should().BeFalse();

        // act
        var result = player.Inventory.AddItem(ringWithMagicShield, Slot.Ring);

        // assert
        result.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();
    }

    [Fact]
    public void Player_loses_magic_shield_condition_when_removing_ring_with_mana_shield()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithMagicShield = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.ManaShield, 1)
            ]);

        // act
        var equipResult = player.Inventory.AddItem(ringWithMagicShield, Slot.Ring);

        // assert
        equipResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        var removeResult = player.Inventory.RemoveItem(Slot.Ring, 1);

        // assert
        removeResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeFalse();
    }

    [Fact]
    [ThreadBlocking]
    public void Player_keeps_magic_shield_until_base_500ms_condition_expires_after_unequipping_mana_shield_ring()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithMagicShield = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.ManaShield, 1)
            ]);

        player.Inventory[Slot.Ring].Should().BeNull();

        player.AddCondition(new Condition(ConditionType.ManaShield, 500));

        // assert
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        var equipResult = player.Inventory.AddItem(ringWithMagicShield, Slot.Ring);

        // assert
        equipResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        Thread.Sleep(200);
        var removeResult = player.Inventory.RemoveItem(Slot.Ring, 1);

        // assert
        removeResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        Thread.Sleep(320);
        ExecuteConditionTick(player);

        // assert
        player.IsManaShieldEnabled.Should().BeFalse();
    }

    [Fact]
    [ThreadBlocking]
    public void Player_keeps_magic_shield_after_200ms_when_ring_is_equipped_after_100ms_and_loses_it_when_ring_is_removed()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithMagicShield = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.ManaShield, 1)
            ]);

        player.Inventory[Slot.Ring].Should().BeNull();
        player.AddCondition(new Condition(ConditionType.ManaShield, 200));

        // assert
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        Thread.Sleep(100);
        var equipResult = player.Inventory.AddItem(ringWithMagicShield, Slot.Ring);

        // assert
        equipResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        Thread.Sleep(200);
        ExecuteConditionTick(player);

        // assert
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        var removeResult = player.Inventory.RemoveItem(Slot.Ring, 1);

        // assert
        removeResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeFalse();
    }

    [Fact]
    [ThreadBlocking]
    public void Player_keeps_magic_shield_after_200ms_when_timed_condition_is_added_to_already_equipped_mana_shield_ring_and_loses_it_on_ring_removal()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithMagicShield = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.ManaShield, 1)
            ]);

        // act
        var equipResult = player.Inventory.AddItem(ringWithMagicShield, Slot.Ring);

        // assert
        equipResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        player.AddCondition(new Condition(ConditionType.ManaShield, 200));

        // assert
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        Thread.Sleep(220);
        ExecuteConditionTick(player);

        // assert
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        var removeResult = player.Inventory.RemoveItem(Slot.Ring, 1);

        // assert
        removeResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeFalse();
    }

    [Fact]
    [ThreadBlocking]
    public void Player_keeps_magic_shield_for_remaining_duration_after_removing_ring_at_100ms_and_loses_it_after_expiration()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithMagicShield = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.ManaShield, 1)
            ]);

        // act
        var equipResult = player.Inventory.AddItem(ringWithMagicShield, Slot.Ring);

        // assert
        equipResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        player.AddCondition(new Condition(ConditionType.ManaShield, 200));

        // assert
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        Thread.Sleep(100);
        var removeResult = player.Inventory.RemoveItem(Slot.Ring, 1);

        // assert
        removeResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        Thread.Sleep(120);
        ExecuteConditionTick(player);

        // assert
        player.IsManaShieldEnabled.Should().BeFalse();
    }

    [Fact]
    [ThreadBlocking]
    public void Player_keeps_magic_shield_after_timed_condition_expires_and_loses_it_only_after_ring_and_boots_are_removed()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithMagicShield = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.ManaShield, 1)
            ]);
        var bootsWithMagicShield = ItemTestDataBuilder.CreateBodyEquipmentItem(2647, "feet",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.ManaShield, 1)
            ]);

        player.Inventory[Slot.Ring].Should().BeNull();
        player.Inventory[Slot.Feet].Should().BeNull();
        player.IsManaShieldEnabled.Should().BeFalse();

        // act
        var equipRingResult = player.Inventory.AddItem(ringWithMagicShield, Slot.Ring);

        // assert
        equipRingResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        var equipBootsResult = player.Inventory.AddItem(bootsWithMagicShield, Slot.Feet);

        // assert
        equipBootsResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        player.AddCondition(new Condition(ConditionType.ManaShield, 200));

        // assert
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        Thread.Sleep(220);
        ExecuteConditionTick(player);

        // assert
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        var removeRingResult = player.Inventory.RemoveItem(Slot.Ring, 1);

        // assert
        removeRingResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeTrue();

        // act
        var removeBootsResult = player.Inventory.RemoveItem(Slot.Feet, 1);

        // assert
        removeBootsResult.Succeeded.Should().BeTrue();
        player.IsManaShieldEnabled.Should().BeFalse();
    }

    private static void ExecuteConditionTick(ICombatActor creature)
    {
        var conditions = creature.GetConditions();
        foreach (var condition in conditions)
        {
            if (!condition.HasExpired) continue;

            creature.RemoveCondition(condition);
        }
    }
}