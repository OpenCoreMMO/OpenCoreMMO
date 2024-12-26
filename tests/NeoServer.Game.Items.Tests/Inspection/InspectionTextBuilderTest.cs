using System.Linq;
using FluentAssertions;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Creatures.Vocation;
using NeoServer.Game.Items.Inspection;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Items.Tests.Inspection;

public class InspectionTextBuilderTest
{
    [Theory]
    [InlineData("You see  item.")]
    [InlineData("You see  item.\nIt can only be wielded properly by knights and paladins.", 1, 2)]
    [InlineData("You see  item.\nIt can only be wielded properly by knights.", 1)]
    [InlineData("You see  item.\nIt can only be wielded properly by knights, paladins and sorcerers.", 1, 2, 3)]
    [InlineData("You see  item.\nIt can only be wielded properly by knights, paladins, sorcerers and druids.", 1, 2, 3,
        4)]
    public void Add_HasVocations_ReturnText(string expected, params int[] vocations)
    {
        var vocationStore = new VocationStore();
        vocationStore.Add(1, new Vocation { Name = "Knight" });
        vocationStore.Add(2, new Vocation { Name = "Paladin" });
        vocationStore.Add(3, new Vocation { Name = "Sorcerer" });
        vocationStore.Add(4, new Vocation { Name = "Druid" });

        var input = vocations.Select(x => (byte)x).ToArray();

        var item = ItemTestData.CreateDefenseEquipmentItem(1);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.Vocation, input);

        var player = PlayerTestDataBuilder.Build(hp: 200);

        //act
        var actual = InspectionTextBuilder.Build(item);

        //assert
        actual.Should().Be(expected);
    }
}