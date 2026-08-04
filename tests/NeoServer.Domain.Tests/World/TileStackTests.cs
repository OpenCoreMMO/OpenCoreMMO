using Moq;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.World.Structures;

namespace NeoServer.Domain.Tests.World;

public class TileStackTests
{
    private static Mock<IThing> CreateMockThing(string name = "item")
    {
        var mock = new Mock<IThing>();
        mock.Setup(x => x.Name).Returns(name);
        return mock;
    }

    [Fact]
    public void Insert_WhenBeforeItemExists_InsertsAtCorrectPosition()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var item1 = CreateMockThing("item1").Object;
        var item2 = CreateMockThing("item2").Object;
        var item3 = CreateMockThing("item3").Object;
        var newItem = CreateMockThing("newItem").Object;

        stack.Push(item1);
        stack.Push(item2);
        stack.Push(item3);

        // Act - Insert before item2 (which is at index 1)
        stack.Insert(newItem, item2);

        // Assert
        stack.Count.Should().Be(4);
        var items = stack.Values.ToList();
        items[0].Should().Be(item1);
        items[1].Should().Be(newItem); // newItem should be inserted before item2
        items[2].Should().Be(item2);
        items[3].Should().Be(item3);
    }

    [Fact]
    public void Insert_WhenBeforeItemIsFirst_InsertsAtBeginning()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var item1 = CreateMockThing("item1").Object;
        var item2 = CreateMockThing("item2").Object;
        var newItem = CreateMockThing("newItem").Object;

        stack.Push(item1);
        stack.Push(item2);

        // Act - Insert before the first item
        stack.Insert(newItem, item1);

        // Assert
        stack.Count.Should().Be(3);
        var items = stack.Values.ToList();
        items[0].Should().Be(newItem); // newItem should be at the beginning
        items[1].Should().Be(item1);
        items[2].Should().Be(item2);
    }

    [Fact]
    public void Insert_WhenBeforeItemIsLast_InsertsBeforeLast()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var item1 = CreateMockThing("item1").Object;
        var item2 = CreateMockThing("item2").Object;
        var newItem = CreateMockThing("newItem").Object;

        stack.Push(item1);
        stack.Push(item2);

        // Act - Insert before the last item
        stack.Insert(newItem, item2);

        // Assert
        stack.Count.Should().Be(3);
        var items = stack.Values.ToList();
        items[0].Should().Be(item1);
        items[1].Should().Be(newItem); // newItem should be before item2
        items[2].Should().Be(item2);
    }

    [Fact]
    public void Insert_WhenBeforeItemNotFound_DoesNotInsert()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var item1 = CreateMockThing("item1").Object;
        var item2 = CreateMockThing("item2").Object;
        var newItem = CreateMockThing("newItem").Object;
        var nonExistentItem = CreateMockThing("nonExistent").Object;

        stack.Push(item1);
        stack.Push(item2);

        // Act - Try to insert before an item that doesn't exist
        stack.Insert(newItem, nonExistentItem);

        // Assert
        stack.Count.Should().Be(2); // Count should remain unchanged
        var items = stack.Values.ToList();
        items[0].Should().Be(item1);
        items[1].Should().Be(item2);
        items.Should().NotContain(newItem); // newItem should not be in the stack
    }

    [Fact]
    public void Insert_WhenStackIsEmpty_DoesNotInsert()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var newItem = CreateMockThing("newItem").Object;
        var beforeItem = CreateMockThing("beforeItem").Object;

        // Act - Try to insert into an empty stack
        stack.Insert(newItem, beforeItem);

        // Assert
        stack.Count.Should().Be(0);
        stack.Values.Should().BeEmpty();
    }

    [Fact]
    public void Insert_WithSingleItemInStack_InsertsCorrectlyIfBeforeItemMatches()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var existingItem = CreateMockThing("existing").Object;
        var newItem = CreateMockThing("newItem").Object;

        stack.Push(existingItem);

        // Act - Insert before the only item in the stack
        stack.Insert(newItem, existingItem);

        // Assert
        stack.Count.Should().Be(2);
        var items = stack.Values.ToList();
        items[0].Should().Be(newItem);
        items[1].Should().Be(existingItem);
    }

    [Fact]
    public void Insert_WithMultipleIdenticalItems_InsertsBeforeFirstOccurrence()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var item1 = CreateMockThing("item1").Object;
        var duplicateItem = CreateMockThing("duplicate").Object;
        var item3 = CreateMockThing("item3").Object;
        var newItem = CreateMockThing("newItem").Object;

        stack.Push(item1);
        stack.Push(duplicateItem);
        stack.Push(duplicateItem); // Add the same reference twice
        stack.Push(item3);

        // Act - Insert before the duplicate item
        stack.Insert(newItem, duplicateItem);

        // Assert
        stack.Count.Should().Be(5);
        var items = stack.Values.ToList();
        items[0].Should().Be(item1);
        items[1].Should().Be(newItem); // Should be inserted before first occurrence
        items[2].Should().Be(duplicateItem);
        items[3].Should().Be(duplicateItem);
        items[4].Should().Be(item3);
    }

    [Fact]
    public void Insert_PreservesStackOrderForOtherItems()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var items = new List<IThing>();
        for (var i = 0; i < 5; i++)
        {
            var item = CreateMockThing($"item{i}").Object;
            items.Add(item);
            stack.Push(item);
        }

        var newItem = CreateMockThing("newItem").Object;

        // Act - Insert in the middle
        stack.Insert(newItem, items[2]);

        // Assert
        stack.Count.Should().Be(6);
        var stackItems = stack.Values.ToList();
        stackItems[0].Should().Be(items[0]);
        stackItems[1].Should().Be(items[1]);
        stackItems[2].Should().Be(newItem);
        stackItems[3].Should().Be(items[2]);
        stackItems[4].Should().Be(items[3]);
        stackItems[5].Should().Be(items[4]);
    }

    [Fact]
    public void Insert_WithNullBeforeItem_DoesNotThrow()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var item = CreateMockThing().Object;
        var newItem = CreateMockThing("newItem").Object;

        stack.Push(item);

        // Act & Assert - Should handle null gracefully
        var act = () => stack.Insert(newItem, null);
        act.Should().NotThrow();

        // Item should not be inserted when beforeItem is null (IndexOf returns -1)
        stack.Count.Should().Be(1);
    }

    [Fact]
    public void Insert_EnumerationStillWorksInReverseOrder()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var item1 = CreateMockThing("item1").Object;
        var item2 = CreateMockThing("item2").Object;
        var item3 = CreateMockThing("item3").Object;
        var newItem = CreateMockThing("newItem").Object;

        stack.Push(item1);
        stack.Push(item2);
        stack.Push(item3);
        stack.Insert(newItem, item2);

        // Act - Enumerate (should be in reverse order)
        var enumeratedItems = stack.ToList();

        // Assert - Enumeration is reversed, so last item comes first
        enumeratedItems.Count.Should().Be(4);
        enumeratedItems[0].Should().Be(item3);
        enumeratedItems[1].Should().Be(item2);
        enumeratedItems[2].Should().Be(newItem);
        enumeratedItems[3].Should().Be(item1);
    }

    [Fact]
    public void Insert_AfterMultipleInsertions_MaintainsCorrectOrder()
    {
        // Arrange
        var stack = new TileStack<IThing>();
        var item1 = CreateMockThing("item1").Object;
        var item2 = CreateMockThing("item2").Object;
        var item3 = CreateMockThing("item3").Object;

        stack.Push(item1);
        stack.Push(item3);

        // Act - Insert item2 between item1 and item3
        stack.Insert(item2, item3);

        // Assert
        var items = stack.Values.ToList();
        items[0].Should().Be(item1);
        items[1].Should().Be(item2);
        items[2].Should().Be(item3);

        // Act - Insert another item
        var item4 = CreateMockThing("item4").Object;
        stack.Insert(item4, item2);

        // Assert
        items = stack.Values.ToList();
        items[0].Should().Be(item1);
        items[1].Should().Be(item4);
        items[2].Should().Be(item2);
        items[3].Should().Be(item3);
    }
}