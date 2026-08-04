using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.World.Events;
using NeoServer.Domain.World.Structures;

namespace NeoServer.Domain.World.Models.Tiles;

public class DynamicTile : BaseTile, IDynamicTile
{
    private byte[] _cache;

    public DynamicTile(Coordinate coordinate, TileFlag tileFlag, IGround ground, IItem[] topItems, IItem[] items,
        uint? houseId = null)
    {
        SetNewLocation(new Location((ushort)coordinate.X, (ushort)coordinate.Y, (byte)coordinate.Z));
        Flags |= (uint)tileFlag;
        AddContent(ground, topItems, items);
        HouseId = houseId;
        
        EventAggregator.Invoke(new TileLoadedEvent(this));
    }

    public byte MovementPenalty => Ground.MovementPenalty;
    public TileStack<IItem> TopItems { get; private set; }
    public TileStack<IItem> DownItems { get; private set; }

    public uint? HouseId { get; private set; }

    public override int ItemsCount => (DownItems?.Count ?? 0) + (TopItems?.Count ?? 0) + (Ground is null ? 0 : 1);

    public override IItem[] AllItems
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

    public override int ThingsCount => (Creatures?.Count ?? 0) + ItemsCount;

    public ushort StepSpeed => Ground.StepSpeed;

    public override ICreature TopCreatureOnStack => Creatures?.LastOrDefault();

    public bool HasHole =>
        Ground is not null &&
        Ground.Metadata.Attributes.TryGetAttribute(ItemTypeAttribute.FloorChange, out var floorChange) &&
        floorChange == "down";

    public IGround Ground { get; private set; }
    public List<IWalkableCreature> Creatures { get; private set; }

    public bool HasAnyCreature => (Creatures?.Count ?? 0) > 0;

    public bool HasCreatureOfType<T>() where T : ICreature
    {
        if (Creatures is null) return false;

        foreach (var creature in Creatures)
            if (creature is T)
                return true;

        return false;
    }

    public bool HasCreature(ICreature creature)
    {
        if (Creatures is null) return false;

        foreach (var tileCreature in Creatures)
            if (tileCreature.Equals(creature))
                return true;

        return false;
    }

    public List<IPlayer> Players
    {
        get
        {
            if (Creatures is null) return [];
            var players = new List<IPlayer>(Creatures.Count);
            foreach (var walkableCreature in Creatures)
            {
                if (walkableCreature is not IPlayer player) continue;
                players.Add(player);
            }

            return players;
        }
    }

    public bool HasTeleport(out TeleportItem teleport)
    {
        teleport = null;
        if (TopItems is null) return false;

        foreach (var topItem in TopItems)
            if (topItem is TeleportItem teleportItem)
            {
                teleport = teleportItem;
                return true;
            }

        return false;
    }

    /// <summary>
    ///     Get the top item on TopItems's stack
    /// </summary>
    public override IItem TopTopItemOnStack => TopItems is not null && TopItems.TryPeek(out var item) ? item :
        DownItems is not null && DownItems.TryPeek(out item) ? item : Ground;

    /// <summary>
    ///     Get the top item on DownItems's stack
    /// </summary>
    public override IItem TopDownItemOnStack => DownItems != null && DownItems.TryPeek(out var item) ? item :
        TopItems is not null && TopItems.TryPeek(out item) ? item : Ground;

