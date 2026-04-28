# C# Native Event Migration to EventAggregator Records

This skill describes how to migrate a C# native event (delegate-based, subscribed with `+=`) to an EventAggregator record-based event (`IEvent`).

## Overview

The codebase has two event systems:

| System | Mechanism | Auto-discovery |
|---|---|---|
| **Legacy (native)** | `delegate` + `event` keyword + manual `+=` subscription in subscriber classes | No — must manually subscribe/unsubscribe per instance |
| **New (EventAggregator)** | `record` implementing `IEvent` + handler implementing `IApplicationEventHandler<T>` | Yes — scanned via `GameAssemblyCache` at `EventAggregator.Initialize()` |

The migration goal: replace manual delegate subscriptions with auto-discovered EventAggregator handlers.

## Migration Checklist

For each native event being migrated:

- [ ] 1. Create the event record in the domain events folder
- [ ] 2. Create handler(s) in the appropriate layer(s)
- [ ] 3. Replace `delegate.Invoke()` with `EventAggregator.Invoke()` in the entity
- [ ] 4. Remove the `event` declaration and delegate type from the entity and its interface
- [ ] 5. Remove `+=` / `-=` subscribe/unsubscribe lines from subscriber classes
- [ ] 6. Remove the handler class (if it was created solely for the delegate) or convert it to `IApplicationEventHandler<T>`

---

## Step 1: Create the Event Record

### Location

`src/Core/NeoServer.Domain/Creatures/Events/Player/` (for Player-related events)

Convention: the event is a `record` in the `NeoServer.Domain.Creatures.Events.Player` namespace.

### Pattern

```csharp
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerSomeEvent(IPlayer Player, IItem Item) : IEvent;
```

**Rules:**
- Must implement `IEvent` (from `NeoServer.Domain.Common`)
- Use `record` type
- Keep same fields from original delegate parameters
- Namespace mirrors folder path
- File name matches record name

### Existing examples

| Event Record | File |
|---|---|
| `PlayerLoggedInEvent` | `PlayerLoggedInEvent.cs` |
| `PlayerLoggedOutEvent` | `PlayerLoggedOutEvent.cs` |
| `PlayerWalkEvent` | `PlayerWalkEvent.cs` |
| `PlayerEquippedItemEvent` | `PlayerEquippedItemEvent.cs` |
| `PlayerUnequippedItemEvent` | `PlayerUnequippedItemEvent.cs` |
| `PlayerReadTextEvent` | `PlayerReadTextEvent.cs` |
| `PlayerStorageUpdateEvent` | `PlayerStorageUpdateEvent.cs` |
| `PlayerThinkEvent` | `PlayerThinkEvent.cs` |
| `PlayerInventoryUpdateEvent` | `PlayerInventoryUpdateEvent.cs` |
| `PlayerRotateItemEvent` | `PlayerRotateItemEvent.cs` |

---

## Step 2: Create Handler(s)

### Handler Locations by Layer

| Layer | Interface | Location |
|---|---|---|
| **Network** | `INetworkingEventHandler<T>` | `src/NetworkingServer/NeoServer.Networking/EventHandlers/` |
| **Application** | `IApplicationEventHandler<T>` | `src/ApplicationServer/NeoServer.Server.Events/` |
| **Lua** | `IApplicationEventHandler<T>` | `src/Extensions/NeoServer.Scripts.LuaJIT/Events/` |

### Network Handler Pattern

```csharp
using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerSomeEventHandler : INetworkingEventHandler<PlayerSomeEvent>
{
    public void Handle(PlayerSomeEvent @event)
    {
        // Network logic — sends packets, etc.
    }
}
```

**Note:** `INetworkingEventHandler<T>` extends `IApplicationEventHandler<T>`, so it implements both. During `EventAggregator.Initialize()`, network handlers are prioritized and run before application handlers.

### Application Handler Pattern

