# AGENTS.md

## Project Overview

OpenCoreMMO is a modern, open-source MMORPG server emulator written in C# targeting **.NET 10**. It emulates the Tibia 8.60 game protocol and is designed to work with OTClient, OTCv8, and OTCR clients. The project began in January 2020 and uses Domain-Driven Design (DDD) principles with a layered architecture.

## Setup & Build Commands

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- (Optional) Docker & Docker Compose for infrastructure services
- (Optional) PostgreSQL for production database

### Build & Run

```sh
# Clone the repository
git clone https://github.com/OpenCoreMMO/OpenCoreMMO.git

# Restore and build (from repo root)
dotnet restore src/Standalone
dotnet build src/Standalone --configuration Release

# Run the server (from repo root)
dotnet run --project src/Standalone
```

### Run Tests

```sh
# Run all tests
dotnet test tests/

# Run a specific test project
dotnet test tests/NeoServer.Domain.Tests

# Run with code coverage
dotnet test tests/ --collect:"XPlat Code Coverage" --results-directory testresults
```

### Docker

```sh
# Start infrastructure (PostgreSQL, Graylog, etc.)
docker compose -f compose.infrastructure.yml up -d

# Start the game server in Docker
docker compose -f compose.server.yml up -d
```

## Solution Architecture

The solution is organized in a layered architecture under `src/`:

```
src/
├── Core/                        # Domain Layer (pure business logic)
│   └── NeoServer.Domain         # Entities, value objects, domain services
│
├── ApplicationServer/                        # Application & Server Layer
│   ├── NeoServer.Application                 # Application services (currently empty)
│   ├── NeoServer.Application.EventHandlers   # Event handler implementations
│   ├── NeoServer.Server                      # Core server runtime
│   ├── NeoServer.Server.Commands             # Command pattern implementations
│   ├── NeoServer.Server.Contracts            # Server-level interfaces
│   ├── NeoServer.Server.Events               # Server event handlers
│   ├── NeoServer.Server.Helpers              # Utility extensions
│   ├── NeoServer.Server.Jobs                 # Scheduled jobs
│   ├── NeoServer.Server.Routines             # Recurring game routines
│   ├── NeoServer.Server.Security             # RSA / authentication
│   └── NeoServer.Server.Compiler             # Runtime C# compilation
│
├── Database/                    # Data Access Layer
│   ├── NeoServer.Data                        # EF Core contexts, repositories, entities
│   ├── NeoServer.Data.InMemory               # In-memory database provider
│   ├── NeoServer.Data.InMemory.DataStores    # In-memory data stores (static data caches)
│   ├── NeoServer.Infra.Data                  # Data infrastructure
│   └── NeoServer.Infra.Data.InMemory         # In-memory infrastructure
│
├── NetworkingServer/            # Networking Layer
│   ├── NeoServer.Networking                  # TCP listeners, connection management
│   ├── NeoServer.Networking.Handlers         # Packet handler routing
│   ├── NeoServer.Networking.Packets          # Packet serialization/deserialization
│   ├── NeoServer.Infra.Networking            # Networking infrastructure
│   └── NeoServer.Infra.Networking.Packets    # Low-level packet infrastructure
│
├── Extensions/                  # Scripting Extensions
│   ├── NeoServer.Scripts.Lua                 # Lua scripting bridge
│   └── NeoServer.Scripts.LuaJIT             # LuaJIT scripting support
│
├── Loaders/                     # Data Loaders
│   ├── NeoServer.Loaders                     # OTB/OTBM/JSON file loaders
│   └── NeoServer.Application.Loaders        # Application-level loaders
│
├── Shared/                      # Shared Libraries
│   ├── NeoServer.IoC                         # Dependency injection helpers
│   └── NeoServer.Web.Shared                  # Web shared components
│
├── Standalone/                  # Application Entry Point
│   ├── Program.cs                            # Main entry point
│   └── IoC/                                  # DI container configuration
│
├── WebAPI/                      # REST API (ASP.NET)
├── NeoServer.Web.Admin/         # Admin web interface
└── NeoServer.Aspire/            # .NET Aspire orchestration
```

### Game Data Files

The `data/` directory holds game content loaded at runtime:

