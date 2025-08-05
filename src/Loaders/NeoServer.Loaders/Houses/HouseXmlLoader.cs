using Microsoft.Extensions.Logging;
using NeoServer.Loaders.Interfaces;
using NeoServer.Server.Configurations;
using System;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Loaders.Houses;

/// <summary>
/// Loader for house XML files that follows the original OpenTibia format
/// Logs house information found in XML files during server startup
/// </summary>
public class HouseXmlLoader : IStartupLoader
{
    private readonly ServerConfiguration _serverConfiguration;

    public HouseXmlLoader(ServerConfiguration serverConfiguration)
    {
        _serverConfiguration = serverConfiguration;
    }
    
    public void Load()
    {
        try
        {
            var worldDir = $"{_serverConfiguration.Data}/world";
            
            // Look for house XML files with various naming patterns
            var houseFiles = new[]
            {
                "houses.xml",
                "small-house.xml", // matches the small.otbm map
                "*house*.xml"
            };

            var loadedCount = 0;
            foreach (var pattern in houseFiles)
            {
                var files = Directory.GetFiles(worldDir, pattern, SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    loadedCount += LoadHousesFromFile(file);
                }
            }

            if (loadedCount > 0)
            {
                Console.WriteLine($"[HouseXmlLoader] Successfully found {loadedCount} houses in XML files");
            }
            else
            {
                Console.WriteLine($"[HouseXmlLoader] No house XML files found in {worldDir}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseXmlLoader] Error loading house XML files: {ex.Message}");
        }
    }
    private int LoadHousesFromFile(string filePath)
    {
        try
        {
            Console.WriteLine($"[HouseXmlLoader] Loading houses from {filePath}");
            
            var doc = new XmlDocument();
            doc.Load(filePath);
            
            var houseNodes = doc.SelectNodes("//house");
            if (houseNodes == null)
            {
                Console.WriteLine($"[HouseXmlLoader] No house nodes found in {filePath}");
                return 0;
            }

            var loadedCount = 0;
            foreach (XmlNode houseNode in houseNodes)
            {
                if (LoadHouseFromXml(houseNode))
                {
                    loadedCount++;
                }
            }
            
            return loadedCount;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseXmlLoader] Error loading houses from file {filePath}: {ex.Message}");
            return 0;
        }
    }
    
