---
name: unit-testing
description: >
  Write, fix, or review NeoServer unit tests using xUnit and
  FluentAssertions. Use this skill when the user wants to create
  new tests, fix failing tests, or review existing tests — even if
  they only say "add tests," "fix the test," or mention testing a
  domain entity (Player, Monster, Item, NPC, Tile), handler, game
  system, or server component. Covers test helpers, behavioral
  naming, xUnit traits, and the repository-only mocking policy.
compatibility: Requires .NET 10 SDK and dotnet CLI. Designed for NeoServer project conventions.
---

## Quick Reference

### Workflow

- [ ] 1. **Identify behavior** — one behavior per test
- [ ] 2. **Write the test** using AAA structure with the template below
- [ ] 3. **Use existing helpers** (`PlayerTestDataBuilder`, `ItemTestDataBuilder`, `MapTestDataBuilder`, etc.) instead of building from scratch
- [ ] 4. **Tag with trait**: `[Trait("Category", "<value>")]`
- [ ] 5. **Run**: `dotnet test tests/<project>`
- [ ] 6. **Fix** any failures and re-run until green

### Test Template

```csharp
[Fact]
[Trait("Category", "<HappyPath|Validation|EdgeCase|ErrorCondition|Integration>")]
public void {Actor}_does_{something}_when_{condition}()
{
    // Arrange
    var player = PlayerTestDataBuilder.Build();
    // ... use existing builders; mock only I*Repository

    // Act
    player.DoSomething();

    // Assert
    result.Should().Be(expected);
}
```

## Project-Specific Conventions

### Test Framework & Mocking

- **xUnit** with **FluentAssertions** (`Should().BeTrue()`, `.BeEquivalentTo()`, etc.)
- **Do NOT mock** domain entities, services, or commands — use real implementations
- **Only mock** infrastructure interfaces: `I*Repository`, `IConnection`, `IEventAggregator`
- Use **Moq** for mocks when needed

### Naming

```
{Actor}_does_{something}_when_{condition}
```

Examples: `Player_gets_disconnected_when_game_is_stopped`, `World_replaces_tile_when_new_tile_provided`, `Player_cannot_push_null_creature`.

### Test Isolation

Each `[Fact]` creates its own instances. Never share objects between tests — no static state, no class-level fixtures. Tests that need serialized execution use `[ThreadBlocking]`.

### Traits

Only `[Trait("Category", "<value>")]` is used. Valid values:

| Value | When to Use |
|---|---|
| `HappyPath` | Core positive/expected behavior |
| `Validation` | Input validation, null checks, rejection |
| `EdgeCase` | Boundary conditions, corner cases |
| `ErrorCondition` | Error state handling (login fails, etc.) |
| `Integration` | Multi-component interaction (real objects, mock only repos) |
| `PathFinding` | Pathfinding algorithm tests |
| `Proximity` | Distance/proximity checks |
| `Tile` | Tile occupation/state tests |

### Custom Attributes

- `[SkipOnGitHubActionsFact]` — skips test in CI (for tests needing real DB)
- `[ThreadBlocking]` — serializes test execution via semaphore

## Helper Methods — Reusable Builders

All helpers live in `tests/NeoServer.Domain.Tests/Helpers/` and use optional parameters with sensible defaults:

### Core Builders

| Builder | Location | Key Method |
|---|---|---|
| `PlayerTestDataBuilder` | `Helpers/Player/` | `Build(id, name, hp, mana, skills, inventoryMap, vocationType, ...)` |
| `ItemTestDataBuilder` | `Helpers/` | `CreateContainer(...)`, `CreateCumulativeItem(...)`, `CreateWeaponItem(...)` (27 methods) |
| `MonsterTestDataBuilder` | `Helpers/` | `Build(maxHealth, speed, isHostile, flags, ...)` |
| `MapTestDataBuilder` | `Helpers/Map/` | `Build(fromX, toX, fromY, toY, fromZ, toZ, ...)` — rectangular grid |
| `NpcTestDataBuilder` | `Helpers/` | `Build(name, npcType)` |
| `InventoryTestDataBuilder` | `Helpers/Player/` | `Build(player, inventoryMap, ...)` |
| `PartyTestDataBuilder` | `Helpers/` | `Build(partyInviteService, players)` |

### Server Test Builders

Located in `tests/NeoServer.Domain.Tests/Server/`:

| Builder | Purpose |
|---|---|
| `GameServerTestBuilder` | Creates `IGameServer` with map |
| `ItemTypeStoreTestBuilder` | Creates `ItemTypeStore` from items |
| `ItemFactoryTestBuilder` | Creates `IItemFactory` with type store |
| `DecayableItemManagerTestBuilder` | Creates `DecayableItemManager` |
| `ScriptManagerTestBuilder` | Creates `IScriptManager` |
| `ServerTestHelper` | Starts game server threads |
| `TestSetup` | Full DI container boot (in `NeoServer.Server.Tests`) |

### Event Helpers

| Helper | Purpose |
|---|---|
| `EventAggregatorTestHelper` | `SetupEventAggregator<TEvent>(handler)` — wires handler to fire on publish |
| `EventAssertHelper` | Fluent extensions: `WithArgs<T>(Expression)` |
| `EventSubscriptionCleanUp` | `CleanUp<T>(eventName)` — tears down subscriptions between tests |

### Pattern

```csharp
// Simple in-file helper
private static Location CreateLocation(ushort x = 100, ushort y = 100, byte z = 7)
    => new(x, y, z);

// Compose from existing builders
private static IPlayer CreatePlayerWithFullInventory()
{
    var player = PlayerTestDataBuilder.Build();
    var inventory = InventoryTestDataBuilder.Build(player);
    return player;
}
```

## Gotchas

- **Mock only infrastructure**: Never mock domain entities (`IPlayer`, `IMonster`, `IItem`), services, or commands — use real implementations. Only mock `I*Repository`, `IConnection`, `IEventAggregator`.
- **No shared state**: Each `[Fact]` must create everything from scratch. Static or class-level state causes test pollution.
- **One behavior per test**: Multiple assertions for different behaviors belong in separate `[Fact]` methods.
- **Always use FluentAssertions**: `result.Should().BeTrue()`, not `Assert.True(result)`.
- **Handler null guards**: Event handlers must guard with `if (@event is null) return;`.
- **Builders are in Helpers/**: Look in `tests/NeoServer.Domain.Tests/Helpers/` for `*TestDataBuilder` classes before writing new setup code.
- **Data-driven tests**: Use `[InlineData]` for simple cases, `[MemberData]` for complex parameter sets, `[ClassData]` for reusable data classes.

## Running Tests

```sh
# All tests
dotnet test tests/

# Specific project
dotnet test tests/NeoServer.Domain.Tests

# Specific category
dotnet test tests/ --filter "Category=HappyPath"

# Code coverage
dotnet test tests/ --collect:"XPlat Code Coverage" --results-directory testresults

# Build before test
dotnet build src/Standalone --configuration Release
```
