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

Use the PR template with:
- **Description** — Short summary of changes
- **Key Changes** — Bullet points of important changes
- **Types of Changes** — Bug fix, new feature, breaking change, documentation, refactoring, merge down
- **Test Case** — How the change was tested

## Testing

### Framework & Libraries

- **xUnit** — Primary testing framework
- **FluentAssertions** — Expressive assertion library
- No mocking framework for business logic; mocks only for repositories/database access

### Test Projects

```
tests/
├── NeoServer.Domain.Tests        # Domain logic tests (largest)
├── NeoServer.Game.Chats.Tests    # Chat system tests
├── NeoServer.Game.Creatures.Tests # Creature behavior tests
├── NeoServer.Game.Items.Tests    # Item system tests
├── NeoServer.Game.Model.Tests    # Game model tests
├── NeoServer.Game.Systems.Tests  # Game systems tests
├── NeoServer.Game.Tests          # General game tests
├── NeoServer.Game.World.Tests    # World/map tests
├── NeoServer.Loaders.Tests       # Data loader tests
├── NeoServer.Networking.Tests    # Network protocol tests
├── NeoServer.Server.Tests        # Server logic tests
└── NeoServer.WebApi.Tests        # API tests
```

### Test Conventions

- **Naming**: `Actor_does_something_when_something_happens` (e.g., `Player_gets_disconnected_when_game_is_stopped`)
- **Structure**: Arrange-Act-Assert (AAA)
- **Isolation**: Do NOT share objects between tests. Each test creates its own instances.
- **Mocking**: Only mock repositories and database access layers. Use real implementations for all other classes.
- **Helpers**: Use static helper methods for building test instances with optional parameters
- **One behavior per test**: Each test verifies a single behavior

Example:

```csharp
[Fact]
public void Player_gets_disconnected_when_game_is_stopped()
{
    // Arrange
    var game = CreateGameServer(state: GameState.Stopped);
    var handler = CreateHandler(game: game);
    var connection = CreateConnection();
    var packet = CreateValidLoginPacket();

    // Act
    handler.HandleMessage(packet, connection);

    // Assert
    connection.ReceivedDisconnectPacket.Should().BeTrue();
    connection.IsClosed.Should().BeTrue();
}
```

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