- `items/` — Item definitions (OTB format)
- `monsters/` — Monster type definitions (JSON)
- `npcs/` — NPC definitions and behaviors
- `spells/` — Spell definitions
- `scripts/` — Lua gameplay scripts (Revscript format)
- `world/` — Map files (OTBM format)
- `vocations.json` — Player vocation definitions
- `groups.json` — Player group/permission definitions
- `quests.json` — Quest definitions
- `channels.json` — Chat channel definitions
- `tiles.json` — Special tile configurations

## Key Design Patterns

### Domain-Driven Design (DDD)

The core domain lives in `NeoServer.Domain` and is isolated from infrastructure concerns. It contains:

- **Entities**: `Player`, `Monster`, `Npc`, `Item`, `ItemType`, `World`, `Map`
- **Value Objects**: `Location`, `LookType`, `Skill`, `Outfit`
- **Domain Services**: `ItemMovementService`, `CreatureDeathService`, `AttackService`, `SpellService`, etc.
- **Repositories** (interfaces): `IPlayerRepository`, `IPlayerMailRepository`
- **Domain Events**: Records implementing `IEvent` (e.g., `TileChangedEvent`, `PlayerWalkedEvent`)

### EventAggregator Pattern

The codebase uses a custom `EventAggregator` for decoupled communication:

```csharp
// Publishing an event
EventAggregator.Invoke(new SomeEvent(data));

// Handling an event (implement the interface)
public class SomeEventHandler : IApplicationEventHandler<SomeEvent>
{
    public void Handle(SomeEvent @event) { ... }
}

// Network handlers get priority over application handlers
public class SomeNetworkHandler : INetworkingEventHandler<SomeEvent>
{
    public void Handle(SomeEvent @event) { ... }
}
```

Event handlers are auto-discovered via assembly scanning during `IEventAggregator.Initialize()`. Network handlers execute before application handlers.

### Factory Pattern

Factories are used extensively for creating game entities:

- `ItemFactory` / `IItemFactory` — Creates items (with sub-factories: `WeaponFactory`, `ContainerFactory`, `RuneFactory`, etc.)
- `CreatureFactory` / `ICreatureFactory` — Creates creatures
- `MonsterFactory` / `IMonsterFactory` — Creates monsters
- `NpcFactory` / `INpcFactory` — Creates NPCs
- `TileFactory` / `ITileFactory` — Creates map tiles
- `ChatChannelFactory` — Creates chat channels

### Dependency Injection

The project uses **Microsoft.Extensions.DependencyInjection** with modular registration in `src/Standalone/IoC/Modules/`:

- `ServiceInjection.cs` — Game services and operations
- `FactoryInjection.cs` — Entity factories
- `EventInjection.cs` — Event handlers and subscribers
- `DatabaseInjection.cs` — Database contexts and connections
- `LoaderInjection.cs` — Data file loaders
- `NetworkInjection.cs` — Networking components
- `ConfigurationInjection.cs` — Configuration bindings
- `LoggerInjection.cs` — Serilog configuration
- `LuaInjection.cs` — Lua scripting engine

All game services are registered as **singletons**.

### In-Memory Data Stores

Static game data (item types, monster types, vocations, etc.) is cached in specialized `DataStore<TKey, TValue>` classes:

- `ItemTypeStore`, `MonsterTypeStore`, `VocationStore`, `GroupStore`, `NpcStore`, `GuildStore`, etc.

These are populated at startup by loaders and remain immutable at runtime.

### Dispatcher & Scheduler

The server uses a three-tier task processing system:

- **Dispatcher** (`IDispatcher`) — Handles immediate game actions on the game thread
- **Scheduler** (`IScheduler`) — Manages time-based events (creature checks, item decay, world light)
- **PersistenceDispatcher** (`IPersistenceDispatcher`) — Handles database writes on a separate thread

## Configuration

Configuration is stored in `src/Standalone/appsettings.json`:

- **`server`** — Server version (860), ports, data paths, save intervals
- **`game`** — Experience rates, loot rates, PvP settings, combat config, death system
- **`client`** — Client-specific feature flags (OTCv8 options)
- **`database`** — Database connections and active provider (`INMEMORY`, `SQLITE`, `POSTGRESQL`)
- **`Log`** — Serilog minimum log level
- **`GrayLog`** — Optional centralized logging

Supported databases: **InMemory** (default for dev), **SQLite**, **PostgreSQL**.

## Code Style & Conventions

### General

