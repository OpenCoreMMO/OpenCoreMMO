using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Monster.Summon;
using Serilog;

namespace NeoServer.Domain.Creatures.Factories;

public class MonsterFactory : IMonsterFactory
{
    private readonly ILogger _logger;
    private readonly IMapTool _mapTool;
    private readonly IMonsterTypeStore _monsterTypeStore;

    public MonsterFactory(IMonsterTypeStore monsterTypeStore,
        ILogger logger, IMapTool mapTool)
    {
        _monsterTypeStore = monsterTypeStore;

        _logger = logger;
        _mapTool = mapTool;
        Instance = this;
    }

    public static IMonsterFactory Instance { get; private set; }

    public IMonster CreateSummon(string name, ICreature master)
    {
        var result = _monsterTypeStore.TryGetValue(name, out var monsterType);
        if (!result)
        {
            _logger.Warning("Given monster name: {Name} is not loaded", name);
            return null;
        }

        IMonster monster = new Summon(monsterType, _mapTool, master);

        return monster;
    }

    public IMonster Create(string name, ISpawnPoint spawn = null)
    {
        var result = _monsterTypeStore.TryGetValue(name, out var monsterType);
        if (!result)
        {
            _logger.Warning("Given monster name: {Name} is not loaded", name);
            return null;
        }

        return new Monster.Monster(monsterType, _mapTool, spawn);
    }
}