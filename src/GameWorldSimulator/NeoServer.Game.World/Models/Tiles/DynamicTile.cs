using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Game.World.Structures;

namespace NeoServer.Game.World.Models.Tiles;

public class DynamicTile : BaseTile, IDynamicTile
{
    private byte[] _cache;

    public DynamicTile(Coordinate coordinate, TileFlag tileFlag, IGround ground, IItem[] topItems, IItem[] items)
    {
        SetNewLocation(new Location((ushort)coordinate.X, (ushort)coordinate.Y, (byte)coordinate.Z));
        Flags |= (byte)tileFlag;
        AddContent(ground, topItems, items);
        TileOperationEvent.OnLoaded(this);
    }

    public byte MovementPenalty => Ground.MovementPenalty;
    public TileStack<IItem> TopItems { get; private set; }
    public TileStack<IItem> DownItems { get; private set; }

    public int ItemsCount => (DownItems?.Count ?? 0) + (TopItems?.Count ?? 0) + (Ground is null ? 0 : 1);

    public IItem[] AllItems
    {
        get
        {
            var currentIndex = 0;

            var items = new IItem[ItemsCount];

            if (Ground is not null) items[currentIndex++] = Ground;

            if (TopItems is not null)
                foreach (var topItem in TopItems)
                    items[currentIndex++] = topItem;

            if (DownItems is not null)
                foreach (var downItem in DownItems)
                    items[currentIndex++] = downItem;

            return items;
        }
    }

    public override int ThingsCount => Creatures?.Count ?? 0 + ItemsCount;

    public ushort StepSpeed => Ground.StepSpeed;

    public override ICreature TopCreatureOnStack => Creatures?.LastOrDefault();

    public bool HasHole =>
        Ground is not null &&
        Ground.Metadata.Attributes.TryGetAttribute(ItemAttribute.FloorChange, out var floorChange) &&
        floorChange == "down";

    public IGround Ground { get; private set; }
    public List<IWalkableCreature> Creatures { get; private set; }

    public bool HasCreature => (Creatures?.Count ?? 0) > 0;

    public bool HasCreatureOfType<T>() where T : ICreature
    {
        if (Creatures is null) return false;

        foreach (var creature in Creatures)
            if (creature is T)
                return true;

        return false;
    }

    public List<IPlayer> Players
    {
        get
        {
            if (Creatures is null) return new List<IPlayer>(0);
            var players = new List<IPlayer>(Creatures.Count);
            foreach (var walkableCreature in Creatures)
            {
                if (walkableCreature is not IPlayer player) continue;
                players.Add(player);
            }

            return players;
        }
    }

    public bool HasTeleport(out ITeleport teleport)
    {
        teleport = null;
        if (TopItems is null) return false;

        foreach (var topItem in TopItems)
            if (topItem is ITeleport teleportItem)
            {
                teleport = teleportItem;
                return true;
            }

        return false;
    }

    /// <summary>
    ///     Get the top item on DownItems's stack
    /// </summary>
    public override IItem TopItemOnStack => DownItems != null && DownItems.TryPeek(out var item) ? item :
        TopItems is not null && TopItems.TryPeek(out item) ? item : Ground;

    public IMagicField MagicField
    {
        get
        {
            if (!HasFlag(TileFlags.MagicField)) return null;

            foreach (var downItem in DownItems)
                if (downItem is IMagicField magicField)
                    return magicField;

            RemoveFlag(TileFlags.MagicField);
            return null;
        }
    }

    public bool HasHeight(int totalHeight)
    {
        var height = 0;

        foreach (var item in AllItems)
        {
            if (!item.Metadata.HasFlag(ItemFlag.HasHeight)) continue;
            if (totalHeight == ++height) return true;
        }

        return false;
    }

    public ICreature GetTopVisibleCreature(ICreature creature)
    {
        if (Creatures is null) return null;

        for (var i = Creatures.Count - 1; i > 0; i--)
        {
            var tileCreature = Creatures[i];

            //var tileCreature = Creatures[creatureId];
            if (creature != null)
            {
                if (creature.CanSee(tileCreature)) return tileCreature;
            }
            else
            {
                var isPlayer = tileCreature is IPlayer;

                var player = isPlayer ? tileCreature as IPlayer : null;

                if (!tileCreature.IsInvisible)
                    if (!isPlayer || !player.IsInvisible)
                        return tileCreature;
            }
        }

        return null;
    }

