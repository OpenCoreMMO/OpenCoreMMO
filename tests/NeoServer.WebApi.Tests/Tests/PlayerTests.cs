using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NeoServer.Web.Shared.Extensions;
using NeoServer.Web.Shared.ViewModels.Response;
using Xunit;

namespace NeoServer.WebApi.Tests.Tests;

public class PlayerTests : BaseIntegrationTests
{
    #region Get Tests

    [Fact(DisplayName = "Get All Players")]
    public async Task Get_All_Players()
    {
        // Arrange
        var productsCount = await NeoContext.Players.CountAsync();

        // Act
        var response =
            await NeoHttpClient.GetAndDeserialize<IEnumerable<PlayerResponseViewModel>>("api/Player");

        //Assert
        response.Count().Should().Be(productsCount);
    }

    [Fact(DisplayName = "Get Player By Id")]
    public async Task Get_Player_By_Id()
    {
        // Arrange
        var player = await CreatePlayer();

        // Act
        var response =
            await NeoHttpClient.GetAndDeserialize<PlayerResponseViewModel>($"api/Player/{player.Id}");

        //Assert
        response.Name.Should().Be(player.Name);
    }


    #endregion
}