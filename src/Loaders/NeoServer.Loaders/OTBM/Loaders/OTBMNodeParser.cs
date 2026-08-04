using System;
using System.Collections.Generic;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Structure;
using NeoServer.Loaders.OTBM.Structure;
using NeoServer.Loaders.OTBM.Structure.TileArea;
using NeoServer.Loaders.OTBM.Structure.Towns;

namespace NeoServer.Loaders.OTBM.Loaders;

/// <summary>
///     A class to parse <see cref="OtbNode"></see> structure to <see cref="Otbm" /> instance
/// </summary>
public sealed class OTBMNodeParser
{
    /// <summary>
    ///     Parses the OTBNode binary tree structure to a OTBM instance <see cref="Otbm.Structure.OTBM"></see>
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public Otbm Parse(OtbNode node)
    {
        Otbm otbm = new()
        {
            Header = new Header(node)
        };

        var children = node.Children;
        if (children.Length == 0) return otbm;

        var mapData = children.Span[0];
        otbm.MapData = GetMapData(mapData);

        var mapDataChildren = mapData.Children;
        var childrenCount = mapDataChildren.Length;

        var tileAreas = new List<TileArea>(childrenCount);
        var towns = new List<TownNode>(childrenCount);
        var waypoints = new List<WaypointNode>(childrenCount);

        var checkWaypoints = otbm.Header.Version > 1;

        foreach (var child in mapDataChildren.Span)
            switch (child.Type)
            {
                case NodeType.TileArea:
                    tileAreas.Add(new TileArea(child));
                    break;

                case NodeType.TownCollection:
                    var townChildren = child.Children;
                    for (var i = 0; i < townChildren.Length; i++) towns.Add(new TownNode(townChildren.Span[i]));
                    break;

                case NodeType.WayPointCollection when checkWaypoints:
                    var waypointChildren = child.Children;
                    for (var i = 0; i < waypointChildren.Length; i++)
                        waypoints.Add(new WaypointNode(waypointChildren.Span[i]));
                    break;
            }

        otbm.TileAreas = tileAreas;
        otbm.Towns = towns;
        otbm.Waypoints = waypoints;

        return otbm;
    }

    private static MapData GetMapData(OtbNode mapData)
    {
        if (mapData.Type != NodeType.MapData) throw new Exception("Could not read root data node");

        return new MapData(mapData);
    }
}