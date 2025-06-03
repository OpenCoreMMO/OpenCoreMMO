using FluentAssertions;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Items.Container;

public class DepotTests
{
    [Fact]
    public void Depot_doesnt_open_if_it_is_already_opened_by_another_player()
    {
        //arrange
        var player1 = PlayerTestDataBuilder.Build();
        var player2 = PlayerTestDataBuilder.Build(2);

        var depot = ItemTestData.CreateDepot();

        player1.Use(depot, 0);

        //act
        player2.Use(depot, 0);

        //assert
        player2.Containers.IsOpened(0).Should().BeFalse();
        player1.Containers.IsOpened(0).Should().BeTrue();
    }

    [Fact]
    public void Depot_can_be_opened_by_others_after_player_closes_it()
    {
        //arrange
        var player1 = PlayerTestDataBuilder.Build();
        var player2 = PlayerTestDataBuilder.Build(2);

        var depot = ItemTestData.CreateDepot();

        player1.Use(depot, 0);

        //act
        player2.Use(depot, 0);

        //assert
        player2.Containers.IsOpened(0).Should().BeFalse();
        player1.Containers.IsOpened(0).Should().BeTrue();

        //act
        player1.Containers.CloseContainer(0);
        player2.Use(depot, 0);

        //assert
        player2.Containers.IsOpened(0).Should().BeTrue();
        player1.Containers.IsOpened(0).Should().BeFalse();
    }

    [Fact]
    public void Depot_can_be_opened_by_others_after_player_closes_an_inner_container()
    {
        //arrange
        var player1 = PlayerTestDataBuilder.Build();
        var player2 = PlayerTestDataBuilder.Build(2);

        var depot = ItemTestData.CreateDepot();
        var backpack = ItemTestData.CreateBackpack();
        depot.AddItem(backpack);

        player1.Use(depot, 0);

        //act
        player2.Use(depot, 0);

        //assert
        player2.Containers.IsOpened(0).Should().BeFalse();
        player1.Containers.IsOpened(0).Should().BeTrue();

        //act
        player1.Use(backpack, 0);
        player1.Containers.CloseContainer(0);
        player2.Use(depot, 0);

        //assert
        player2.Containers.IsOpened(0).Should().BeTrue();
        player1.Containers.IsOpened(0).Should().BeFalse();
    }
}