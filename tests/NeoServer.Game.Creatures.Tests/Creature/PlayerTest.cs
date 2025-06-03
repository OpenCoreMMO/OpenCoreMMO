using System;
using System.Collections.Generic;
using Moq;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Player;
using NeoServer.Game.Systems.Services;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.World.Models;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Creature;

public class PlayerTest
{
    [Theory]
    [InlineData(Direction.NorthWest, Direction.West)]
    [InlineData(Direction.SouthWest, Direction.West)]
    [InlineData(Direction.NorthEast, Direction.East)]
    [InlineData(Direction.SouthEast, Direction.East)]
    [InlineData(Direction.South, Direction.South)]
    [InlineData(Direction.East, Direction.East)]
    [InlineData(Direction.West, Direction.West)]
    [InlineData(Direction.North, Direction.North)]
    public void SafeDirection_When_Is_Diagonal_Return_Safe_Direction(Direction input, Direction expected)
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);
        sut.TurnTo(input);
        Assert.Equal(expected, sut.SafeDirection);
    }

    [Fact]
    public void ChangeOutfit_Changes_Outfit_And_Emit_Event()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);
        var changedOutfit = false;

        sut.OnChangedOutfit += (_, _) => changedOutfit = true;

        IOutfit outfit = new Outfit();
        outfit.Addon = 3;
        outfit.LookType = 12;
        outfit.Feet = 1;
        outfit.Head = 1;
        outfit.Body = 1;
        outfit.Legs = 1;
        outfit
            .SetEnabled(true)
            .SetGender(sut.Gender)
            .SetName("OUTFIT-TESTE")
            .SetPremium(false)
            .SetUnlocked(true);

        sut.ChangeOutfit(outfit);

        Assert.Equal(12, sut.Outfit.LookType);
        Assert.Equal(3, sut.Outfit.Addon);
        Assert.Equal(1, sut.Outfit.Body);
        Assert.Equal(1, sut.Outfit.Feet);
        Assert.Equal(1, sut.Outfit.Head);
        Assert.Equal(1, sut.Outfit.Legs);
        Assert.True(changedOutfit);
    }

    [Fact]
    public void SetTemporaryOutfit_Store_Current_To_LastOutfit_And_Changes_Outfit()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);
        var changedOutfit = false;

        sut.OnChangedOutfit += (_, _) => changedOutfit = true;

        sut.SetTemporaryOutfit(1, 1, 1, 1, 1, 1);

        Assert.Equal(1, sut.Outfit.LookType);
        Assert.Equal(1, sut.Outfit.Addon);
        Assert.Equal(1, sut.Outfit.Body);
        Assert.Equal(1, sut.Outfit.Feet);
        Assert.Equal(1, sut.Outfit.Head);
        Assert.Equal(1, sut.Outfit.Legs);
        Assert.True(changedOutfit);

        Assert.Equal(0, sut.LastOutfit.LookType);
        Assert.Equal(0, sut.LastOutfit.Addon);
        Assert.Equal(0, sut.LastOutfit.Body);
        Assert.Equal(0, sut.LastOutfit.Feet);
        Assert.Equal(0, sut.LastOutfit.Head);
        Assert.Equal(0, sut.LastOutfit.Legs);
    }

    [Fact]
    public void BackToOldOutfit_Sets_LastOutfit_To_Outfit_And_Changes_Outfit()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);
        var changedOutfit = false;

        sut.SetTemporaryOutfit(1, 1, 1, 1, 1, 1);

        sut.OnChangedOutfit += (_, _) => changedOutfit = true;

        sut.BackToOldOutfit();

        Assert.Null(sut.LastOutfit);
        Assert.True(changedOutfit);

        Assert.Equal(0, sut.Outfit.LookType);
        Assert.Equal(0, sut.Outfit.Addon);
        Assert.Equal(0, sut.Outfit.Body);
        Assert.Equal(0, sut.Outfit.Feet);
        Assert.Equal(0, sut.Outfit.Head);
        Assert.Equal(0, sut.Outfit.Legs);
    }

    [Fact]
    public void CanSeeInvisible_Returns_Flag_Value()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);

        Assert.False(sut.CanSeeInvisible);

        sut.Group.EnableFlag(PlayerFlag.CanSenseInvisibility);

        Assert.True(sut.CanSeeInvisible);
    }

    [Fact]
    public void CanSee_When_Creature_Is_Invisible_And_Cant_See_Invisible_Returns_False()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);

        var creature = new Mock<ICreature>();
        creature.Setup(x => x.IsInvisible).Returns(true);

        var result = sut.CanSee(creature.Object);

        Assert.False(result);
    }

    [Fact]
    public void CanSee_When_Creature_Is_Invisible_And_Can_See_Invisible_Returns_True()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);

        sut.Group.EnableFlag(PlayerFlag.CanSenseInvisibility);

        var creature = new Mock<ICreature>();
        creature.Setup(x => x.IsInvisible).Returns(true);

        var result = sut.CanSee(creature.Object);

        Assert.True(result);
    }

    [Fact]
    public void Say_Should_Emit_Event()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);
        var messageEmitted = "";
        var speechTypeEmitted = SpeechType.None;

        sut.SetTemporaryOutfit(1, 1, 1, 1, 1, 1);

        sut.OnSay += (_, type, message, _) =>
        {
            messageEmitted = message;
            speechTypeEmitted = type;
        };

        sut.Say("Hello", SpeechType.Say);

        Assert.Equal("Hello", messageEmitted);
        Assert.Equal(SpeechType.Say, speechTypeEmitted);
    }

    [Fact]
    public void Say_To_Receiver_Should_Emit_Event()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);
        var receiver = new Mock<ICreature>();
        var messageEmitted = "";
        var speechTypeEmitted = SpeechType.None;
        ICreature to = null;

        sut.SetTemporaryOutfit(1, 1, 1, 1, 1, 1);

        sut.OnSay += (_, type, message, receiver) =>
        {
            messageEmitted = message;
            speechTypeEmitted = type;
            to = receiver;
        };

        sut.Say("Hello", SpeechType.Private, receiver.Object);

        Assert.Equal("Hello", messageEmitted);
        Assert.Equal(SpeechType.Private, speechTypeEmitted);
        Assert.Equal(receiver.Object, to);
    }

    [Fact]
    public void Say_Empty_Message_Dont_Emit_Event()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);
        var receiver = new Mock<ICreature>();
        string messageEmitted = null;
        var speechTypeEmitted = SpeechType.None;
        ICreature to = null;

        sut.SetTemporaryOutfit(1, 1, 1, 1, 1, 1);

        sut.OnSay += (_, type, message, receiver) =>
        {
            messageEmitted = message;
            speechTypeEmitted = type;
            to = receiver;
        };

        sut.Say("", SpeechType.Private, receiver.Object);

        Assert.Null(messageEmitted);
        Assert.Equal(SpeechType.None, speechTypeEmitted);
        Assert.Null(to);
    }

    [Fact]
    public void CanBeSeen_Returns_True_Or_False_Depending_On_Flag_State()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);

        sut.Group.EnableFlag(PlayerFlag.IgnoreYellCheck);
        Assert.True(sut.CanBeSeen);

        sut.Group.DisableFlag(PlayerFlag.IgnoreYellCheck);
        Assert.False(sut.CanBeSeen);
    }

    [Fact]
    public void Use_WalksToItem_WhenUsedOnTargetTile()
    {
        //arrange
        var walkLocation = Location.Zero;

        // Hook into the WalkToMechanism the player uses to move to the target item.
        // When the WalkToMechanism is triggered set the walkLocation variable to the destination.
        var walkMechanismMock = new Mock<IWalkToMechanism>();
        walkMechanismMock.Setup(x =>
                x.WalkTo(It.IsAny<IPlayer>(), It.IsAny<Action>(), It.IsAny<Location>(), It.IsAny<bool>()))
            .Callback((IPlayer _, Action _, Location location, bool _) => { walkLocation = location; });

        // BuildLookText our player, used item, and targetTile. Each should have a different location.
        var player = PlayerTestDataBuilder.Build();

        var itemLocation = new Location(105, 105, 7);
        var usedItemMock = new Mock<IUsableOn>();
        usedItemMock.Setup(x => x.Location).Returns(itemLocation);

        var tileLocation = new Location(101, 101, 7);
        var targetTileMock = new Mock<ITile>();
        targetTileMock.Setup(x => x.Location).Returns(tileLocation);

        var map = MapTestDataBuilder.Build(targetTileMock.Object);
        var sut = new PlayerUseService(walkMechanismMock.Object, map);

        //act
        sut.Use(player, usedItemMock.Object, targetTileMock.Object);

        //assert
        Assert.Equal(itemLocation, walkLocation);
    }

    [Fact]
    public void Player_Lost_Experience_On_Death()
    {
        var player = PlayerTestDataBuilder.Build(hp: 100, skills: new Dictionary<SkillType, ISkill>
        {
            { SkillType.Level, new Skill(SkillType.Level, 9, 9100) }
        }) as Player.Player;

        player.Death(null);

        Assert.Equal(8190, (double)player.Experience);
        Assert.Equal(9, player.Level);
    }

    [Fact]
    public void Player_Lost_Level_On_Death()
    {
        var player = PlayerTestDataBuilder.Build(hp: 100, skills: new Dictionary<SkillType, ISkill>
        {
            { SkillType.Level, new Skill(SkillType.Level, 9, 6500) }
        }) as Player.Player;
        player.Death(null);

        Assert.Equal(5850, (double)player.Experience);
        Assert.Equal(8, player.Level);
    }

    [Fact]
    public void Player_Has_Changed_Local_To_Temple_On_Death()
    {
        var townCoordinate = new Coordinate(1000, 2033, 8);

        var player =
            PlayerTestDataBuilder.Build(hp: 100, town: new Town { Coordinate = townCoordinate }) as Player.Player;

        player.SetNewLocation(new Location(1234, 1341, 3));

        Assert.NotEqual(player.Location, townCoordinate.Location);

        player.Death(null);

        Assert.Equal(player.Location, townCoordinate.Location);
    }
}