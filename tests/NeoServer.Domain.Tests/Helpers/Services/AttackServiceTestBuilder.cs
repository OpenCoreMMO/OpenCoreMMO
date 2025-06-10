using Moq;
using NeoServer.Domain.Combat;
using NeoServer.Domain.Combat.Services;
using NeoServer.Domain.Combat.Services.Attacks;
using NeoServer.Domain.Combat.Services.Attacks.Validators;
using NeoServer.Domain.Combat.Services.Spells;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Monster.Managers;
using NeoServer.Domain.Items;
using NeoServer.Domain.Services;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Services;
using Serilog;

namespace NeoServer.Domain.Tests.Helpers.Services;

public class AttackServiceTestBuilder
{
    public static IAttackService Build(IMap map, PvpType pvpType = PvpType.OpenPvP)
    {
        var gameConfiguration = new GameConfiguration
        {
            PvP = new PvPConfiguration(pvpType),
            Combat = new CombatConfiguration(true, true)
        };

        var skullService = new PlayerSkullService(gameConfiguration);
        var logger = new Mock<ILogger>();
        var mockEventAggregator = new Mock<IEventAggregator>();

        var itemTypeStore = ItemTypeStoreTestBuilder.Build(new ItemType().SetId(2019));

        var magicFieldService =
            new MagicFieldService(map, ItemFactoryTestBuilder.Build(itemTypeStore), gameConfiguration.PvP);

        var monsterDataManager = new MonsterDataManager();

        var conditionAttackService = new ConditionAttackService(monsterDataManager);

        var areaAttackService =
            new AreaAttackService(mockEventAggregator.Object, map, magicFieldService, conditionAttackService);

        var singleTargetCombat =
            new SingleTargetAttackService(mockEventAggregator.Object, gameConfiguration.Combat, conditionAttackService,
                magicFieldService);

        var attackValidation = new AttackValidation(new MapTool(map, new PathFinder(map)), map, gameConfiguration.PvP);

        return new AttackService(logger.Object, skullService, areaAttackService, singleTargetCombat, attackValidation);
    }
}

public static class MonsterCombatServiceTestBuilder
{
    public static MonsterCombatService Build(IMap map, PvpType pvpType = PvpType.OpenPvP)
    {
        var attackService = AttackServiceTestBuilder.Build(map, pvpType);
        var mapTool = new MapTool(map, new PathFinder(map));
        var spellService = new SpellService(new SpellCastValidation(mapTool), new Mock<IEventAggregator>().Object, map);
        return new MonsterCombatService(attackService, spellService);
    }
}