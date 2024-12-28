using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Creatures.Monster.Summon;
using Serilog;

namespace NeoServer.Game.Creatures.Factories;

public class MonsterFactory : IMonsterFactory
{
    private readonly IMapTool _mapTool;
    private readonly IMonsterDataManager _monsterManager;

    private readonly ILogger logger;

    public MonsterFactory(IMonsterDataManager monsterManager,
        ILogger logger, IMapTool mapTool)
    {
        _monsterManager = monsterManager;

        this.logger = logger;
        _mapTool = mapTool;
        Instance = this;
    }

    public static IMonsterFactory Instance { get; private set; }

    public IMonster CreateSummon(string name, ICreature master)
    {
        var result = _monsterManager.TryGetMonster(name, out var monsterType);
        if (result == false)
        {
            logger.Warning($"Given monster name: {name} is not loaded");
            return null;
        }

        IMonster monster = new Summon(monsterType, _mapTool, master);

        return monster;
    }

    public IMonster Create(string name, ISpawnPoint spawn = null)
    {
        var result = _monsterManager.TryGetMonster(name, out var monsterType);
        if (result == false)
        {
            logger.Warning($"Given monster name: {name} is not loaded");
            return null;
        }

        return new Monster.Monster(monsterType, _mapTool, spawn);
    }
}