    public MagicField MagicField
    {
        get
        {
            if (!HasFlag(TileFlags.MagicField)) return null;

            foreach (var downItem in DownItems)
                if (downItem is MagicField magicField)
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

    public override IItem GetItemByIndex(int index)
    {
        // Check if ground exists and the index is 0
        if (Ground != null)
        {
            if (index == 0) return Ground;

            // Decrement index since we've accounted for ground
            index--;
        }

        // Check top items
        if (TopItems is { Count: > 0 })
        {
            if (index < TopItems.Count) return TopItems.Values.ElementAt(index);

            // Decrement index by the number of top items
            index -= TopItems.Count;
        }

        // Skip creatures in the index calculation since we're only returning items
        // But we need to account for their presence in the stack
        if (Creatures is { Count: > 0 })
            // Decrement index by the number of creatures
            index -= Creatures.Count;

        // Check down items
        if (DownItems != null && index >= 0 && index < DownItems.Count) return DownItems.ElementAt(index);

        // Index out of range
        return null;
    }

    public ICreature GetTopVisibleCreature(ICreature creature)
    {
        if (Creatures is null) return null;

        foreach (var tileCreature in Creatures)
            if (creature != null)
            {
                if (creature.CanSee(tileCreature)) return tileCreature;
            }
            else
            {
                var isPlayer = tileCreature is IPlayer;

                var player = isPlayer ? tileCreature as IPlayer : null;

                if (!tileCreature.IsInvisible && (!isPlayer || !player.IsInvisible)) return tileCreature;
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
        if (item is null) return false;


        if (Ground is not null)
        {
            if (ReferenceEquals(Ground, item)) return true;
            stackPosition++;
        }

        if (item.IsAlwaysOnTop)
        {
            if (TopItems is null) return false;

            foreach (var topItem in TopItems.Values)
            {
                if (ReferenceEquals(topItem, item)) return true;
                if (++stackPosition >= 10) return false;
            }

            return false;
        }

        stackPosition += (byte)(TopItems?.Count ?? 0);
        if (stackPosition >= 10) return false;

        if (DownItems is null) return false;

        foreach (var downItem in DownItems)
        {
            if (ReferenceEquals(downItem, item)) return true;
            if (++stackPosition >= 10) return false;
        }

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
        // Only use the cache if there are no creatures (as they change frequently)
        if (_cache != null && (Creatures == null || Creatures.Count == 0))
            return _cache;

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
            foreach (var item in TopItems.Values) //todo: remove reverse
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

        var result = stream[..countBytes].ToArray();

        // Only caches if there are no creatures
        if (Creatures == null || Creatures.Count == 0)
            _cache = result;

        return result;
    }

    public event AddCreatureToTile CreatureAdded;

    public IItem[] RemoveStaticItems()
    {
        if (TopItems is null) return [];

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

    public IItem RemoveItem(ItemGroup group)
    {
        foreach (var item in AllItems)
            if (item.Metadata.Group == group)
            {
                RemoveItem(item, 1, 0, out var removedItem);
                return removedItem;
            }

        return null;
    }

    public IItem RemoveItem(IItem item)
    {
        foreach (var tileItem in AllItems)
            if (item.ServerId == tileItem.ServerId)
            {
                RemoveItem(item, item.Amount, 0, out var removedItem);
                return removedItem;
            }

        return null;
    }

    public IItem[] RemoveAllItems()
    {
        if (DownItems is null) return [];

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
        if (Creatures is null) return [];

        var removedCreatures = new ICreature[Creatures.Count];

        var i = 0;
        while (Creatures.Count != 0)
        {
            var creature = Creatures.First();
            RemoveCreature(creature, out var removedCreature);
            removedCreatures[i++] = removedCreature;
        }

        return removedCreatures;
    }

    public Result<IItem> RemoveTopItem(bool force = false)
    {
        if (Guard.IsNull(TopDownItemOnStack)) return Result<IItem>.Fail(InvalidOperation.CannotMove);

        if (!TopDownItemOnStack.CanBeMoved && !force) return Result<IItem>.Fail(InvalidOperation.CannotMove);

        RemoveItem(TopDownItemOnStack, TopDownItemOnStack.Amount, out var removedItem);

        return new Result<IItem>(removedItem);
    }

    public Result CanAddItem(IItem thing, byte amount = 1, byte? slot = null)
    {
        if (HasFlag(TileFlags.Depot) || HasFlag(TileFlags.HasHeight)) return Result.Success;

        if (HasFlag(TileFlags.Unpassable)) return new Result(InvalidOperation.NotEnoughRoom);

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

        // TransformThing cannot express a layer change. Remove then re-add so clients get
        // correct stack positions (0x6C + 0x6A). Same-layer transforms keep an in-place update.
        var wasAlwaysOnTop = fromItem.IsAlwaysOnTop;
        var willBeAlwaysOnTop = toItemType.HasFlag(ItemFlag.AlwaysOnTop) || toItemType.HasFlag(ItemFlag.Hangable);
        var alwaysOnTopChanged = wasAlwaysOnTop != willBeAlwaysOnTop;

        if (alwaysOnTopChanged)
        {
            RemoveItem(item, item.Amount == 0 ? (byte)1 : item.Amount, out var removedItem);
            if (removedItem is null)
            {
                return false;
            }

            removedItem.UpdateMetadata(toItemType);
            AddItem(removedItem);
            return true;
        }

        TryGetStackPositionOfItem(fromItem, out var stackPosition);

        item.UpdateMetadata(toItemType);

        ResetTileFlags();
        SetTileFlags(fromItem);

        EventAggregator.Invoke(new TileChangedEvent(this, fromItem,
            new OperationResultList<IItem>(Operation.Updated, fromItem, stackPosition)));

        return true;
    }

    public void ReplaceItem(IItem fromItem, IItem toItem)
    {
        if (fromItem is IGround && toItem is IGround ground)
        {
            ReplaceGround(ground);
            return;
        }

        // Prefer exact instance; fall back to ServerId for static→dynamic recreate mismatches.
        IItem itemToRemove = FindItem(fromItem);
        if (itemToRemove is null)
        {
            itemToRemove = DownItems?.FirstOrDefault(c => c.ServerId == fromItem.ServerId)
                           ?? TopItems?.FirstOrDefault(c => c.ServerId == fromItem.ServerId);
        }

        if (itemToRemove is null) return;

        var alwaysOnTopChanged = toItem is not null && itemToRemove.IsAlwaysOnTop != toItem.IsAlwaysOnTop;

        // Layer changes must Remove+Add (TFS). Same-layer can update in place.
        if (alwaysOnTopChanged || toItem is null)
        {
            RemoveItem(itemToRemove, itemToRemove.Amount == 0 ? (byte)1 : itemToRemove.Amount, out _);
            if (toItem is not null)
            {
                AddItem(toItem);
            }

            return;
        }

        if (itemToRemove.IsAlwaysOnTop)
        {
            if (TopItems is null || !TopItems.Remove(itemToRemove)) return;
            TopItems.Push(toItem);
        }
        else
        {
            if (DownItems is null || !DownItems.Remove(itemToRemove)) return;
            DownItems.Push(toItem);
        }

        toItem.SetNewLocation(Location);
        TryGetStackPositionOfItem(toItem, out var stackPosition);

        ResetTileFlags();
        SetTileFlags(toItem);
        SetCacheAsExpired();

        EventAggregator.Invoke(new TileChangedEvent(this, toItem,
            new OperationResultList<IItem>(Operation.Updated, toItem, stackPosition)));
    }

    public void ReplaceItem(ushort fromId, IItem toItem)
    {
        IItem removed = null;

        var topItemOnStack = TopDownItemOnStack;

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
        else DownItems?.Push(toItem);

        TryGetStackPositionOfItem(toItem, out var stackPosition);

        ResetTileFlags();
        SetTileFlags(toItem);

        EventAggregator.Invoke(new TileChangedEvent(this, toItem,
            new OperationResultList<IItem>(Operation.Updated, toItem, stackPosition)));
    }

    /// <summary>
    ///     Replaces an existing item on the tile by removing all items belonging to the same group
    ///     and then adding the specified item.
    /// </summary>
    /// <param name="item">The item to add, which will replace any existing items of the same group.</param>
    public void ReplaceItemByGroup(IItem item)
    {
        RemoveItem(item.Metadata.Group);
        AddItem(item);
    }

    public uint PossibleAmountToAdd(IItem thing, byte? toPosition = null)
    {
        var freeSpace = 10 - (DownItems?.Count ?? 0);
        if (thing is not ICumulative cumulative)
        {
            if (freeSpace <= 0) return 0;
            return (uint)freeSpace;
        }

        if (TopDownItemOnStack is ICumulative c &&
            TopDownItemOnStack.ClientId == cumulative.ClientId
            && c.AmountToComplete > 0)
            return c.AmountToComplete;

        var possibleAmountToAdd = freeSpace * 100;
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
        if (operations?.HasAnyOperation ?? false)
        {
            item.SetNewLocation(Location);
            item.SetOwner(null);
        }

        if (item is IContainer container) container.SetParent(this);

        EventAggregator.Invoke(new TileChangedEvent(this, item, operations));
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
        if (!TryGetStackPositionOfItem(item, out stackPosition) || stackPosition >= 10)
        {
            return false;
        }

        if (!item.IsAlwaysOnTop && item is not IGround)
        {
            stackPosition = (byte)(stackPosition + GetCreatureStackPositionIndex(observer));
        }

        return true;
    }

    private bool TryGetStackPositionOfCreature(IPlayer observer, ICreature creature, out byte stackPosition)
    {
        stackPosition = 0;

        var id = creature.CreatureId;
        if (id == 0) throw new ArgumentNullException(nameof(id));

        if (Ground is not null) stackPosition++;

        if (TopItems is not null)
        {
            stackPosition += (byte)TopItems.Count;
            if (stackPosition >= 10) return false;
        }

        if (Creatures is not null)
            foreach (var c in Creatures)
                if (ReferenceEquals(c, creature))
                    return true;
                else if (observer.CanSee(c))
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

        EventAggregator.Invoke(new TileChangedEvent(this, ground, operations));
    }

    public Result<OperationResultList<ICreature>> AddCreature(ICreature creature, bool forced = false)
    {
        if (creature is not IWalkableCreature walkableCreature)
            return Result<OperationResultList<ICreature>>.NotPossible;

        if (!forced && !walkableCreature.TileEnterRule.CanEnter(this, creature))
            return Result<OperationResultList<ICreature>>.NotPossible;

        if (!forced && (!CanEnterFunction?.Invoke(creature) ?? false))
            return Result<OperationResultList<ICreature>>.NotPossible;

        Creatures ??= [];
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
                AddTopItem(item, operations);
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
                    // Capture stackpos before any overflow remainder is pushed (TFS updateThing-before-addThing).
                    TryGetStackPositionOfItem(topCumulative, out var stackPosition);
                    operations.Add(Operation.Updated, topCumulative, stackPosition);

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

                if (item.Metadata.Attributes.HasAttribute(ItemTypeAttribute.Field)) SetFlag(TileFlags.MagicField);
            }
        }

        if (item is IGround) ResetTileFlags();

        SetTileFlags(item);

        SetCacheAsExpired();
        return operations;
    }

    private void AddTopItem(IItem item, OperationResultList<IItem> operations)
    {
        TopItems ??= new TileStack<IItem>();

        //loop stack from beginning to the end in ascending order
        foreach (var itemOnStack in TopItems.Values)
            if (item.Metadata.TopOrder <= itemOnStack.Metadata.TopOrder)
            {
                //item will be inserted before itemOnStack
                TopItems.Insert(item, itemOnStack);
                operations.Add(Operation.Added, item);
                return;
            }

        TopItems.Push(item);
        operations.Add(Operation.Added, item);
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
            foreach (var item in topItems.OrderBy(i => i.Metadata.TopOrder))
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
        Creatures ??= [];
        removedCreature = null;

        if (Creatures.Count == 0)
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
            if (TopItems is null || !TopItems.Remove(itemToRemove))
            {
                // Fall back if flag/stack disagree (e.g. after a prior bad transform).
                if (DownItems is null || !DownItems.Remove(itemToRemove))
                {
                    return new Result<OperationResultList<IItem>>(operations);
                }
            }

            operations.Add(Operation.Removed, itemToRemove, stackPosition);
            removedItem = itemToRemove;
        }
        else if (DownItems is not null && DownItems.TryPeek(out var topStackItem) &&
                 itemToRemove is ICumulative && topStackItem is ICumulative topCumulative &&
                 ReferenceEquals(topStackItem, itemToRemove))
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
                operations.Add(Operation.Updated, topCumulative, stackPosition);
            }
        }
        else if (DownItems is not null && DownItems.Remove(itemToRemove))
        {
            operations.Add(Operation.Removed, itemToRemove, stackPosition);
            removedItem = itemToRemove;
        }
        else if (TopItems is not null && TopItems.Remove(itemToRemove))
        {
            operations.Add(Operation.Removed, itemToRemove, stackPosition);
            removedItem = itemToRemove;
        }
        else if (itemToRemove == Ground)
        {
            Ground = null;
            operations.Add(Operation.Removed, itemToRemove, stackPosition);
            removedItem = itemToRemove;
        }

        if (removedItem is null && operations.HasAnyOperation is false)
        {
            return new Result<OperationResultList<IItem>>(operations);
        }

        SetCacheAsExpired();

        ResetTileFlags(AllItems);

        itemToRemove.OnItemRemoved(this);

        EventAggregator.Invoke(new TileChangedEvent(this, itemToRemove, operations));
        return new Result<OperationResultList<IItem>>(operations);
    }

    internal void AddItems(IItem[] items)
    {
        foreach (var item in items) AddItem(item);
    }
}