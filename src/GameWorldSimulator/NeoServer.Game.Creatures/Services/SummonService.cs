using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Creatures.Monster.Summon;
using Serilog;

namespace NeoServer.Game.Creatures.Services;

public class SummonService(ICreatureFactory creatureFactory, IMap map, ILogger logger) : ISummonService {
    public IMonster Summon(ICreature master, string summonName)
    {
        if (creatureFactory.CreateSummon(summonName, master) is not Summon summon)
        {
            logger.Error($"Summon with name: {summonName} does not exists");
            return null;
        }

        foreach (var neighbour in master.Location.Neighbours)
            if (map[neighbour] is IDynamicTile { HasCreature: false } toTile &&
                !toTile.HasFlag(TileFlags.BLockSolid) && !toTile.HasTeleport(out _))
            {
                summon.Born(toTile.Location); // TODO: NTN - create MonsterSummonAnotherUseCase
                return summon;
            }

        return null;
    }
}