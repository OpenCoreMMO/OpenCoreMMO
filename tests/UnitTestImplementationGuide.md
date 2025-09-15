# Unit Test Implementation Guide

This guide provides technical guidelines for implementing unit tests for the NeoServer application.

## Testing Framework and Libraries
- **xUnit**: Primary testing framework for writing and running tests.
- **FluentAssertions**: For expressive and readable assertions.

## Mocking Strategy
- **Do not use mocks** for business logic classes, domain objects, or services.
- **Use mocks only** for repositories or database access layers (e.g., IAccountRepository, IIpBansRepository).
- Test real implementations where possible to ensure integration and behavior accuracy.

## Test Isolation
- **Do not share objects** between unit tests, including within the same test class. Avoid global state or shared instances.
- Each test method should create its own instances and use fresh setups to avoid state pollution.

## Naming Conventions
- **Behavioral Style**: Use names like `Actor_does_something_when_something_happens`.
- **Actor-Action Oriented**: Focus on what the actor (domain entity like Player, Monster, Item, NPC, Tile) does in response to conditions.
- Examples:
  - `Player_gets_disconnected_when_game_is_stopped`
  - `Player_gets_disconnect_packet_when_account_name_is_empty`
  - `Player_gets_loaded_and_placed_on_map_when_login_succeeds`

## Test Structure and Helpers
- **Helper Methods**: Use static helper methods to build instances consistently.
- Example helper for creating a valid PlayerLogInPacket:
  ```csharp
  private static PlayerLogInPacket CreateValidLoginPacket(string account = "test", string password = "pass", string character = "char")
  {
      // Implementation to build packet with valid data
  }
  ```
- Example helper for setting up handler dependencies:
  ```csharp
  private static PlayerLogInHandler CreateHandler(IGameServer game = null, IAccountRepository repo = null)
  {
      // Implementation with defaults or mocks only for repos
  }
  ```

## Expanding Helper Methods
- **Modular Helpers**: Create separate helper methods for different components (e.g., `CreateGameServer`, `CreateConnection`, `CreatePlayerRecord`).
- **Parameterization**: Use optional parameters to allow customization while providing sensible defaults.
- **Composition**: Combine simpler helpers to build complex setups (e.g., `CreateHandler` can call `CreateGameServer` and `CreateRepository`).
- **Consistency**: Ensure all helpers produce valid, consistent instances to avoid test flakiness.
- **Expansion**: As tests grow, add new helpers for recurring setups rather than duplicating code in test methods.

## Test Categories
- **Happy Path Tests**: Verify successful flows.
- **Validation Tests**: Test input validation and error responses.
- **Integration Tests**: Test interactions between components (with real objects, mock only repos).
- **Edge Case Tests**: Test boundary conditions and unusual inputs.
- **Failure Mode Tests**: Test error handling and recovery.

## Test Organization
- **xUnit Traits**: Use `[Trait("Category", "HappyPath")]` or similar attributes to categorize tests for selective running and reporting.
- **Other Mechanisms**: Consider using test collections, custom attributes, or folder structures to group related tests.
- **Benefits**: Enables running specific test categories (e.g., only validation tests) and improves maintainability.

## Example Test Structure
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

## Best Practices
- **Arrange-Act-Assert (AAA)**: Structure each test clearly.
- **Descriptive Assertions**: Use FluentAssertions for detailed failure messages.
- **Test One Thing**: Each test should verify a single behavior.
- **Avoid Over-Mocking**: Prefer real objects to catch integration issues.
- **Helper Consistency**: Ensure helpers produce valid, predictable instances.