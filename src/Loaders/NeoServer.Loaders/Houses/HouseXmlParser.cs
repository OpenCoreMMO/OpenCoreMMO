using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Serilog;

namespace NeoServer.Loaders.Houses;

public class HouseXmlParser(ILogger logger)
{
    public List<HouseXmlData> Parse(string filePath)
    {
        if (!File.Exists(filePath)) return [];

        var doc = XDocument.Load(filePath);
        var houses = doc.Root?.Elements("house");
        if (houses is null) return [];

        var result = new List<HouseXmlData>();
        foreach (var element in houses)
        {
            var houseIdAttr = element.Attribute("houseid");
            if (houseIdAttr is null) continue;

            var houseId = (uint)houseIdAttr;
            var name = (string)element.Attribute("name") ?? string.Empty;

            var entryX = element.Attribute("entryx") is { } ex ? (ushort)(uint)ex : (ushort)0;
            var entryY = element.Attribute("entryy") is { } ey ? (ushort)(uint)ey : (ushort)0;
            var entryZ = element.Attribute("entryz") is { } ez ? (byte)(uint)ez : (byte)0;

            if (entryX == 0 && entryY == 0 && entryZ == 0)
            {
                logger.Warning(
                    "House entry not set - Name: {HouseName} - House id: {HouseId}",
                    name, houseId);
            }

            var townId = element.Attribute("townid") is { } town ? (ushort)(uint)town : (ushort)0;
            var rent = element.Attribute("rent") is { } r ? (uint)r : 0u;

            result.Add(new HouseXmlData(houseId, name, townId, rent, entryX, entryY, entryZ));
        }

        return result;
    }
}

public readonly record struct HouseXmlData(
    uint Id,
    string Name,
    ushort TownId,
    uint Rent,
    ushort EntryX,
    ushort EntryY,
    byte EntryZ
);