- **Language**: C# with latest language version features (file-scoped namespaces, records, pattern matching, extension methods)
- **Target Framework**: .NET 10
- The solution uses `Directory.Build.props` for centralized package version management
- Prefer **interface-based design** — most services have a corresponding `I*` interface
- Domain logic must not depend on infrastructure (networking, database, IoC)
- Use `record` types for event definitions (DTOs implementing `IEvent`)
- Prefer **composition over inheritance** for entity behavior
- **Avoid variable abbreviations** — use descriptive names like `player` instead of `p`, `container` instead of `cont`, `experience` instead of `exp`. Abbreviations hurt readability and make code harder to search.
  - Acceptable exceptions: common loop variables (`i`, `j`, `k`), widely known acronyms (`Rsa`, `Http`, `Json`, `Xml`), and lambda parameters in trivial expressions (`x => x.Name`)
  - Do NOT abbreviate domain concepts (`creature`, `item`, `tile`, `player`)
- **Avoid `else`** — prefer early returns and guard clauses to reduce nesting and improve readability. An `else` often indicates the happy path isn't clearly separated from edge cases.
  - Discouraged: `if (condition) { ... } else { ... }`
  - Preferred: `if (condition) return ...;` / `if (!condition) return ...;` then proceed with the main flow
- **Use braces on all control flow statements** (`if`, `for`, `foreach`, `while`, `do`) — except when the body is a single `return;`. Always use `{ }` for any other single-statement body to prevent bugs when adding lines later.
  - Allowed: `if (condition) return;`
  - Required braces: `if (condition) DoSomething();` ❌ → `if (condition) { DoSomething(); }` ✅
- **Use primary constructors when possible** — prefer the concise `class Service(IType dep)` syntax over explicit field backing for simple dependency injection and immutable state.
- **Use the latest C# language features** — the project targets .NET 10, so use the latest available features (file-scoped namespaces, collection expressions, `List<string>`, primary constructors, raw string literals, etc.) unless there is a specific compatibility or readability reason not to.

### Naming

- **Namespaces**: Mirror the folder structure. Root namespace is `NeoServer.*`
- **Interfaces**: Prefix with `I` (e.g., `IItemFactory`, `ICreatureFactory`)
- **Event records**: Suffix with `Event` (e.g., `PlayerWalkedEvent`, `TileChangedEvent`)
- **Event handlers**: Suffix with `EventHandler` (e.g., `PlayerWalkedEventHandler`)
- **Services**: Suffix with `Service` (e.g., `ItemMovementService`, `DecayService`)
- **Factories**: Suffix with `Factory` (e.g., `ItemFactory`, `MonsterFactory`)
- **Data stores**: Suffix with `Store` (e.g., `ItemTypeStore`, `VocationStore`)
- **Loaders**: Suffix with `Loader` (e.g., `ItemTypeLoader`, `MonsterLoader`)
- **Routines**: Suffix with `Routine` (e.g., `GameCreatureRoutine`, `PlayerPersistenceRoutine`)

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

Types: `fix`, `feat`, `build`, `chore`, `ci`, `docs`, `style`, `refactor`, `perf`, `test`.

### Pull Requests

Use the **Pull Request Skill** (`.agents/skills/pull-request/SKILL.md`) for all PRs. Load it via `skill pull-request` before creating or reviewing a pull request. The skill covers:

- **Branch naming** — `feat/`, `fix/`, `refactor/` prefixed branches from `develop`
- **PR title** — domain/feature-oriented, not implementation-oriented
- **PR template** — Description, Key Changes, Types of Changes, Test Case
- **PR creation** — `github_create_pull_request` with the required parameters

## Performance Guidelines

### Philosophy

The server is a high-throughput, low-latency application. Write allocation-conscious code by default. Readability matters, but not at the cost of predictable GC pressure or CPU overhead in hot paths.

### Prefer Explicit Loops Over LINQ

Use `for` / `foreach` instead of LINQ methods in performance-sensitive code. LINQ allocates iterators, closures, and intermediate collections.

**Discouraged:**
```csharp
var items = tiles.Where(t => t.AllItems is not null).SelectMany(t => t.AllItems).Where(i => i.IsPickupable).ToList();
```

**Preferred:**
```csharp
var items = new List<IItem>();
foreach (var tile in tiles)
{
    if (tile.AllItems is null) continue;
    foreach (var item in tile.AllItems)
    {
        if (item is not null && item.IsPickupable)
            items.Add(item);
    }
}
```

