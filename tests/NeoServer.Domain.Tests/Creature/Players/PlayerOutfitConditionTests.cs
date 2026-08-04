using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Creatures.Player.Outfit;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerOutfitConditionTests
{
    [Fact]
    public void Outfit_sets_temporary_outfit_when_applied()
    {
        var player = PlayerTestDataBuilder.Build();

        var outfit = new OutfitCondition(10_000, 128, 1, 2, 3, 4, 0);
        player.AddCondition(outfit);

        player.Outfit.LookType.Should().Be(128);
        player.Outfit.Head.Should().Be(1);
        player.Outfit.Body.Should().Be(2);
        player.Outfit.Legs.Should().Be(3);
        player.Outfit.Feet.Should().Be(4);
    }

    [Fact]
    public void Outfit_restores_original_outfit_when_ended()
    {
        var player = PlayerTestDataBuilder.Build();
        var originalOutfit = new Outfit
        {
            LookType = player.Outfit.LookType,
            Head = player.Outfit.Head,
            Body = player.Outfit.Body,
            Legs = player.Outfit.Legs,
            Feet = player.Outfit.Feet,
            Addon = player.Outfit.Addon
        };

        var outfit = new OutfitCondition(10_000, 128, 1, 2, 3, 4, 0);
        player.AddCondition(outfit);

        player.RemoveCondition(outfit);

        player.Outfit.LookType.Should().Be(originalOutfit.LookType);
        player.Outfit.Head.Should().Be(originalOutfit.Head);
        player.Outfit.Body.Should().Be(originalOutfit.Body);
        player.Outfit.Legs.Should().Be(originalOutfit.Legs);
        player.Outfit.Feet.Should().Be(originalOutfit.Feet);
    }

    [Fact]
    public void Outfit_removes_existing_outfit_when_reapplied()
    {
        var player = PlayerTestDataBuilder.Build();

        var first = new OutfitCondition(10_000, 128, 1, 1, 1, 1, 0);
        player.AddCondition(first);

        var second = new OutfitCondition(10_000, 129, 2, 2, 2, 2, 0);
        player.AddCondition(second);

        player.GetCondition(ConditionType.Outfit).Should().BeSameAs(second);
    }

    [Fact]
    public void Outfit_does_not_crash_when_no_outfit_change_needed()
    {
        var player = PlayerTestDataBuilder.Build();

        var outfit = new OutfitCondition(10_000, 0, 0, 0, 0, 0, 0);

        player.Invoking(x => x.AddCondition(outfit)).Should().NotThrow();
    }

    [Fact]
    public void Outfit_restores_original_outfit_when_replaced_by_another_outfit()
    {
        var player = PlayerTestDataBuilder.Build();
        var originalLookType = player.Outfit.LookType;

        var first = new OutfitCondition(10_000, 128, 1, 1, 1, 1, 0);
        player.AddCondition(first);

        var second = new OutfitCondition(10_000, 129, 2, 2, 2, 2, 0);
        player.AddCondition(second);

        player.RemoveCondition(second);

        player.Outfit.LookType.Should().Be(originalLookType);
    }
}
