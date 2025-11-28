using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Creatures.Monster.Summon;
using Serilog;

namespace NeoServer.Domain.Creatures.Services;

public class SummonService(ICreatureFactory creatureFactory, IMap map, ILogger logger) : ISummonService
{
    public IMonster SpamSummon(ICreature master, string summonName)
    {
        if (creatureFactory.CreateSummon(summonName, master) is not Summon summon)
        {
            logger.Error("Summon with name: {SummonName} does not exists", summonName);
            return null;
        }

        foreach (var neighbour in master.Location.Neighbours)
            if (map[neighbour] is IDynamicTile { HasAnyCreature: false } toTile &&
                toTile.CanEnter(summon) && !toTile.HasTeleport(out _))
            {
                summon.Born(toTile.Location);
                return summon;
            }

        return null;
    }
}