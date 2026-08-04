using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerEquipmentSuppressConditionTest
{
    [Fact]
    public void Player_loses_drunk_condition_when_equipping_ring_with_suppress_drunk_attribute()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithSuppressDrunk = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.SuppressDrunk, true)
            ]);

        player.AddCondition(new Condition(ConditionType.Drunk, 500));

        player.Inventory[Slot.Ring].Should().BeNull();
        player.HasCondition(ConditionType.Drunk).Should().BeTrue();

        // act
        var result = player.Inventory.AddItem(ringWithSuppressDrunk, Slot.Ring);

        // assert
        result.Succeeded.Should().BeTrue();
        player.HasCondition(ConditionType.Drunk).Should().BeFalse();
    }

    [Fact]
    public void Player_gets_drunk_condition_back_when_removing_ring_with_suppress_drunk_attribute()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithSuppressDrunk = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.SuppressDrunk, true)
            ]);

        player.AddCondition(new Condition(ConditionType.Drunk, 5000));

        player.HasCondition(ConditionType.Drunk).Should().BeTrue();

        // act
        var equipResult = player.Inventory.AddItem(ringWithSuppressDrunk, Slot.Ring);

        // assert
        equipResult.Succeeded.Should().BeTrue();
        player.HasCondition(ConditionType.Drunk).Should().BeFalse();

        // act
        var removeResult = player.Inventory.RemoveItem(Slot.Ring, 1);

        // assert
        removeResult.Succeeded.Should().BeTrue();
        player.HasCondition(ConditionType.Drunk).Should().BeTrue();
    }

    [Fact]
    [ThreadBlocking]
    public void Player_loses_drunk_condition_after_500ms_even_when_ring_with_suppress_drunk_is_removed_after_200ms()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ringWithSuppressDrunk = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.SuppressDrunk, true)
            ]);

        player.AddCondition(new Condition(ConditionType.Drunk, 500));

        player.HasCondition(ConditionType.Drunk).Should().BeTrue();

        // act
        var equipResult = player.Inventory.AddItem(ringWithSuppressDrunk, Slot.Ring);

        // assert
        equipResult.Succeeded.Should().BeTrue();
        player.HasCondition(ConditionType.Drunk).Should().BeFalse();

        // act
        Thread.Sleep(200);
        var removeResult = player.Inventory.RemoveItem(Slot.Ring, 1);

        // assert
        removeResult.Succeeded.Should().BeTrue();
        player.HasCondition(ConditionType.Drunk).Should().BeTrue();

        // act
        Thread.Sleep(300);
        ExecuteConditionTick(player);

        // assert
        player.HasCondition(ConditionType.Drunk).Should().BeFalse();
    }

    [Fact]
    public void Player_gets_drunk_condition_back_only_after_removing_ring_1_and_boots_with_suppress_drunk_attribute()
    {
        // arrange
        var player = PlayerTestDataBuilder.Build();
        var ring1WithSuppressDrunk = ItemTestDataBuilder.CreateDefenseEquipmentItem(2162, "ring",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.SuppressDrunk, true)
            ]);
        var bootsWithSuppressDrunk = ItemTestDataBuilder.CreateBodyEquipmentItem(2202, "feet",
            itemTypeAttributes:
            [
                (ItemTypeAttribute.SuppressDrunk, true)
            ]);

        player.AddCondition(new Condition(ConditionType.Drunk, 5000));
        player.HasCondition(ConditionType.Drunk).Should().BeTrue();

        // act
        var equipRing1Result = player.Inventory.AddItem(ring1WithSuppressDrunk, Slot.Ring);

        // assert
        equipRing1Result.Succeeded.Should().BeTrue();
        player.HasCondition(ConditionType.Drunk).Should().BeFalse();

        // act
        var equipBootsResult = player.Inventory.AddItem(bootsWithSuppressDrunk, Slot.Feet);

        // assert
        equipBootsResult.Succeeded.Should().BeTrue();
        player.HasCondition(ConditionType.Drunk).Should().BeFalse();

        // act
        var removeBootsResult = player.Inventory.RemoveItem(Slot.Feet, 1);

        // assert
        removeBootsResult.Succeeded.Should().BeTrue();
        player.HasCondition(ConditionType.Drunk).Should().BeFalse();

        // act
        var removeRing1Result = player.Inventory.RemoveItem(Slot.Ring, 1);

        // assert
        removeRing1Result.Succeeded.Should().BeTrue();
        player.HasCondition(ConditionType.Drunk).Should().BeTrue();
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