    public override bool TryGetStackPositionOfThing(IPlayer observer, IThing thing, out byte stackPosition)
    {
        stackPosition = default;

        if (thing is IItem item)
            return TryGetStackPositionOfItem(observer, item, out stackPosition);
        if (thing is ICreature creature)
            return TryGetStackPositionOfCreature(observer, creature, out stackPosition);

        return false;
    }

    public bool TryGetStackPositionOfItem(IItem item, out byte stackPosition)
    {
        stackPosition = 0;

        var id = item.ClientId;
        if (id == default) throw new ArgumentNullException(nameof(id));

        if (Ground?.ClientId == id) return true;
        if (Ground?.ClientId != 0) ++stackPosition;

        if (item.IsAlwaysOnTop)
        {
            foreach (var topItem in TopItems)
                if (id == topItem.ClientId)
                    return true;
                else if (++stackPosition == 10) return false;
        }
        else
        {
            stackPosition += (byte)(TopItems?.Count ?? 0);
            if (stackPosition >= 10) return false;
        }

        if (!item.IsAlwaysOnTop && DownItems is not null)
            foreach (var downItem in DownItems)
                if (id == downItem.ClientId)
                    return true;
                else if (++stackPosition >= 10) return false;

        return false;
    }

    public override byte GetCreatureStackPositionIndex(IPlayer observer)
    {
        if (Creatures is null) return 0;

        byte stackPosition = 0;
        foreach (var creature in Creatures)
            if (observer.CanSee(creature) && ++stackPosition >= 10)
                return 0;
        return stackPosition;
    }

    public bool HasBlockPathFinding => HasFlag(TileFlags.BlockPath);

    public byte[] GetRaw(IPlayer playerRequesting)
    {
        if (_cache != null && !(Creatures?.Any() ?? false)) return _cache;

        Span<byte> stream = stackalloc byte[930]; //max possible length

        var countThings = 0;
        var countBytes = 0;

        if (Ground != default)
        {
            BitConverter.GetBytes(Ground.ClientId).AsSpan().CopyTo(stream);

            countThings++;
            countBytes += 2;
        }

        if (TopItems is not null)
            foreach (var item in TopItems.Reverse()) //todo: remove reverse
            {
                if (countThings == 9) break;

                var raw = item.GetRaw();
                raw.CopyTo(stream.Slice(countBytes, raw.Length));

                countThings++;
                countBytes += raw.Length;
            }

        if (Creatures is not null)
            foreach (var creature in Creatures)
            {
                if (!playerRequesting.CanSee(creature)) continue;
                if (countThings == 9) break;

                var raw = creature.GetRaw(playerRequesting);

                raw.CopyTo(stream.Slice(countBytes, raw.Length));

                countThings++;
                countBytes += raw.Length;
            }

        if (DownItems is not null)
            foreach (var item in DownItems)
            {
                if (countThings == 9) break;

                var raw = item.GetRaw();
                raw.CopyTo(stream.Slice(countBytes, raw.Length));

                countThings++;
                countBytes += raw.Length;
            }

        _cache = stream[..countBytes].ToArray();
        return _cache;
    }

    public event AddCreatureToTile CreatureAdded;

    public IItem[] RemoveStaticItems()
    {
        if (TopItems is null) return Array.Empty<IItem>();

        var removedItems = new IItem[TopItems.Count];

        var i = 0;
        while (TopItems.TryPeek(out var topItem))
        {
            RemoveItem(topItem, topItem.Amount, 0, out var removedItem);
            removedItems[i++] = removedItem;
        }

        return removedItems;
    }

    public IItem RemoveItem(ushort id)
    {
        foreach (var item in AllItems)
            if (item.ServerId == id)
            {
                RemoveItem(item, 1, 0, out var removedItem);
                return removedItem;
            }

        return null;
    }

    public IItem RemoveItem(IItem item)
    {
        foreach (var tileItem in AllItems)
            if (item == tileItem)
            {
                RemoveItem(item, item.Amount, 0, out var removedItem);
                return removedItem;
            }

        return null;
    }

    public IItem[] RemoveAllItems()
    {
        if (DownItems is null) return Array.Empty<IItem>();

        var removedItems = new IItem[DownItems.Count];

        var i = 0;
        while (DownItems.TryPeek(out var topItem))
        {
            RemoveItem(topItem, topItem.Amount, 0, out var removedItem);
            removedItems[i++] = removedItem;
        }

        return removedItems;
    }

