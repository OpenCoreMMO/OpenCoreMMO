using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Repositories;
using Serilog;

namespace NeoServer.Data.Repositories;

public class HouseRepository : BaseRepository<HouseEntity>, IHouseRepository
{
    private readonly IHouseFactory _houseFactory;

    public HouseRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger, IHouseFactory houseFactory)
        : base(contextOptions, logger)
    {
        _houseFactory = houseFactory;
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
