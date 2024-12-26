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
    [InlineData("You see  item.\nIt can only be wielded properly by knights and paladins.", "knight", "paladin")]
    [InlineData("You see  item.\nIt can only be wielded properly by knights.", "knight")]
    [InlineData("You see  item.\nIt can only be wielded properly by knights, paladins and sorcerers.", "knight", "paladin", "sorcerer")]
    [InlineData("You see  item.\nIt can only be wielded properly by knights, paladins, sorcerers and druids.", "knight", "paladin", "sorcerer", "druid")]
    public void Add_HasVocations_ReturnText(string expected, params string[] vocations)
    {
        var item = ItemTestData.CreateDefenseEquipmentItem(1);
        item.Metadata.Attributes.SetAttribute(ItemAttribute.VocationNames, vocations);
        
        //act
        var actual = InspectionTextBuilder.Build(item);

        //assert
        actual.Should().Be(expected);
    }
}