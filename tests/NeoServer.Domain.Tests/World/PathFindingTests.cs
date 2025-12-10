using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Algorithms.AStar;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.World;

public class PathFindingTests
{
    [Fact]
    [Trait("Category", "PathFinding")]
    [ThreadBlocking]
    public void Monster_finds_path_when_push_creatures_flag_allows_pushing_blocking_monsters()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        // Create a start monster with a push creatures flag at x=100, y=105
        var startMonster = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });

        ((DynamicTile)map[100, 105, 7]).AddCreature(startMonster);

        // Create a target player at x=105, y=105
        var targetPlayer = PlayerTestDataBuilder.Build();
        ((DynamicTile)map[105, 105, 7]).AddCreature(targetPlayer);

        // Add non-push creatures monsters around the player (8 blocking monsters)
        var blockingMonster1 = MonsterTestDataBuilder.Build();
        blockingMonster1.Metadata.Flags.Add(CreatureFlagAttribute.Pushable, 1);
        ((DynamicTile)map[104, 105, 7]).AddCreature(blockingMonster1); // west

        var blockingMonster2 = MonsterTestDataBuilder.Build();
        blockingMonster2.Metadata.Flags.Add(CreatureFlagAttribute.Pushable, 1);

        ((DynamicTile)map[105, 104, 7]).AddCreature(blockingMonster2); // north

        var blockingMonster3 = MonsterTestDataBuilder.Build();
        blockingMonster3.Metadata.Flags.Add(CreatureFlagAttribute.Pushable, 1);

        ((DynamicTile)map[105, 106, 7]).AddCreature(blockingMonster3); // south

        var blockingMonster4 = MonsterTestDataBuilder.Build();
        blockingMonster4.Metadata.Flags.Add(CreatureFlagAttribute.Pushable, 1);

        ((DynamicTile)map[106, 105, 7]).AddCreature(blockingMonster4); // east

        var blockingMonster5 = MonsterTestDataBuilder.Build();
        blockingMonster5.Metadata.Flags.Add(CreatureFlagAttribute.Pushable, 1);

        ((DynamicTile)map[104, 104, 7]).AddCreature(blockingMonster5); // northwest

        var blockingMonster6 = MonsterTestDataBuilder.Build();
        blockingMonster6.Metadata.Flags.Add(CreatureFlagAttribute.Pushable, 1);

        ((DynamicTile)map[104, 106, 7]).AddCreature(blockingMonster6); // southwest

        var blockingMonster7 = MonsterTestDataBuilder.Build();
        blockingMonster7.Metadata.Flags.Add(CreatureFlagAttribute.Pushable, 1);

        ((DynamicTile)map[106, 104, 7]).AddCreature(blockingMonster7); // northeast

        var blockingMonster8 = MonsterTestDataBuilder.Build();
        blockingMonster8.Metadata.Flags.Add(CreatureFlagAttribute.Pushable, 1);

        ((DynamicTile)map[106, 106, 7]).AddCreature(blockingMonster8); // southeast

        var fpp = new FindPathParams(true)
        {
            AllowDiagonal = true,
            ClearSight = true,
            KeepDistance = false,
            FullPathSearch = true,
            MaxSearchDist = 12,
            MaxTargetDist = 1,
            MinTargetDist = 1,
            PushMonsters = true
        };

        var tileEnterRule = MonsterEnterTileRule.Rule;

        //act
        var result = AStar.GetPathMatching(map, startMonster, startMonster.Location, targetPlayer.Location, fpp,
            tileEnterRule);

        //assert
        result.Found.Should().BeTrue();
        result.Directions.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "PathFinding")]
    [ThreadBlocking]
    public void Monster_does_not_find_path_when_player_surrounded_by_monsters_with_push_creatures_flag()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        // Create start monster without push creatures flag at x=100, y=105
        var startMonster = MonsterTestDataBuilder.Build();
        ((DynamicTile)map[100, 105, 7]).AddCreature(startMonster);

        // Create target player at x=105, y=105
        var targetPlayer = PlayerTestDataBuilder.Build();
        ((DynamicTile)map[105, 105, 7]).AddCreature(targetPlayer);

        // Add monsters with push creatures flag around the player (8 blocking monsters)
        var blockingMonster1 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((DynamicTile)map[104, 105, 7]).AddCreature(blockingMonster1); // west

        var blockingMonster2 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((DynamicTile)map[105, 104, 7]).AddCreature(blockingMonster2); // north

        var blockingMonster3 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((DynamicTile)map[105, 106, 7]).AddCreature(blockingMonster3); // south

        var blockingMonster4 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((DynamicTile)map[106, 105, 7]).AddCreature(blockingMonster4); // east

        var blockingMonster5 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((DynamicTile)map[104, 104, 7]).AddCreature(blockingMonster5); // northwest

        var blockingMonster6 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((DynamicTile)map[104, 106, 7]).AddCreature(blockingMonster6); // southwest

        var blockingMonster7 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((DynamicTile)map[106, 104, 7]).AddCreature(blockingMonster7); // northeast

        var blockingMonster8 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((DynamicTile)map[106, 106, 7]).AddCreature(blockingMonster8); // southeast

        var fpp = new FindPathParams(true)
        {
            AllowDiagonal = true,
            ClearSight = true,
            KeepDistance = false,
            FullPathSearch = true,
            MaxSearchDist = 12,
            MaxTargetDist = 1,
            MinTargetDist = 1
        };

        var tileEnterRule = MonsterEnterTileRule.Rule;

        //act
        var result = AStar.GetPathMatching(map, startMonster, startMonster.Location, targetPlayer.Location, fpp,
            tileEnterRule);

        //assert
        result.Found.Should().BeFalse();
        result.Directions.Should().BeEmpty();
    }


    [Fact]
    [Trait("Category", "PathFinding")]
    [ThreadBlocking]
    public void Monster_finds_path_around_pushable_monster()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 102, 100, 101, 7, 7);

        // Create sut monster with canpushcreatures flag at x=100, y=100
        var sutMonster = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        }, name: "sutMonster");

        sutMonster.SetNewLocation(new Location(100, 100, 7));
        map.PlaceCreature(sutMonster);


        // Create a target player at x=102, y=100
        var targetPlayer = PlayerTestDataBuilder.Build();
        targetPlayer.SetNewLocation(new Location(102, 100, 7));
        map.PlaceCreature(targetPlayer);

        // Add a pushable monster in the middle at x=101, y=100
        var middleMonster = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.Pushable, 1 }
        });
        middleMonster.SetNewLocation(new Location(101, 100, 7));
        map.PlaceCreature(middleMonster);

        var fpp = new FindPathParams(true)
        {
            AllowDiagonal = true,
            ClearSight = true,
            KeepDistance = false,
            FullPathSearch = true,
            MaxSearchDist = 12,
            MaxTargetDist = 1,
            MinTargetDist = 1,
            PushMonsters = true
        };

        //act
        var result = new PathFinder(map).Find(sutMonster, targetPlayer.Location, fpp, sutMonster.TileEnterRule);

        //assert
        result.Found.Should().BeTrue();
        result.Directions.Should().HaveCount(2);

        result.Directions[0].Should().Be(Direction.South);
        result.Directions[1].Should().Be(Direction.East);

        // Calculate path locations
        var pathLocations = new List<Location> { sutMonster.Location };
        foreach (var dir in result.Directions)
        {
            var next = pathLocations.Last().GetNextLocation(dir);
            pathLocations.Add(next);
        }

        // Path should go through 100,101 and 101,101 without passing through 101,100
        pathLocations.Should().Contain(new Location(100, 101, 7));
        pathLocations.Should().Contain(new Location(101, 101, 7));
        pathLocations.Should().NotContain(new Location(101, 100, 7));
    }
}