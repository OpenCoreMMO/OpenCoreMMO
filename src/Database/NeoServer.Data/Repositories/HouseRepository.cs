using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Serializers;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Repositories;
using Serilog;

namespace NeoServer.Data.Repositories;

public class HouseRepository : BaseRepository<HouseEntity>, IHouseRepository
{
    private readonly IHouseFactory _houseFactory;
    private readonly IItemFactory _itemFactory;

    public HouseRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger, IHouseFactory houseFactory, IItemFactory itemFactory)
        : base(contextOptions, logger)
    {
        _houseFactory = houseFactory;
        _itemFactory = itemFactory;
    }

    public async Task<IEnumerable<House>> GetAll()
    {
        await using var context = NewDbContext;
        var entities = await context.Houses.Include(h => h.HouseLists).ToListAsync();

        return entities.Select(e =>
        {
            var house = _houseFactory.Create(
                (uint)e.Id,
                e.Name,
                (ushort)e.TownId,
                (uint)e.Rent,
                (uint)e.OwnerId,
                e.OwnerName ?? string.Empty,
                e.OwnerAccountId,
                e.PaidUntil,
                (byte)e.Warnings
            );

            if (e.EntryX.HasValue && e.EntryY.HasValue && e.EntryZ.HasValue)
                house.EntryPosition = new NeoServer.Domain.Common.Location.Structs.Location(
                    (ushort)e.EntryX.Value, (ushort)e.EntryY.Value, (byte)e.EntryZ.Value);

            return house;
        });
    }

    public async Task<IReadOnlyDictionary<uint, List<(uint ListId, string ListText)>>> GetAllAccessListText()
    {
        await using var context = NewDbContext;
        var rows = await context.HouseList.ToListAsync();

        var dict = new Dictionary<uint, List<(uint, string)>>();
        foreach (var row in rows)
        {
            var houseId = (uint)row.HouseId;
            if (!dict.TryGetValue(houseId, out var list))
            {
                list = new List<(uint, string)>();
                dict[houseId] = list;
            }
            list.Add(((uint)row.ListId, row.List));
        }

        return new ReadOnlyDictionary<uint, List<(uint ListId, string ListText)>>(dict);
    }

    public async Task<House> GetById(uint id)
    {
        await using var context = NewDbContext;
        var entity = await context.Houses
            .Include(h => h.HouseLists)
            .FirstOrDefaultAsync(h => h.Id == (int)id);

        if (entity is null) return null;

        var house = _houseFactory.Create(
            (uint)entity.Id,
            entity.Name,
            (ushort)entity.TownId,
            (uint)entity.Rent,
            (uint)entity.OwnerId,
            entity.OwnerName ?? string.Empty,
            entity.OwnerAccountId,
            entity.PaidUntil,
            (byte)entity.Warnings
        );

        if (entity.EntryX.HasValue && entity.EntryY.HasValue && entity.EntryZ.HasValue)
            house.EntryPosition = new NeoServer.Domain.Common.Location.Structs.Location(
                (ushort)entity.EntryX.Value, (ushort)entity.EntryY.Value, (byte)entity.EntryZ.Value);

        return house;
    }

    public void Save(House house)
    {
        SaveAsync(house).GetAwaiter().GetResult();
    }
    
    private async Task SaveAsync(House house)
    {
        await using var context = NewDbContext;

        var entity = await context.Houses.FirstOrDefaultAsync(h => h.Id == (int)house.Id);
        if (entity is null)
        {
            entity = new HouseEntity
            {
                Id = (int)house.Id,
                Name = house.Name,
                Size = 0,
                Beds = 0
            };
            context.Houses.Add(entity);
        }

        entity.OwnerId = (int)house.OwnerGuid;
        entity.OwnerName = house.OwnerName;
        entity.OwnerAccountId = house.OwnerAccountId;
        entity.PaidUntil = house.PaidUntil;
        entity.Warnings = house.PayRentWarnings;
        entity.Rent = (int)house.Rent;
        entity.TownId = house.TownId;
        entity.EntryX = house.EntryPosition?.X;
        entity.EntryY = house.EntryPosition?.Y;
        entity.EntryZ = house.EntryPosition?.Z;
        await context.SaveChangesAsync();
    }

    public void SaveAccessList(uint houseId, uint listId, string text)
    {
        SaveAccessListAsync(houseId, listId, text).GetAwaiter().GetResult();
    }

    public async Task SaveTilesAsync(House house)
    {
        // Skip houses with no linked tiles — avoids wiping persisted items
        // when tile linking failed at boot (e.g., OTBM/XML mismatch).
        if (house.TileCount == 0) return;

        await using var context = NewDbContext;

        var existingTiles = await context.HouseTiles
            .Where(ht => ht.HouseId == (int)house.Id)
            .ToListAsync();

        context.HouseTiles.RemoveRange(existingTiles);

        AddHouseTileEntities(house, context);

        await context.SaveChangesAsync();
    }

    public async Task SaveTilesAsync(List<House> houses)
    {
        if (houses is null || houses.Count == 0) return;

        // Skip houses with no linked tiles — this is the critical guard.
        // Houses are added to the store before tile linking (HouseLoader.cs:86),
        // so a house whose tiles failed to link would otherwise have its DB rows
        // deleted with nothing re-inserted, permanently wiping stored items.
        var linkedHouses = new List<House>(houses.Count);
        foreach (var house in houses)
        {
            if (house.TileCount > 0)
                linkedHouses.Add(house);
        }

        if (linkedHouses.Count == 0) return;

        await using var context = NewDbContext;

        var houseIds = linkedHouses.Select(h => (int)h.Id).ToList();

        var existingTiles = await context.HouseTiles
            .Where(ht => houseIds.Contains(ht.HouseId))
            .ToListAsync();

        context.HouseTiles.RemoveRange(existingTiles);

        foreach (var house in linkedHouses)
        {
            AddHouseTileEntities(house, context);
        }

        await context.SaveChangesAsync();
    }

    private static void AddHouseTileEntities(House house, NeoContext context)
    {
        if (house.Tiles is null) return;

        foreach (var tile in house.Tiles)
        {
            if (tile.AllItems is null) continue;

            var pickupableItems = new List<IItem>(tile.AllItems.Length);
            foreach (var item in tile.AllItems)
            {
                if (item is not null && item.IsPickupable)
                    pickupableItems.Add(item);
            }

            if (pickupableItems.Count == 0) continue;

            var data = HouseTileItemSerializer.Serialize(pickupableItems);

            var entity = new HouseTileEntity
            {
                HouseId = (int)house.Id,
                TileX = tile.Location.X,
                TileY = tile.Location.Y,
                TileZ = tile.Location.Z,
                Data = data
            };
            context.HouseTiles.Add(entity);
        }
    }

    public async Task<IReadOnlyDictionary<uint, List<(Location Location, List<IItem> Items)>>> GetAllTileDataAsync()
    {
        await using var context = NewDbContext;
        var entities = await context.HouseTiles.ToListAsync();

        var result = new Dictionary<uint, List<(Location, List<IItem>)>>();
        foreach (var entity in entities)
        {
            var houseId = (uint)entity.HouseId;
            var location = new Location((ushort)entity.TileX, (ushort)entity.TileY, (byte)entity.TileZ);
            var items = HouseTileItemSerializer.Deserialize(entity.Data, _itemFactory, location);

            if (items.Count == 0) continue;

            if (!result.TryGetValue(houseId, out var list))
            {
                list = new List<(Location, List<IItem>)>();
                result[houseId] = list;
            }

            list.Add((location, items));
        }

        return new ReadOnlyDictionary<uint, List<(Location, List<IItem>)>>(result);
    }

    private async Task SaveAccessListAsync(uint houseId, uint listId, string text)
    {
        await using var context = NewDbContext;

        var entity = await context.HouseList
            .FirstOrDefaultAsync(hl => hl.HouseId == (int)houseId && hl.ListId == (int)listId);

        if (entity is null)
        {
            entity = new HouseListEntity
            {
                HouseId = (int)houseId,
                ListId = (int)listId,
                List = text
            };
            context.HouseList.Add(entity);
        }
        else
        {
            entity.List = text;
        }

        await context.SaveChangesAsync();
    }
}
