using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Algorithms.AStar;

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

        // Create start monster with push creatures flag at x=100, y=105
        var startMonster = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[100, 105, 7]).AddCreature(startMonster);

        // Create target player at x=105, y=105
        var targetPlayer = PlayerTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[105, 105, 7]).AddCreature(targetPlayer);

        // Add non push creatures monsters around the player (8 blocking monsters)
        var blockingMonster1 = MonsterTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[104, 105, 7]).AddCreature(blockingMonster1); // west

        var blockingMonster2 = MonsterTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[105, 104, 7]).AddCreature(blockingMonster2); // north

        var blockingMonster3 = MonsterTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[105, 106, 7]).AddCreature(blockingMonster3); // south

        var blockingMonster4 = MonsterTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[106, 105, 7]).AddCreature(blockingMonster4); // east

        var blockingMonster5 = MonsterTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[104, 104, 7]).AddCreature(blockingMonster5); // northwest

        var blockingMonster6 = MonsterTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[104, 106, 7]).AddCreature(blockingMonster6); // southwest

        var blockingMonster7 = MonsterTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[106, 104, 7]).AddCreature(blockingMonster7); // northeast

        var blockingMonster8 = MonsterTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[106, 106, 7]).AddCreature(blockingMonster8); // southeast

        var fpp = new FindPathParams
        {
            AllowDiagonal = true,
            ClearSight = true,
            KeepDistance = false,
            OneStep = false,
            FullPathSearch = true,
            MaxSearchDist = 12,
            MaxTargetDist = 1,
            MinTargetDist = 1,
            PushMonsters = true
        };

        var tileEnterRule = MonsterEnterTileRule.Rule;

        //act
        var result = AStar.GetPathMatching(map, startMonster, startMonster.Location, targetPlayer.Location, fpp, tileEnterRule);

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
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[100, 105, 7]).AddCreature(startMonster);

        // Create target player at x=105, y=105
        var targetPlayer = PlayerTestDataBuilder.Build();
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[105, 105, 7]).AddCreature(targetPlayer);

        // Add monsters with push creatures flag around the player (8 blocking monsters)
        var blockingMonster1 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[104, 105, 7]).AddCreature(blockingMonster1); // west

        var blockingMonster2 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[105, 104, 7]).AddCreature(blockingMonster2); // north

        var blockingMonster3 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[105, 106, 7]).AddCreature(blockingMonster3); // south

        var blockingMonster4 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[106, 105, 7]).AddCreature(blockingMonster4); // east

        var blockingMonster5 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[104, 104, 7]).AddCreature(blockingMonster5); // northwest

        var blockingMonster6 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[104, 106, 7]).AddCreature(blockingMonster6); // southwest

        var blockingMonster7 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[106, 104, 7]).AddCreature(blockingMonster7); // northeast

        var blockingMonster8 = MonsterTestDataBuilder.Build(flags: new Dictionary<CreatureFlagAttribute, ushort>
        {
            { CreatureFlagAttribute.CanPushCreatures, 1 }
        });
        ((NeoServer.Domain.World.Models.Tiles.DynamicTile)map[106, 106, 7]).AddCreature(blockingMonster8); // southeast

        var fpp = new FindPathParams
        {
            AllowDiagonal = true,
            ClearSight = true,
            KeepDistance = false,
            OneStep = false,
            FullPathSearch = true,
            MaxSearchDist = 12,
            MaxTargetDist = 1,
            MinTargetDist = 1
        };

        var tileEnterRule = MonsterEnterTileRule.Rule;

        //act
        var result = AStar.GetPathMatching(map, startMonster, startMonster.Location, targetPlayer.Location, fpp, tileEnterRule);

        //assert
        result.Found.Should().BeFalse();
        result.Directions.Should().BeEmpty();
    }
}