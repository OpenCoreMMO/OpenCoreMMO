using NeoServer.Domain.Houses;
using NeoServer.Domain.Tests.Helpers.House;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Houses;

public class HouseTransferTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void TryAttachTransfer_succeeds_when_house_has_no_pending_transfer()
    {
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);
        var item = CreateTransferItem(house);

        house.TryAttachTransfer(item).Should().BeTrue();
        house.PendingTransfer.Should().BeSameAs(item);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void TryAttachTransfer_fails_when_a_transfer_is_already_pending()
    {
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);
        var first = CreateTransferItem(house);
        var second = CreateTransferItem(house);

        house.TryAttachTransfer(first).Should().BeTrue();
        house.TryAttachTransfer(second).Should().BeFalse();
        house.PendingTransfer.Should().BeSameAs(first);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void ExecuteTransfer_with_a_different_item_does_not_clear_pending_transfer()
    {
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);
        var pending = CreateTransferItem(house);
        var other = CreateTransferItem(house);
        house.TryAttachTransfer(pending);

        house.ExecuteTransfer(other).Should().BeFalse();
        house.PendingTransfer.Should().BeSameAs(pending);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ExecuteTransfer_with_pending_item_clears_the_transfer()
    {
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);
        var pending = CreateTransferItem(house);
        house.TryAttachTransfer(pending);

        house.ExecuteTransfer(pending).Should().BeTrue();
        house.PendingTransfer.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ResetTransfer_allows_a_new_attach()
    {
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);
        house.TryAttachTransfer(CreateTransferItem(house));
        house.ResetTransfer();

        var next = CreateTransferItem(house);
        house.TryAttachTransfer(next).Should().BeTrue();
        house.PendingTransfer.Should().BeSameAs(next);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Transfer_document_look_text_includes_house_name()
    {
        var house = HouseTestDataBuilder.Build(name: "Sunset Lane 4", ownerGuid: 1);
        var item = CreateTransferItem(house);

        item.GetLookText(isClose: true).Should().Contain("house transfer document for 'Sunset Lane 4'");
    }

    private static HouseTransferItem CreateTransferItem(House house)
    {
        var seller = PlayerTestDataBuilder.Build(id: house.OwnerGuid == 0 ? 1 : house.OwnerGuid);
        return HouseTransferItem.Create(HouseTransferItem.CreateMetadata(), house, seller, _ => { });
    }
}
