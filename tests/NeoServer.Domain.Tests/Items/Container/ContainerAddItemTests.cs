using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Items.Container;

public class ContainerAddItemTests
{
  [Fact]
  public void Add_item_to_invalid_slot_container_throws_exception()
  {
      //arrange
      var sut = ItemTestData.CreateContainer(capacity: 2);
      var item = ItemTestData.CreateWeaponItem(id: 1);
      
      //act
      var act = () => sut.AddItem(item, 3);

      //assert
      act.Should().Throw<ArgumentOutOfRangeException>();
  }   
}