using Moq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;
using PathFinder = NeoServer.Domain.World.Map.PathFinder;

namespace NeoServer.Domain.Tests.Creature.WalkableCreature;

public class PlayerTest
{
    [Fact]
    public void HasNextStep_Returns_True_When_Player_Has_Steps_To_Walk()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100, skills: new Dictionary<SkillType, ISkill>
        {
            { SkillType.Level, new Skill(SkillType.Level, 100) }
        });

        Assert.False(sut.HasNextStep);
        sut.WalkTo(Direction.South, Direction.North);
        Assert.True(sut.HasNextStep);
    }

    [Fact]
    [ThreadBlocking]
    public void IsFollowing_Returns_True_When_Player_Is_Following_Someone()
    {
        var pathFinder = new Mock<IPathFinder>();
        var directions = new[] { Direction.North };
        pathFinder.Setup(x => x.Find(It.IsAny<ICreature>(), It.IsAny<Location>(), It.IsAny<FindPathParams>(),
            It.IsAny<ITileEnterRule>())).Returns((true, directions));

        var sut = PlayerTestDataBuilder.Build(hp: 100, skills: new Dictionary<SkillType, ISkill>
        {
            { SkillType.Level, new Skill(SkillType.Level, 100) }
        }, pathFinder: pathFinder.Object);

        var creature = new Mock<ICreature>();
        creature.Setup(x => x.Location).Returns(sut.Location);
        creature.Setup(x => x.CreatureId).Returns(123);

        Assert.False(sut.IsFollowing);
        sut.Follow(creature.Object);
        Assert.True(sut.IsFollowing);
    }

    [Theory]
    [InlineData(100, 200)]
    [InlineData(300, 0)]
    [InlineData(400, 0)]
    public void DecreaseSpeed_Should_Decrease_Speed_Value(ushort decrease, ushort expected)
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100, speed: 300);
        var emittedEvent = false;
        sut.OnChangedSpeed += (_, _) => emittedEvent = true;

        sut.DecreaseSpeed(decrease);

        Assert.Equal(expected, sut.Speed);
        Assert.True(emittedEvent);
    }

    [Theory]
    [InlineData(100, 400)]
    [InlineData(0, 300)]
    [InlineData(300, 600)]
    public void IncreaseSpeed_Should_Increase_Speed_Value(ushort increase, ushort expected)
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100, speed: 300);
        var emittedEvent = false;
        sut.OnChangedSpeed += (_, _) => emittedEvent = true;

        sut.IncreaseSpeed(increase);

        Assert.Equal(expected, sut.Speed);
        Assert.True(emittedEvent);
    }

    [Fact]
    [ThreadBlocking]
    public void Follow_Should_Emmit_Follow_And_Walk_Event()
    {
        var directions = new[] { Direction.North, Direction.East };
        var pathFinder = new Mock<IPathFinder>();
        pathFinder.Setup(x => x.Find(It.IsAny<ICreature>(), It.IsAny<Location>(), It.IsAny<FindPathParams>(),
            It.IsAny<ITileEnterRule>())).Returns((true, directions));

        var sut = PlayerTestDataBuilder.Build(hp: 100, speed: 300, pathFinder: pathFinder.Object);
        var followEventEmitted = false;
        var walkEventEmitted = false;

        sut.OnStartedWalking += _ => walkEventEmitted = true;
        sut.OnStartedFollowing += (_, _, _) => followEventEmitted = true;

        var creature = new Mock<ICreature>();
        creature.Setup(x => x.Location).Returns(new Location(100, 105, 7));
        creature.Setup(x => x.CreatureId).Returns(123);

        var tile = new Mock<IDynamicTile>();
        tile.Setup(x => x.Ground.StepSpeed).Returns(1000);
        tile.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        sut.SetCurrentTile(tile.Object);
        sut.Follow(creature.Object);

        Assert.True(sut.IsFollowing);
        Assert.Equal(creature.Object, sut.Following);
        Assert.True(followEventEmitted);
        Assert.True(walkEventEmitted);
        Assert.Equal(Direction.North, sut.GetNextStep());
        Assert.Equal(Direction.East, sut.GetNextStep());
    }

    [Fact]
    public void Stop_following_interrupts_player_walk()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);

        var sut = PlayerTestDataBuilder.Build(hp: 100, speed: 300, pathFinder: pathFinder);
        var stoppedWalkEventEmitted = false;

        sut.OnStoppedWalking += _ => stoppedWalkEventEmitted = true;

        var creature = new Mock<ICreature>();
        creature.Setup(x => x.Location).Returns(new Location(100, 105, 7));
        creature.Setup(x => x.CreatureId).Returns(123);

        var tile = map[100, 100, 7] as IDynamicTile;

        sut.SetCurrentTile(tile);

        sut.Follow(creature.Object);

        //act
        sut.StopFollowing();

        //assert
        Assert.False(sut.IsFollowing);
        Assert.Null(sut.Following);
        Assert.True(stoppedWalkEventEmitted);
        Assert.Equal(Direction.None, sut.GetNextStep());
    }

    [Fact]
    [ThreadBlocking]
    public void WalkTo_Should_Emit_Events_And_Add_Next_Steps()
    {
        var directions = new[] { Direction.North, Direction.East };
        var pathFinder = new Mock<IPathFinder>();
        pathFinder.Setup(x => x.Find(It.IsAny<ICreature>(), It.IsAny<Location>(), It.IsAny<FindPathParams>(),
            It.IsAny<ITileEnterRule>())).Returns((true, directions));

        var sut = PlayerTestDataBuilder.Build(hp: 100, speed: 300, pathFinder: pathFinder.Object);

        var stoppedWalkingEvent = false;

        sut.OnStoppedWalking += _ => stoppedWalkingEvent = true;

        var tile = new Mock<IDynamicTile>();
        tile.Setup(x => x.Ground.StepSpeed).Returns(100);
        tile.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        sut.SetCurrentTile(tile.Object);
        //First instruction walking
        sut.WalkTo(new Location(105, 100, 7));

        //Second intruction walking (need stop first Walking)
        sut.WalkTo(new Location(102, 100, 7));

        Assert.True(sut.HasNextStep);
        Assert.True(stoppedWalkingEvent);
        Assert.Equal(Direction.North, sut.GetNextStep());
        Assert.Equal(Direction.East, sut.GetNextStep());
    }

    #region StopAllActions

    [Fact]
    public void Stop_All_Actions_When_IsFollowing()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);

        var sut = PlayerTestDataBuilder.Build(hp: 100, speed: 300, pathFinder: pathFinder);
        var stoppedWalkEventEmitted = false;

        sut.OnStoppedWalking += _ => stoppedWalkEventEmitted = true;

        var creature = new Mock<ICreature>();
        creature.Setup(x => x.Location).Returns(new Location(100, 105, 7));
        creature.Setup(x => x.CreatureId).Returns(123);

        var tile = map[100, 100, 7] as IDynamicTile;

        sut.SetCurrentTile(tile);

        sut.Follow(creature.Object);

        //act
        sut.StopAllActions();

        //assert
        Assert.False(sut.HasNextStep);
        Assert.False(sut.IsFollowing);
        Assert.Null(sut.Following);
        Assert.False(sut.Attacking);
        Assert.True(stoppedWalkEventEmitted);
        Assert.Equal(Direction.None, sut.GetNextStep());
    }

    [Fact]
    [ThreadBlocking]
    public void Stop_All_Actions_When_IsWalking()
    {
        var directions = new[] { Direction.North, Direction.East };
        var pathFinder = new Mock<IPathFinder>();
        pathFinder.Setup(x => x.Find(It.IsAny<ICreature>(), It.IsAny<Location>(), It.IsAny<FindPathParams>(),
            It.IsAny<ITileEnterRule>())).Returns((true, directions));

        var sut = PlayerTestDataBuilder.Build(hp: 100, speed: 300, pathFinder: pathFinder.Object);

        var stoppedWalkEventEmitted = false;

        sut.OnStoppedWalking += _ => stoppedWalkEventEmitted = true;

        var tile = new Mock<IDynamicTile>();
        tile.Setup(x => x.Ground.StepSpeed).Returns(100);
        tile.Setup(x => x.Location).Returns(new Location(100, 100, 7));

        sut.SetCurrentTile(tile.Object);
        //First instruction walking
        sut.WalkTo(new Location(105, 100, 7));

        sut.StopAllActions();

        Assert.False(sut.HasNextStep);
        Assert.False(sut.IsFollowing);
        Assert.Null(sut.Following);
        Assert.False(sut.Attacking);
        Assert.True(stoppedWalkEventEmitted);
        Assert.Equal(Direction.None, sut.GetNextStep());
    }

    [Fact]
    public void Stop_All_Actions_When_Attacking()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);

        var player = PlayerTestDataBuilder.Build(hp: 100, speed: 300, pathFinder: pathFinder);

        var monster = MonsterTestDataBuilder.Build(map: map);
        monster.SetNewLocation(new Location(100, 100, 7));

        (map[100, 100, 7] as DynamicTile)?.AddCreature(monster);
        (map[101, 100, 7] as DynamicTile)?.AddCreature(player);

        var stoppedAttackEventEmitted = false;

        player.OnStoppedAttack += _ => stoppedAttackEventEmitted = true;

        //act
        player.Attack(monster);
        player.StopAllActions();

        Assert.False(player.HasNextStep);
        Assert.False(player.IsFollowing);
        Assert.Null(player.Following);
        Assert.False(player.Attacking);
        Assert.True(stoppedAttackEventEmitted);
        Assert.Equal(Direction.None, player.GetNextStep());
    }

    #endregion
}