    public ICreature[] RemoveAllCreatures()
    {
        if (Creatures is null) return Array.Empty<ICreature>();

        var removedCreatures = new ICreature[Creatures.Count];

        var i = 0;
        while (Creatures.Any())
        {
            var creature = Creatures.First();
            RemoveCreature(creature, out var removedCreature);
            removedCreatures[i++] = removedCreature;
        }

        return removedCreatures;
    }

    public Result<IItem> RemoveTopItem(bool force = false)
    {
        if (Guard.IsNull(TopItemOnStack)) return Result<IItem>.Fail(InvalidOperation.CannotMove);

        if (!TopItemOnStack.CanBeMoved && !force) return Result<IItem>.Fail(InvalidOperation.CannotMove);

        RemoveItem(TopItemOnStack, TopItemOnStack.Amount, out var removedItem);

        return new Result<IItem>(removedItem);
    }

    public Result CanAddItem(IItem thing, byte amount = 1, byte? slot = null)
    {
        if (HasFlag(TileFlags.Depot) || HasFlag(TileFlags.HasHeight)) return Result.Success;

        if (HasFlag(TileFlags.BLockSolid)) return new Result(InvalidOperation.NotEnoughRoom);

        if (thing is null) return new Result(InvalidOperation.NotPossible);

        if (thing is IGround) return Result.Success;

        if (thing is { IsAlwaysOnTop: true } && TopItems?.Count >= 10)
            return new Result(InvalidOperation.NotEnoughRoom);

        if (thing is { IsAlwaysOnTop: false } && DownItems?.Count >= 10)
            return new Result(InvalidOperation.NotEnoughRoom);

        return Result.Success;
    }

    public bool UpdateItemType(IItem fromItem, IItemType toItemType)
    {
        if (toItemType is null) return false;
        if (fromItem.Metadata.Group != toItemType.Group) return false;

        var item = FindItem(fromItem);
        if (item is null) return false;

        item.UpdateMetadata(toItemType);

        TryGetStackPositionOfItem(fromItem, out var stackPosition);

        ResetTileFlags();
        SetTileFlags(fromItem);

        TileOperationEvent.OnChanged(this, fromItem,
            new OperationResultList<IItem>(Operation.Updated, fromItem, stackPosition));

        return true;
    }

    public void ReplaceItem(IItem fromItem, IItem toItem)
    {
        if (fromItem is IGround && toItem is IGround ground)
        {
            ReplaceGround(ground);
            return;
        }

        var isRemoved = DownItems != null ? DownItems.Remove(fromItem) : false;
        if (!isRemoved) isRemoved = TopItems != null ? TopItems.Remove(fromItem) : false;

        if (!isRemoved) return;

        if (toItem is null) return;

        if (toItem.IsAlwaysOnTop) TopItems.Push(toItem);
        else DownItems.Push(toItem);

        TryGetStackPositionOfItem(toItem, out var stackPosition);

        ResetTileFlags();
        SetTileFlags(toItem);

        TileOperationEvent.OnChanged(this, toItem,
            new OperationResultList<IItem>(Operation.Updated, toItem, stackPosition));
    }

    public void ReplaceItem(ushort fromId, IItem toItem)
    {
        IItem removed = null;

        var topItemOnStack = TopItemOnStack;

        if (topItemOnStack.ServerId != fromId) return;

        if (topItemOnStack is IGround && toItem is IGround ground)
        {
            ReplaceGround(ground);
            return;
        }

        if (topItemOnStack.IsAlwaysOnTop) TopItems.TryPop(out removed);

        DownItems?.TryPop(out removed);

        if (removed is null) return;

        if (toItem.IsAlwaysOnTop) TopItems.Push(toItem);
        else DownItems.Push(toItem);

        TryGetStackPositionOfItem(toItem, out var stackPosition);

        ResetTileFlags();
        SetTileFlags(toItem);

        TileOperationEvent.OnChanged(this, toItem,
            new OperationResultList<IItem>(Operation.Updated, toItem, stackPosition));
    }

    public uint PossibleAmountToAdd(IItem thing, byte? toPosition = null)
    {
        var freeSpace = 10 - (DownItems?.Count ?? 0);
        if (thing is not ICumulative cumulative)
        {
            if (freeSpace <= 0) return 0;
            return (uint)freeSpace;
        }

        var possibleAmountToAdd = freeSpace * 100;
        if (TopItemOnStack is ICumulative c && TopItemOnStack.ClientId == cumulative.ClientId)
            possibleAmountToAdd += c.AmountToComplete;

        return (uint)possibleAmountToAdd;
    }

