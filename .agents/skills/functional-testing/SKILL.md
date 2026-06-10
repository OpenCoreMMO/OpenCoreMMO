---
name: functional-testing
description: >
  Write, fix, or review NeoServer functional tests that start from the
  network handler and run against the real server stack. Use this skill when
  the user wants end-to-end style coverage without mocks, including login,
  packet handling, persistence, routines, and other server flows exercised
  through real dependency injection and the test container.
compatibility: Requires .NET 10 SDK and dotnet CLI. Designed for NeoServer project conventions.
---

## Scope

Functional tests in this repo:

- Start from the network handler or top-level command path
- Use the real DI container and real infrastructure where practical
- Do not mock domain entities, services, commands, or the game server stack
- Prefer real packets, real handlers, real routines, and real database access
- Assert against game state, packet output, and persisted data

## Core Rule

If the test can be expressed through the real server path, do that.

- Use the real `TestSetup.Setup()` container when the flow needs a full boot
- Use the real `PlayerLogInCommand` / handlers / routines
- Avoid Moq unless you are testing an infrastructure boundary that cannot be real

## Reference Pattern

Use `tests/NeoServer.Server.Tests/Login/PlayerLoginTests.cs` as the canonical example.

It shows:

- Full container boot via `TestSetup.Setup()`
- Real `NeoContext`, `IGameServer`, and commands from DI
- `SkipOnGitHubActionsFactAttribute` for tests that need the full environment
- Assertions over packet sends, map placement, database state, and runtime behavior

### Key setup pattern

```csharp
_container = TestSetup.Setup().Result;
_command = _container.GetService<PlayerLogInCommand>();
_game = _container.GetService<IGameServer>();
_context = _container.GetService<NeoContext>();
```

## Workflow

- [ ] 1. Identify the entry point: network handler, command, or routine
- [ ] 2. Decide whether the test needs the full container or a narrower integration harness
- [ ] 3. Use real packets and real server objects wherever possible
- [ ] 4. Keep assertions on visible outcomes: packets, database rows, map placement, game state
- [ ] 5. Add `SkipOnGitHubActionsFact` if the test needs full boot or is environment-sensitive
- [ ] 6. Run `dotnet test tests/NeoServer.Server.Tests`

## Test Style

Use AAA and behavioral names.

```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task Player_gets_placed_on_map_when_login_succeeds()
{
    // Arrange
    _game.Open();

    var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var randomNumber = (byte)123;
    var connection = CreateMockConnection(timestamp, randomNumber);
    var request = CreatePlayerLogInRequest(timestamp, randomNumber);

    // Act
    var (success, message) = await _command.Execute(request, connection.Object);

    // Assert
    success.Should().BeTrue();
    message.Should().BeNull();
}
```

## Test Data

- Prefer existing builders and fixtures from `tests/NeoServer.Domain.Tests/Helpers/`
- Prefer `TestSetup` for full server state
- Use direct packet instances when a handler expects them
- Use real entities from the database seed when validating persistence

## Mocking Policy

Do not mock:

- Domain entities (`IPlayer`, `IMonster`, `IItem`)
- Services
- Commands
- Game server internals

Only mock when the boundary is external or infrastructure-level and the real dependency is not practical to use:

- `IConnection`
- `IEventAggregator` if you need to intercept publish behavior
- Repository interfaces when the test intentionally isolates persistence

## Assertions

Prefer assertions that verify the end result of the flow:

- player loaded and placed on map
- connection receives or does not receive specific packets
- database fields updated
- runtime state changed as expected
- no duplicate player instances after reconnect

Avoid asserting internal implementation details unless the test is specifically about that behavior.

## Common Scenarios

- Login success and rejection paths
- Packet-to-command execution
- Condition application and expiry through real handlers/routines
- Persistence updates after gameplay actions
- Reconnect behavior
- World placement and creature visibility

## When to Use Unit Tests Instead

If the behavior is purely internal logic and does not need the server boot or packet path, prefer unit tests under `NeoServer.Domain.Tests` or the relevant test project.