### Avoid Specific LINQ Methods Without Justification

`Where`, `Select`, `Any`, `Count`, `FirstOrDefault`, `SingleOrDefault`, `ToList`, `ToArray` — none of these should appear in hot paths unless a comment explains why the allocation is acceptable.

### Avoid Lambda Expressions in Hot Paths

Lambdas (including those passed to LINQ methods, event handlers, and async continuations) capture variables and allocate closures on the heap. In tight loops or frequently-called code, extract the logic into a local method or a static method instead.

### Avoid `yield return` and Iterator Methods

Iterator methods (those using `yield return`) generate compiler-backed state machine objects. Every call allocates a new enumerator. Return a concrete collection (`List<T>`, array) directly instead.

**Discouraged:**
```csharp
public IEnumerable<IItem> PickupableItems
{
    get
    {
        foreach (var tile in _tiles)
            if (tile.AllItems is not null)
                foreach (var item in tile.AllItems)
                    if (item.IsPickupable)
                        yield return item;
    }
}
```

**Preferred:**
```csharp
public List<IItem> PickupableItems
{
    get
    {
        var items = new List<IItem>();
        foreach (var tile in _tiles)
        {
            if (tile.AllItems is null) continue;
            foreach (var item in tile.AllItems)
            {
                if (item is not null && item.IsPickupable)
                    items.Add(item);
            }
        }
        return items;
    }
}
```

### Pre-Allocate Collections When Possible

When the size is known or bounded, pre-allocate to avoid growth-induced reallocations:

```csharp
var items = new List<IItem>(capacity: 16);
```

### Minimize Allocations

- Avoid creating intermediate collections (e.g., `.ToList()` just to iterate).
- Avoid boxing — prefer generic collections and avoid `object` parameters.
- Use `ArrayPool<T>` for large temporary buffers.
- Pool frequently-allocated objects where appropriate.

### Benchmark New Abstractions

Any abstraction that may impact performance (new interface dispatch layer, reflection-based registration, dynamic code paths) must be validated with benchmarks before being merged into `develop`.

---

## Testing

Use the **Unit Testing Skill** (`.agents/skills/unit-testing/SKILL.md`) for all testing work. Load it via `skill unit-testing` before writing, fixing, or reviewing tests. The skill covers:

- **Framework & mocking** — xUnit, FluentAssertions, Moq (repositories only)
- **Test conventions** — naming, AAA structure, isolation, one behavior per test
- **Test builders** — `PlayerTestDataBuilder`, `ItemTestDataBuilder`, `MapTestDataBuilder`, and others
- **Traits** — `Category` values: `HappyPath`, `Validation`, `EdgeCase`, `ErrorCondition`, `Integration`, `PathFinding`, `Proximity`, `Tile`
- **Custom attributes** — `[SkipOnGitHubActionsFact]`, `[ThreadBlocking]`
- **Running tests** — `dotnet test tests/`, per-project, by category

## CI/CD Pipeline

CI runs on **GitHub Actions** with the workflow in `.github/workflows/opencoremmo-validation.yaml`:

1. **Build & Test** — Runs on `ubuntu-latest`, `windows-latest`, `macos-15-intel`
   - `dotnet restore`
   - `dotnet build --configuration Release`
   - `dotnet test` with code coverage (retries up to 3 times)
2. **Code Coverage** — Uploaded to Codecov (Windows only)
3. **CodeQL Analysis** — Security scanning after build

Triggered on: push to `develop`, PRs targeting `develop`, or manual dispatch.

## Lua Scripting

The server supports **Revscript-style Lua scripting** for gameplay customization:

- Scripts live in `data/scripts/` and are hot-reloadable
- Lua bindings are provided via `NeoServer.Scripts.LuaJIT`
- Available APIs: Action, Creature, CreatureEvent, GlobalEvent, Item, Monster, NPC, Player, Spell, TalkAction, MoveEvent, and more
- See `data/README.md` for the full scripting API reference

## Security Considerations

- RSA encryption is used for login packet decryption (`Rsa.LoadPem()`)
- The RSA key is stored at `data/key.pem`
- Never commit production RSA keys to the repository
- Database credentials should be managed via environment variables (see `.env.example`)
- For Docker deployments, configure secrets via compose files and environment variables
