using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Combat;
using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Combat.Monster;
using NeoServer.Domain.Combat.Player;
using NeoServer.Domain.Combat.Validations;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Items;
using NeoServer.Domain.Services;
using NeoServer.Domain.Spells;
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

        var attackValidation = new AttackValidation(new MapTool(map, new PathFinder(map)), map, gameConfiguration.PvP);

        var itemTypeStore = ItemTypeStoreTestBuilder.Build(new ItemType().SetId(2019));

        var magicFieldService =
            new MagicFieldService(map, ItemFactoryTestBuilder.Build(itemTypeStore), gameConfiguration.PvP);

        var monsterTypeStore = new MonsterTypeStore();

        var conditionAttackService = new ConditionAttackService(monsterTypeStore);

        var areaCalculationService = new AreaCalculationService(map);

        var areaAttackService =
            new AreaAttackService(mockEventAggregator.Object, map, magicFieldService, conditionAttackService, attackValidation, areaCalculationService);

        var singleTargetCombat =
            new SingleTargetAttackService(mockEventAggregator.Object, gameConfiguration.Combat, conditionAttackService,
                magicFieldService);

        return new AttackService(logger.Object, skullService, areaAttackService, singleTargetCombat,
            conditionAttackService, attackValidation);
    }
}

public static class MonsterCombatServiceTestBuilder
{
    public static MonsterCombatService Build(IMap map, PvpType pvpType = PvpType.OpenPvP)
    {
        var attackService = AttackServiceTestBuilder.Build(map, pvpType);
        var mapTool = new MapTool(map, new PathFinder(map));
        var spellService = new SpellService(new SpellCastValidation(mapTool), new Mock<IEventAggregator>().Object, map);
        var logger = new Mock<ILogger>();
        return new MonsterCombatService(attackService, spellService, logger.Object);
    }
}