```csharp
using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;

namespace NeoServer.Server.Events.Player;

public class PlayerSomeEventHandler : IApplicationEventHandler<PlayerSomeEvent>
{
    public void Handle(PlayerSomeEvent @event)
    {
        if (@event is null) return;
        // Application logic
    }
}
```

### Lua Handler Pattern

```csharp
using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerSomeEventHandler(ICreatureEvents creatureEvents) : IApplicationEventHandler<PlayerSomeEvent>
{
    public void Handle(PlayerSomeEvent @event)
    {
        creatureEvents.SomeMethod(@event.Player);
    }
}
```

For Lua callback events (Revscript), use the `Callbacks/Player` folder:

```csharp
using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Callbacks.Player;

public class PlayerSomeEventHandler(IEventsCallbacks eventsCallbacks)
    : IApplicationEventHandler<PlayerSomeEvent>
{
    public void Handle(PlayerSomeEvent @event)
    {
        eventsCallbacks.ExecuteCallback(
            EventCallbackType.PlayerOnSomeEvent,
            callback =>
                callback.PlayerOnSomeEvent(@event.Player));
    }
}
```

### Handler Placement Rule (Network vs Application)

When migrating, examine the handler's logic to determine the correct layer:

- **If the handler sends packets, accesses connections, or does any networking I/O** → place it in the **Network layer** (`INetworkingEventHandler<T>` in `src/NetworkingServer/NeoServer.Networking/EventHandlers/`)
- **If the handler only does game logic** → place it in the **Application layer** (`IApplicationEventHandler<T>` in `src/ApplicationServer/NeoServer.Server.Events/`)

Existing application-layer handlers that already send networking packets must be moved to the networking layer during migration. This ensures proper architectural separation: the application layer should not depend on networking concerns.

### Handler Discovery

Handlers are auto-discovered by `EventAggregator.Initialize()` via assembly scanning:

```csharp
GameAssemblyCache.Cache
    .Where(type => typeof(IApplicationEventHandler).IsAssignableFrom(type))
    .Where(type => !type.IsAbstract && !type.IsEnum && !type.IsInterface)
    .SelectMany(type => type.GetInterfaces()
        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IApplicationEventHandler<>))
        .Select(i => new { EventTypeFullName = i.GetGenericArguments().First().FullName, HandlerType = type }))
```

No manual registration is needed — handlers are resolved from DI (`IServiceProvider.GetService(x)`) at initialization time.

### Execution Order

1. **Network handlers** (`INetworkingEventHandler<T>`) — run first, immediately
2. **Application handlers** (`IApplicationEventHandler<T>`) — run second, deferred to `PropagateEvents()`

---

## Step 3: Replace Delegate Invocation with EventAggregator.Invoke

### Before (native delegate)

```csharp
// In the entity class (e.g., Player.cs)
public event PlayerLevelAdvance OnLevelAdvanced;

public void SomeMethod()
{
    OnLevelAdvanced?.Invoke(this, arg1, arg2);
}
```

### After (EventAggregator)

```csharp
// In the entity class (e.g., Player.cs)
public void SomeMethod()
{
    EventAggregator.Invoke(new PlayerLevelAdvancedEvent(this, arg1, arg2));
}
```

**Important:**
- The static `EventAggregator.Invoke(IEvent)` calls `InvokeEvent` immediately (not queued)
- The `EventAggregator.Instance` is set in the constructor (singleton per app)
- No `?.Invoke` null check needed — if no handlers, EventAggregator is a no-op

### Handling Events with No Subscribers

If a delegate-based event has **no subscribers** in any layer (e.g., `OnChangedChaseMode`, `OnCompleteWalking`):

1. Still create the event record
2. Still replace `delegate.Invoke()` with `EventAggregator.Invoke(new ...)`
3. **Do NOT create any handler classes**
4. The EventAggregator will dispatch to an empty handler list — a safe no-op
5. This keeps the door open for future handlers without touching entity code

