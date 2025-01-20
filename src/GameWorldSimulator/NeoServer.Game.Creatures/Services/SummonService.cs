using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Creatures.Monster.Summon;
using Serilog;

namespace NeoServer.Game.Creatures.Services;

public class SummonService : ISummonService
{
    private readonly ICreatureFactory _creatureFactory;
    private readonly ILogger _logger;
    private readonly IMap _map;

    public SummonService(ICreatureFactory creatureFactory, IMap map, ILogger logger)
    {
        _creatureFactory = creatureFactory;
        _map = map;
        _logger = logger;
    }

    public IMonster Summon(ICreature master, string summonName)
    {
        if (_creatureFactory.CreateSummon(summonName, master) is not Summon summon)
        {
            _logger.Error($"Summon with name: {summonName} does not exists");
            return null;
        }

        foreach (var neighbour in master.Location.Neighbours)
            if (_map[neighbour] is IDynamicTile { HasCreature: false } toTile &&
                !toTile.HasFlag(TileFlags.BLockSolid) && !toTile.HasTeleport(out _))
            {
                summon.Born(toTile.Location);
                return summon;
            }

        return null;
    }
}