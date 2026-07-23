using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items.Inspection;
using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Items.Inspection;

public class GateOfExpertiseInspectionTextBuilderTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void Build_closed_gate_with_level_requirement_shows_level_and_worthy_line_when_close()
    {
        var item = CreateGateItem(actionId: 1020);

        var actual = InspectionTextBuilder.Build(item, isClose: true);

        actual.Should().Be("You see a gate of expertise for level 20.\nOnly the worthy may pass.");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Build_closed_gate_with_any_level_action_id_shows_any_level_text()
    {
        var item = CreateGateItem(actionId: 1000);

        var actual = InspectionTextBuilder.Build(item, isClose: true);

        actual.Should().Be("You see a gate of expertise for any level.");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Build_open_gate_without_leveldoor_metadata_still_shows_level_from_preserved_action_id()
    {
        var item = CreateGateItem(actionId: 1020, includeLevelDoorMetadata: false);

        var actual = InspectionTextBuilder.Build(item, isClose: true);

        actual.Should().Be("You see a gate of expertise for level 20.\nOnly the worthy may pass.");
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Build_gate_without_action_id_shows_plain_name()
    {
        var item = CreateGateItem();

        var actual = InspectionTextBuilder.Build(item, isClose: true);

        actual.Should().Be("You see a gate of expertise.");
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Build_gate_with_action_id_below_base_shows_plain_name()
    {
        var item = CreateGateItem(actionId: 500);

        var actual = InspectionTextBuilder.Build(item, isClose: true);

        actual.Should().Be("You see a gate of expertise.");
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Build_quest_door_with_high_action_id_remains_unchanged()
    {
        var item = CreateDoorItem(name: "quest door", actionId: 5000);

        var actual = InspectionTextBuilder.Build(item, isClose: true);

        actual.Should().Be("You see a quest door.");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Build_gate_from_far_look_shows_level_without_worthy_line()
    {
        var item = CreateGateItem(actionId: 1020);

        var actual = InspectionTextBuilder.Build(item, isClose: false);

        actual.Should().Be("You see a gate of expertise for level 20.");
    }

    private static IItem CreateGateItem(ushort actionId = 0, bool includeLevelDoorMetadata = true)
    {
        return CreateDoorItem("gate of expertise", actionId, includeLevelDoorMetadata);
    }

    private static IItem CreateDoorItem(string name, ushort actionId = 0, bool includeLevelDoorMetadata = false)
    {
        var itemTypeAttributes = new List<(ItemTypeAttribute, IConvertible)>
        {
            (ItemTypeAttribute.Type, "door")
        };

        if (includeLevelDoorMetadata)
        {
            itemTypeAttributes.Add((ItemTypeAttribute.LevelDoor, GateOfExpertiseInspectionTextBuilder.LevelDoorActionIdBase));
        }

        (ItemAttribute, IConvertible)[] itemAttributes = actionId > 0
            ?
            [
                (ItemAttribute.ActionId, actionId),
                (ItemAttribute.Count, 1)
            ]
            :
            [
                (ItemAttribute.Count, 1)
            ];

        var item = ItemTestDataBuilder.CreateRegularItem(1227, itemTypeAttributes.ToArray(), itemAttributes);
        item.Metadata.SetName(name);
        item.Metadata.SetArticle("a");
        return item;
    }
}