    public Result<OperationResultList<IItem>> RemoveItem(IItem thing, byte amount, byte fromPosition,
        out IItem removedThing)
    {
        amount = amount == 0 ? (byte)1 : amount;
        var result = RemoveItem(thing, amount, out removedThing);
        return result;
    }

    public Result<OperationResultList<IItem>> AddItem(IItem item, byte? position = null)
    {
        var operations = AddItemToTile(item);
        if (operations.HasAnyOperation)
        {
            item.SetNewLocation(Location);
            item.SetOwner(null);
        }

        if (item is IContainer container) container.SetParent(this);

        TileOperationEvent.OnChanged(this, item, operations);
        return new Result<OperationResultList<IItem>>(operations);
    }

    public Result<uint> CanAddItem(IItemType itemType)
    {
        throw new NotImplementedException();
    }

    public void ReplaceGround(IGround ground)
    {
        AddItem(ground);
    }

    public Func<ICreature, bool> CanEnterFunction { get; set; }

    public bool CanRemoveItem(IItem thing)
    {
        if (thing is { CanBeMoved: false }) return false;

        return true;
    }

    private IItem FindItem(IItem item)
    {
        if (item is null) return null;

        foreach (var tileItem in AllItems)
            if (tileItem == item)
                return item;

        return null;
    }

    private bool TryGetStackPositionOfItem(IPlayer observer, IItem item, out byte stackPosition)
    {
        TryGetStackPositionOfItem(item, out stackPosition);

        if (stackPosition >= 10) return false;

        stackPosition = (byte)(stackPosition +
                               (item.IsAlwaysOnTop || item is IGround
                                   ? 0
                                   : GetCreatureStackPositionIndex(observer)));

        return false;
    }

    private bool TryGetStackPositionOfCreature(IPlayer observer, ICreature creature, out byte stackPosition)
    {
        stackPosition = default;

        var id = creature.CreatureId;
        if (id == default) throw new ArgumentNullException(nameof(id));

        if (Ground?.ClientId != 0) stackPosition++;

        if (TopItems is not null)
        {
            stackPosition += (byte)TopItems.Count;
            if (stackPosition >= 10) return false;
        }

        if (Creatures is not null)
            foreach (var c in Creatures)
                if (ReferenceEquals(c, creature))
                    return true;
                else if (observer.CanSee(creature))
                    if (++stackPosition >= 10)
                        return false;
        return false;
    }

    private void SetGround(IGround ground)
    {
        var operations = new OperationResultList<IItem>();

        if (Ground is not null) operations.Add(Operation.Updated, ground);
        if (Ground is null) operations.Add(Operation.Added, ground);

        Ground = ground;
        FloorDirection = ground.FloorDirection;

        TileOperationEvent.OnChanged(this, ground, operations);
    }

    public Result<OperationResultList<ICreature>> AddCreature(ICreature creature)
    {
        if (creature is not IWalkableCreature walkableCreature)
            return Result<OperationResultList<ICreature>>.NotPossible;

        if (!walkableCreature.TileEnterRule.CanEnter(this, creature))
            return Result<OperationResultList<ICreature>>.NotPossible;

        if (!CanEnterFunction?.Invoke(creature) ?? false) return Result<OperationResultList<ICreature>>.NotPossible;

        Creatures ??= new List<IWalkableCreature>();
        Creatures.Add(walkableCreature);

        walkableCreature.SetCurrentTile(this);

        SetCacheAsExpired();

        CreatureAdded?.Invoke(walkableCreature, this);
        Ground?.CreatureEntered(walkableCreature);

        return new Result<OperationResultList<ICreature>>(
            new OperationResultList<ICreature>(Operation.Added, creature));
    }

