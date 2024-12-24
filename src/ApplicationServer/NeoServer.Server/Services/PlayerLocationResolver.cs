using NeoServer.Data.Entities;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures;
using NeoServer.Game.World;
using Serilog;

namespace NeoServer.Server.Services;

//todo: this project is not the best place for this 
public class PlayerLocationResolver(World world, ILogger logger)
{
    public Location GetPlayerLocation(PlayerEntity playerEntity)
    {
        var location = new Location((ushort)playerEntity.PosX, (ushort)playerEntity.PosY, (byte)playerEntity.PosZ);

        var playerTile = world.TryGetTile(ref location, out var tile) &&
                         PlayerEnterTileRule.Rule.CanEnter(tile, location)
            ? tile
            : null;

        if (playerTile is not null) return location;
        
        foreach (var neighbour in location.Neighbours)
        {
            world.TryGetTile(ref location, out var neighbourTile);
            if (neighbourTile is IDynamicTile && PlayerEnterTileRule.Rule.CanEnter(neighbourTile, neighbour))
            {
                return location;
            }
        }

        var town = GetTown(playerEntity);
        if (town is null) return Location.Zero;
        
        var townLocation = town.Coordinate.Location;

        playerTile = world.TryGetTile(ref townLocation, out var townTile) && townTile is IDynamicTile townDynamicTile &&
                     PlayerEnterTileRule.Rule.CanEnter(townDynamicTile, townLocation) 
            ? townDynamicTile
            : null;

        return playerTile?.Location ?? Location.Zero;
    }

    protected ITown GetTown(PlayerEntity playerEntity)
    {
        if (!world.TryGetTown((ushort)playerEntity.TownId, out var town))
            logger.Error("player town not found: {PlayerModelTownId}", playerEntity.TownId);
        return town;
    }
}