    private bool LoadHouseFromXml(XmlNode houseNode)
    {
        try
        {
            // Read house attributes
            var houseIdAttr = houseNode.Attributes?["houseid"];
            var nameAttr = houseNode.Attributes?["name"];
            var entryXAttr = houseNode.Attributes?["entryx"];
            var entryYAttr = houseNode.Attributes?["entryy"];
            var entryZAttr = houseNode.Attributes?["entryz"];
            var rentAttr = houseNode.Attributes?["rent"];
            var townIdAttr = houseNode.Attributes?["townid"];
            var sizeAttr = houseNode.Attributes?["size"];
            
            if (houseIdAttr == null || !uint.TryParse(houseIdAttr.Value, out var houseId))
            {
                Console.WriteLine("[HouseXmlLoader] Invalid or missing house ID in XML node");
                return false;
            }
            
            var houseName = nameAttr?.Value ?? $"House #{houseId}";
            var rent = rentAttr != null && uint.TryParse(rentAttr.Value, out var rentValue) ? rentValue : 0;
            var size = sizeAttr != null && uint.TryParse(sizeAttr.Value, out var sizeValue) ? sizeValue : 0;
            var townId = townIdAttr != null && uint.TryParse(townIdAttr.Value, out var townValue) ? townValue : 1;
            
            // Parse entry position
            var entryX = entryXAttr != null && ushort.TryParse(entryXAttr.Value, out var x) ? x : (ushort)0;
            var entryY = entryYAttr != null && ushort.TryParse(entryYAttr.Value, out var y) ? y : (ushort)0;
            var entryZ = entryZAttr != null && byte.TryParse(entryZAttr.Value, out var z) ? z : (byte)0;
            
            Console.WriteLine($"[HouseXmlLoader] Found house {houseId} - {houseName} (Entry: {entryX},{entryY},{entryZ} Rent: {rent}, Size: {size}, Town: {townId})");
            
            // Process house tiles and doors from XML child nodes
            
            // Process house tiles and doors from XML child nodes
            ProcessHouseChildren(houseNode, houseId);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseXmlLoader] Error loading house from XML node: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Processes child elements of a house node (tiles, doors, beds)
    /// </summary>
    private void ProcessHouseChildren(XmlNode houseNode, uint houseId)
    {
        // Process house tiles
        var tileNodes = houseNode.SelectNodes(".//tile");
        if (tileNodes != null)
        {
            foreach (XmlNode tileNode in tileNodes)
            {
                ProcessHouseTile(tileNode, houseId);
            }
        }
        
        // Process house doors  
        var doorNodes = houseNode.SelectNodes(".//door");
        if (doorNodes != null)
        {
            foreach (XmlNode doorNode in doorNodes)
            {
                ProcessHouseDoor(doorNode, houseId);
            }
        }
        
        // Process house beds
        var bedNodes = houseNode.SelectNodes(".//bed");
        if (bedNodes != null)
        {
            foreach (XmlNode bedNode in bedNodes)
            {
                ProcessHouseBed(bedNode, houseId);
            }
        }
    }
    
    /// <summary>
    /// Processes a house tile XML node
    /// </summary>
    private void ProcessHouseTile(XmlNode tileNode, uint houseId)
    {
        try
        {
            var xAttr = tileNode.Attributes?["x"];
            var yAttr = tileNode.Attributes?["y"];
            var zAttr = tileNode.Attributes?["z"];
            
            if (xAttr != null && yAttr != null && zAttr != null &&
                ushort.TryParse(xAttr.Value, out var x) &&
                ushort.TryParse(yAttr.Value, out var y) &&
                byte.TryParse(zAttr.Value, out var z))
            {
                Console.WriteLine($"[HouseXmlLoader] Found house tile for house {houseId} at position ({x},{y},{z})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseXmlLoader] Error processing house tile for house {houseId}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Processes a house door XML node
    /// </summary>
    private void ProcessHouseDoor(XmlNode doorNode, uint houseId)
    {
        try
        {
            var xAttr = doorNode.Attributes?["x"];
            var yAttr = doorNode.Attributes?["y"];
            var zAttr = doorNode.Attributes?["z"];
            var doorIdAttr = doorNode.Attributes?["doorid"];
            
            if (xAttr != null && yAttr != null && zAttr != null &&
                ushort.TryParse(xAttr.Value, out var x) &&
                ushort.TryParse(yAttr.Value, out var y) &&
                byte.TryParse(zAttr.Value, out var z))
            {
                var doorId = doorIdAttr?.Value ?? "0";
                Console.WriteLine($"[HouseXmlLoader] Found house door for house {houseId} at position ({x},{y},{z}) with doorId {doorId}");
                
                // Note: Door positions are processed during house loading by HouseStartupLoader
                // This loader is just for debugging/logging purposes
                // The actual door position registration happens in HouseStartupLoader.LoadHousesFromXml()
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseXmlLoader] Error processing house door for house {houseId}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Processes a house bed XML node
    /// </summary>
    private void ProcessHouseBed(XmlNode bedNode, uint houseId)
    {
        try
        {
            var xAttr = bedNode.Attributes?["x"];
            var yAttr = bedNode.Attributes?["y"];
            var zAttr = bedNode.Attributes?["z"];
            var bedIdAttr = bedNode.Attributes?["bedid"];
            
            if (xAttr != null && yAttr != null && zAttr != null &&
                ushort.TryParse(xAttr.Value, out var x) &&
                ushort.TryParse(yAttr.Value, out var y) &&
                byte.TryParse(zAttr.Value, out var z))
            {
                var bedId = bedIdAttr?.Value ?? "0";
                Console.WriteLine($"[HouseXmlLoader] Found house bed for house {houseId} at position ({x},{y},{z}) with bedId {bedId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseXmlLoader] Error processing house bed for house {houseId}: {ex.Message}");
        }
    }
}
