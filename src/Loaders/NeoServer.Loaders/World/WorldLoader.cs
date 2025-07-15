using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Models;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTBM.Enums;
using NeoServer.Loaders.OTBM.Loaders;
using NeoServer.Loaders.OTBM.Structure;
using NeoServer.Loaders.OTBM.Structure.TileArea;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.World;

public class WorldLoader
{
    private readonly IItemTypeStore _itemTypeStore;
    private readonly ITileFactory _tileFactory;
    private readonly IItemFactory itemFactory;
    private readonly ILogger logger;
    private readonly ServerConfiguration serverConfiguration;
    private readonly Domain.World.World world;

    public WorldLoader(Domain.World.World world, ILogger logger, IItemFactory itemFactory,
        ServerConfiguration serverConfiguration, ITileFactory tileFactory, IItemTypeStore itemTypeStore)
    {
        this.world = world;
        this.logger = logger;
        this.itemFactory = itemFactory;
        this.serverConfiguration = serverConfiguration;
        _tileFactory = tileFactory;
        _itemTypeStore = itemTypeStore;
    }

    public void Load()
    {
        logger.Step("Loading world...", "{tiles} tiles, {towns} towns and {waypoints} waypoints loaded", () =>
        {
            using var fileStream = new FileStream($"{serverConfiguration.Data}/world/{serverConfiguration.OTBM}",
                FileMode.Open, FileAccess.Read);

            var fileBytes = new byte[fileStream.Length];
            fileStream.ReadExactly(fileBytes, 0, fileBytes.Length);

            var otbmNode = OtbBinaryTreeBuilder.Deserialize(fileBytes);

            var otbm = new OTBMNodeParser().Parse(otbmNode);

            LoadTiles(otbm);

            foreach (var townNode in otbm.Towns)
                world.AddTown(new Town
                {
                    Id = townNode.Id,
                    Name = townNode.Name,
                    Coordinate = townNode.Coordinate
                });

            foreach (var waypointNode in otbm.Waypoints)
                world.AddWaypoint(new Waypoint
                {
                    Coordinate = waypointNode.Coordinate,
                    Name = waypointNode.Name
                });

            return [world.LoadedTilesCount, world.LoadedTownsCount, world.LoadedWaypointsCount];
        });
    }

    private void LoadTiles(Otbm otbm)
    {
        foreach (var tileNode in otbm.TileAreas.SelectMany(t => t.Tiles)) LoadTile(tileNode);
    }

    private void LoadTile(TileNode tileNode)
    {
        Span<byte> raw = stackalloc byte[tileNode.Items.Count * sizeof(ushort)];
        LoadClientIdsStream(tileNode, ref raw);

        var cachedTile = _tileFactory.GetTileFromCache(tileNode.Coordinate, ref raw);

        if (cachedTile is not null)
        {
            world.AddTile(cachedTile, tileNode.Coordinate.Location);
            return;
        }

        var items = GetItemsOnTile(tileNode).ToArray();

        var tile = _tileFactory.CreateTile(tileNode.Coordinate, (TileFlag)tileNode.Flag, items, false,
            tileNode.HouseId);

        if (tile is IStaticTile)
        {
            world.AddTile(tile, tileNode.Coordinate.Location);
            return;
        }

        world.AddTile(tile);
    }

    private void LoadClientIdsStream(TileNode tileNode, ref Span<byte> clientIds)
    {
        var index = 0;

        foreach (var itemNode in tileNode.Items)
        {
            if (itemNode.ItemId is 0) continue;

            if (!_itemTypeStore.TryGetValue(itemNode.ItemId, out var itemType)) continue;

            var clientId = itemType.ClientId;

            clientIds[index++] = (byte)(clientId & 0xFF);
            clientIds[index++] = (byte)((clientId >> 8) & 0xFF);
        }
    }

    private Span<IItem> GetItemsOnTile(TileNode tileNode)
    {
        Span<IItem> items = new IItem[tileNode.Items.Count];
        var i = 0;
        foreach (var itemNode in tileNode.Items)
        {
            IDictionary<ItemAttribute, IConvertible> attributes = null;
            if (itemNode.ItemNodeAttributes != null)
            {
                attributes = new Dictionary<ItemAttribute, IConvertible>(); 
                foreach (var attr in itemNode.ItemNodeAttributes)
                {
                    var mappedAttr = MapAttribute(attr.AttributeName);
                    if (mappedAttr != null)
                        attributes.TryAdd(mappedAttr.Value, attr.Value);
                }
            }

            var children = CreateChildrenItems(tileNode, itemNode, attributes);

            var item = itemFactory.Create(itemNode.ItemId, new Location(tileNode.Coordinate), null, attributes, children);

            if (item.IsNull())
            {
                logger.Error("Failed to create item {ItemNodeItemId} on {TileNodeCoordinate}", itemNode.ItemId,
                    tileNode.Coordinate);
                continue;
            }

            // item.LoadedFromMap = true;

            if (item.CanBeMoved && tileNode.NodeType == NodeType.HouseTile)
            {
                //yield return item;
                //logger.Warning($"Movable item with ID: {itemNode.ItemType} in house at position {tileNode.Coordinate}.");
            }

            items[i++] = item;
        }

        return items;
    }



    private static ItemAttribute? MapAttribute(ItemNodeAttribute nodeAttr)
    {
        return nodeAttr switch
        {
            ItemNodeAttribute.ActionId => ItemAttribute.ActionId,
            ItemNodeAttribute.UniqueId => ItemAttribute.UniqueId,
            ItemNodeAttribute.Text => ItemAttribute.Text,
            ItemNodeAttribute.Description => ItemAttribute.Description,
            ItemNodeAttribute.WrittenDate => ItemAttribute.Date,
            ItemNodeAttribute.WrittenBy => ItemAttribute.Writer,
            ItemNodeAttribute.Name => ItemAttribute.Name,
            ItemNodeAttribute.Article => ItemAttribute.Article,
            ItemNodeAttribute.PluralName => ItemAttribute.PluralName,
            ItemNodeAttribute.Weight => ItemAttribute.Weight,
            ItemNodeAttribute.Attack => ItemAttribute.Attack,
            ItemNodeAttribute.Defense => ItemAttribute.Defense,
            ItemNodeAttribute.ExtraDefense => ItemAttribute.ExtraDefense,
            ItemNodeAttribute.Armor => ItemAttribute.Armor,
            ItemNodeAttribute.HitChance => ItemAttribute.HitChance,
            ItemNodeAttribute.ShootRange => ItemAttribute.ShootRange,
            ItemNodeAttribute.Duration => ItemAttribute.Duration,
            ItemNodeAttribute.DecayingState => ItemAttribute.DecayState,
            ItemNodeAttribute.Charges => ItemAttribute.Charges,
            ItemNodeAttribute.HouseDoorId => ItemAttribute.DoorId,
            ItemNodeAttribute.DecayTo => ItemAttribute.DecayTo,
            ItemNodeAttribute.TeleportDestination => ItemAttribute.TeleportDestination,
            _ => null
        };
    }


    private IEnumerable<IItem> CreateChildrenItems(TileNode tileNode, ItemNode itemNode,
        IDictionary<ItemAttribute, IConvertible> attributes)
    {
        var items = new List<IItem>();
        foreach (var child in itemNode.Children)
        {
            var children = CreateChildrenItems(tileNode, child, attributes);
            var item = itemFactory.Create(child.ItemId, new Location(tileNode.Coordinate), null, attributes, children);

            if (item is null) continue;
            items.Add(item);
        }

        return items;
    }
}