---

## Step 4: Remove the Event Declaration

### Before

In the entity class (e.g., `Player.cs`):

```csharp
public event PlayerLevelAdvance OnLevelAdvanced;
```

In the interface (e.g., `IPlayer.cs`):

```csharp
public event PlayerLevelAdvance OnLevelAdvanced;
```

Also remove the delegate type definition if it's no longer used anywhere:

```csharp
public delegate void PlayerLevelAdvance(Player player, SkillType type, int fromLevel, int toLevel);
```

### After

Both the `event` field and the `delegate` type are deleted entirely.

---

## Step 5: Remove Manual Subscriptions

### Before (in subscriber class)

```csharp
// PlayerEventSubscriber.Subscribe()
player.OnLevelAdvanced += playerLevelAdvancedEventHandler.Execute;

// PlayerEventSubscriber.Unsubscribe()
player.OnLevelAdvanced -= playerLevelAdvancedEventHandler.Execute;
```

### After

Delete both the `+=` and `-=` lines from the subscriber class.

If the handler class was only used for this delegate (and had no other purpose), it can be deleted. If it was used for multiple events, keep it but remove the relevant method.

### Subscriber Files to Check

| Subscriber | Layer | File |
|---|---|---|
| `PlayerEventSubscriber` | Application | `src/ApplicationServer/NeoServer.Server.Events/Subscribers/PlayerEventSubscriber.cs` |
| `CreatureEventSubscriber` | Application | `src/ApplicationServer/NeoServer.Server.Events/Subscribers/CreatureEventSubscriber.cs` |
| `CreatureEventsSubscriber` | Lua | `src/Extensions/NeoServer.Scripts.LuaJIT/Events/CreatureEventsSubscriber.cs` |

---

## Complete Migration Example

### Target: Player `OnStatusChanged`

**1. Create the event record:**

```csharp
// src/Core/NeoServer.Domain/Creatures/Events/Player/PlayerStatusChangedEvent.cs
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerStatusChangedEvent(IPlayer Player) : IEvent;
```

**2. Convert the existing handler:**

```csharp
// Before (PlayerEventSubscriber.cs line 76)
player.OnStatusChanged += playerManaReducedEventHandler.Execute;

// After — PlayerManaChangedEventHandler becomes:
using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;

namespace NeoServer.Server.Events.Player;

public class PlayerManaChangedEventHandler : IApplicationEventHandler<PlayerStatusChangedEvent>
{
    public void Handle(PlayerStatusChangedEvent @event)
    {
        // existing Execute logic
    }
}
```

**3. Replace invocation in Player.cs:**

```csharp
// Before
OnStatusChanged?.Invoke(this);

// After
EventAggregator.Invoke(new PlayerStatusChangedEvent(this));
```

**4. Clean up:**

- Remove `public event ReduceMana OnStatusChanged;` from `Player.cs` and `IPlayer.cs`
- Remove the `Subscribe`/`Unsubscribe` lines from `PlayerEventSubscriber.cs`
- Remove the `ReduceMana` delegate type (if no other uses)

---

## Testing

After migration, run `dotnet build` to verify compilation. Only run the full test suite if the build fails or if you suspect a behavioral regression. The event migration is a structural change — if it compiles, it works.

## Common Pitfalls

- **Double-firing**: Ensure `EventAggregator.Invoke` replaces the old invocation entirely — don't have both
- **Missing data**: The record must carry all data the handlers previously received from the delegate parameters
- **Subscriber files**: Always check both `Subscribe` and `Unsubscribe` (and the `Unsubscribe` section often has typos like `+=` instead of `-=`)
- **Multiple event declarations**: Some events are declared in both `Player.cs` and `IPlayer.cs` — remove from both
- **Base class events**: Events from `Creature`, `WalkableCreature`, `CombatActor` may need migration too — check `CreatureEventSubscriber.cs`
