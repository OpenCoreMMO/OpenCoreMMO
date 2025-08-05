using Serilog;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Core.Houses;
using NeoServer.Server.Common.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace NeoServer.Loaders.Houses;

/// <summary>
/// Startup loader that loads house data from XML files and provides them to the TileFactory for HouseTile creation
/// Executes before world loading to ensure house entities are available
/// </summary>
public class HouseStartupLoader : IRunBeforeLoaders
{
    private readonly ILogger _logger;
    private readonly ITileFactory _tileFactory;
    private readonly HousePersistenceService _persistenceService;
    private readonly string _dataPath;
    
    public HouseStartupLoader(ILogger logger, ITileFactory tileFactory)
    {
        _logger = logger;
        _tileFactory = tileFactory;
        _persistenceService = new HousePersistenceService(logger, null); // Will auto-detect connection string
        _dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "world");
    }

    public void Run()
    {
        try
        {
            Console.WriteLine("[HouseStartupLoader] ===== STARTING HOUSE LOADING =====");
            var houses = LoadHousesFromXml();
            Console.WriteLine($"[HouseStartupLoader] Loaded {houses.Count} houses from XML");
            
            foreach (var house in houses)
            {
                Console.WriteLine($"[HouseStartupLoader] House {house.Id}: {house.Name} at entry {house.Entry.X},{house.Entry.Y},{house.Entry.Z}");
                Console.WriteLine($"[HouseStartupLoader] House {house.Id} doors: [{string.Join(", ", house.Doors.Select(d => $"{d.X},{d.Y},{d.Z}"))}]");
            }
            
            // Synchronize houses to database before setting them in TileFactory
            _persistenceService.SynchronizeHousesToDatabase(houses);
            Console.WriteLine("[HouseStartupLoader] Synchronized houses to database");
            
            // Load existing ownership data from database back into house objects
            _persistenceService.LoadHouseOwnershipFromDatabase(houses);
            Console.WriteLine("[HouseStartupLoader] Loaded house ownership from database");
            
            _tileFactory.SetHouseEntities(houses);
            Console.WriteLine("[HouseStartupLoader] Set house entities in TileFactory");
            
            HouseService.RegisterHouses(houses);
            Console.WriteLine("[HouseStartupLoader] Registered houses in HouseService");
            
            _logger.Information("[HouseStartupLoader] Loaded {HouseCount} houses from XML", houses.Count);
            Console.WriteLine("[HouseStartupLoader] ===== HOUSE LOADING COMPLETED =====");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseStartupLoader] ERROR during house loading: {ex.Message}");
            Console.WriteLine($"[HouseStartupLoader] Stack trace: {ex.StackTrace}");
            _logger.Error(ex, "[HouseStartupLoader] Error loading houses from XML");
        }
    }

    private List<House> LoadHousesFromXml()
    {
        var houses = new List<House>();
        var houseFiles = new[] { "houses.xml", "small-house.xml" };

        foreach (var fileName in houseFiles)
        {
            var filePath = Path.Combine(_dataPath, fileName);
            if (!File.Exists(filePath))
            {
                _logger.Warning("[HouseStartupLoader] House file not found: {FilePath}", filePath);
                continue;
            }

            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);

                var houseNodes = doc.SelectNodes("//house");
                if (houseNodes != null)
                {
                    foreach (XmlNode houseNode in houseNodes)
                    {
                        var house = ParseHouseFromXml(houseNode);
                        if (house != null)
                        {
                            houses.Add(house);
                            _logger.Debug("[HouseStartupLoader] Loaded house {Id}: {Name} (Entry: {EntryX},{EntryY},{EntryZ})", 
                                house.Id, house.Name, house.Entry.X, house.Entry.Y, house.Entry.Z);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[HouseStartupLoader] Error parsing house file: {FilePath}", filePath);
            }
        }

        return houses;
    }

    private House ParseHouseFromXml(XmlNode houseNode)
    {
        try
        {
            var houseIdAttr = houseNode.Attributes?["id"] ?? houseNode.Attributes?["houseid"];
            var nameAttr = houseNode.Attributes?["name"];
            var entryXAttr = houseNode.Attributes?["entryx"];
            var entryYAttr = houseNode.Attributes?["entryy"];
            var entryZAttr = houseNode.Attributes?["entryz"];
            var rentAttr = houseNode.Attributes?["rent"];
            var townIdAttr = houseNode.Attributes?["townid"];
            var sizeAttr = houseNode.Attributes?["size"];

            if (houseIdAttr == null || !uint.TryParse(houseIdAttr.Value, out var houseId))
            {
                _logger.Warning("[HouseStartupLoader] Invalid or missing house ID in XML node");
                return null;
            }

            var houseName = nameAttr?.Value ?? $"House #{houseId}";
            var rent = rentAttr != null && uint.TryParse(rentAttr.Value, out var rentValue) ? rentValue : 1000;
            var size = sizeAttr != null && uint.TryParse(sizeAttr.Value, out var sizeValue) ? sizeValue : 1;
            var townId = townIdAttr != null && uint.TryParse(townIdAttr.Value, out var townValue) ? townValue : 1;

            var entryX = entryXAttr != null && ushort.TryParse(entryXAttr.Value, out var x) ? x : (ushort)1000;
            var entryY = entryYAttr != null && ushort.TryParse(entryYAttr.Value, out var y) ? y : (ushort)1000;
            var entryZ = entryZAttr != null && byte.TryParse(entryZAttr.Value, out var z) ? z : (byte)7;

            return new House
            {
                Id = houseId,
                Name = houseName,
                Owner = 0, // No owner initially
                Guests = new List<uint>(),
                SubOwners = new List<uint>(),
                Entry = new Coordinate(entryX, entryY, (sbyte)entryZ),
                TownId = townId,
                Price = rent * 10, // Price is typically rent * some multiplier
                Rent = rent,
                Size = size,
                GuildId = 0,
                PaidUntil = 0,
                Tiles = GenerateHouseTiles(entryX, entryY, entryZ, size), // Generate tiles around the entry
                Doors = new List<Coordinate> { new Coordinate(entryX, entryY, (sbyte)entryZ) } // Entry point is the door
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[HouseStartupLoader] Error parsing house XML node");
            return null;
        }
    }

    /// <summary>
    /// Generate house tiles around the entry point based on the house size
    /// This is a simplified approach - in a real scenario you'd want to read from the map data
    /// </summary>
    private List<Coordinate> GenerateHouseTiles(ushort entryX, ushort entryY, byte entryZ, uint size)
    {
        var tiles = new List<Coordinate>();
        
        // For now, just create a simple area around the entry point
        // You can customize this logic based on your house layouts
        int tileRadius = (int)Math.Sqrt(size); // Simple approximation
        
        for (int dx = -tileRadius; dx <= tileRadius; dx++)
        {
            for (int dy = -tileRadius; dy <= tileRadius; dy++)
            {
                var x = (ushort)(entryX + dx);
                var y = (ushort)(entryY + dy);
                
                // Don't add the entry point as a regular tile
                if (x == entryX && y == entryY) continue;
                
                tiles.Add(new Coordinate(x, y, (sbyte)entryZ));
            }
        }
        
        Console.WriteLine($"[HouseStartupLoader] Generated {tiles.Count} tiles for house around entry {entryX},{entryY},{entryZ}");
        return tiles;
    }
}