    private OperationResultList<IItem> AddItemToTile(IItem item)
    {
        var operations = new OperationResultList<IItem>();

        if (Guard.IsNull(item)) return operations;

        if (item is IGround ground)
        {
            SetGround(ground);
        }
        else
        {
            if (item.IsAlwaysOnTop)
            {
                TopItems ??= new TileStack<IItem>();

                if (TopItems.TryPeek(out var topItem) && topItem.ClientId == item.ClientId)
                {
                    operations.Add(Operation.Added, item);
                }
                else
                {
                    TopItems.Push(item);
                    operations.Add(Operation.Added, item);
                }
            }
            else
            {
                DownItems ??= new TileStack<IItem>();

                if (!DownItems.TryPeek(out var topStackItem))
                {
                    DownItems.Push(item);
                    operations.Add(Operation.Added, item);
                }
                else if (item is ICumulative cumulative &&
                         topStackItem is ICumulative topCumulative &&
                         topStackItem.ClientId == cumulative.ClientId &&
                         topCumulative.TryJoin(ref cumulative))
                {
                    operations.Add(Operation.Updated, topCumulative);

                    if (cumulative is not null)
                    {
                        DownItems.Push(cumulative);
                        operations.Add(Operation.Added, cumulative);
                    }
                }
                else
                {
                    DownItems.Push(item);
                    operations.Add(Operation.Added, item);
                }

                if (item.Metadata.Attributes.HasAttribute(ItemAttribute.Field)) SetFlag(TileFlags.MagicField);
            }
        }

        if (item is IGround) ResetTileFlags();

        SetTileFlags(item);

        SetCacheAsExpired();
        return operations;
    }

    private void AddContent(IGround ground, IItem[] topItems, IItem[] items)
    {
        if (topItems?.Length > 0) TopItems = new TileStack<IItem>();
        if (items?.Length > 0) DownItems = new TileStack<IItem>();

        if (ground != null)
        {
            Ground = ground;
            SetTileFlags(ground);
        }

        if (topItems is not null)
            foreach (var item in topItems)
            {
                TopItems.Push(item);
                SetTileFlags(item);
            }

        if (items is not null)
            foreach (var item in items)
            {
                DownItems.Push(item);
                SetTileFlags(item);
            }
    }

    private void SetCacheAsExpired()
    {
        _cache = null;
    }

    public Result<OperationResultList<ICreature>> RemoveCreature(ICreature creatureToRemove,
        out ICreature removedCreature)
    {
        Creatures ??= new List<IWalkableCreature>();
        removedCreature = null;

        if (!Creatures.Any())
            return new Result<OperationResultList<ICreature>>(
                new OperationResultList<ICreature>(Operation.None, creatureToRemove));

        var i = 0;
        foreach (var creature in Creatures)
        {
            if (creature.CreatureId == creatureToRemove.CreatureId) break;

            i++;
        }

        if (i >= Creatures.Count)
            return new Result<OperationResultList<ICreature>>(new OperationResultList<ICreature>(Operation.None,
                creatureToRemove));

        removedCreature = Creatures[i];
        Creatures.RemoveAt(i);
        SetCacheAsExpired();

        return new Result<OperationResultList<ICreature>>(new OperationResultList<ICreature>(Operation.Removed,
            creatureToRemove));
    }

    public Result<OperationResultList<IItem>> RemoveItem(IItem itemToRemove, byte amount, out IItem removedItem)
    {
        var operations = new OperationResultList<IItem>();

        TryGetStackPositionOfItem(itemToRemove, out var stackPosition);
        removedItem = null;

        if (itemToRemove.IsAlwaysOnTop)
        {
            TopItems.TryPop(out var item);
            operations.Add(Operation.Removed, item, stackPosition);
            removedItem = item;
        }
        else if (DownItems is not null && DownItems.TryPeek(out var topStackItem))
        {
            if (itemToRemove is ICumulative && topStackItem is ICumulative topCumulative)
            {
                var amountBeforeSplit = topCumulative.Amount;
                removedItem = topCumulative.Split(amount);

                if ((removedItem?.Amount ?? 0) == amountBeforeSplit)
                {
                    DownItems.TryPop(out var item);
                    operations.Add(Operation.Removed, item, stackPosition);
                }
                else
                {
                    operations.Add(Operation.Updated, topCumulative);
                }
            }
            else
            {
                DownItems.TryPop(out var item);
                operations.Add(Operation.Removed, item, stackPosition);
                removedItem = item;
            }
        }
        else if (itemToRemove == Ground)
        {
            Ground = null;
            operations.Add(Operation.Removed, itemToRemove, stackPosition);
            removedItem = itemToRemove;
        }

        SetCacheAsExpired();

        ResetTileFlags(AllItems);

        itemToRemove.OnItemRemoved(this);

        TileOperationEvent.OnChanged(this, itemToRemove, operations);
        return new Result<OperationResultList<IItem>>(operations);
    }

    internal void AddItems(IItem[] items)
    {
        foreach (var item in items) AddItem(item);
    }
}