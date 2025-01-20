using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Entities;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Player;
using NeoServer.Web.API.Response.World;
using Xunit;
using Type = NeoServer.Data.Entities.Type;

namespace NeoServer.WebApi.Tests.Tests;


[Collection("Non-Parallel WorldTests")]
public class WorldTests : BaseIntegrationTests
{
    #region Get Tests

    [Fact(DisplayName = "Get All Worlds")]
    public async Task Get_All_Worlds()
    {
        // Arrange
        var world = await CreateWorld();

        var worldCount = await NeoContext.Worlds.CountAsync();

        //Act
        var response = await NeoHttpClient.GetFromJsonAsync<BasePagedResponseViewModel<IEnumerable<PlayerResponseViewModel>>>("api/World");

        //Assert
        Assert.NotNull(response);
        Assert.Equal(worldCount, response.TotalRecords);
        Assert.Equal(1, response.TotalPages);
        Assert.NotEmpty(response.Data);
    }


    [Fact(DisplayName = "Get World with filter")]
    public async Task Get_All_Worlds_with_Filter()
    {
        // Arrange
        var world = await CreateWorld();
        var world2 =await CreateWorld();
        var worldCount = await NeoContext.Worlds.CountAsync();
    
        //Act
        var response = await NeoHttpClient.GetFromJsonAsync<BasePagedResponseViewModel<IEnumerable<WorldResponseViewModel>>>($"api/World?name={world.Name}");
        NeoContext.Worlds.Remove(world);
        NeoContext.Worlds.Remove(world2);
    
        //Assert
        Assert.NotNull(response);
        Assert.NotEqual(worldCount, response.TotalRecords);
        Assert.Equal(1, response.TotalPages);
        Assert.NotEmpty(response.Data);
        Assert.Equal(world.Id, response.Data.First().Id);
    }


    [Fact(DisplayName = "Get World By Id")]
    public async Task Get_World_By_Id()
    {
        // Arrange
        var world = await CreateWorld();

        //Act
        var response = await NeoHttpClient.GetFromJsonAsync<PlayerResponseViewModel>($"api/world/{world.Id}");

        //Assert
        Assert.NotNull(response);
    }


    [Fact(DisplayName = "Get World By Id when not found")]
    public async Task Get_World_By_Id_NotFound()
    {
        //Act
        var response = await NeoHttpClient.GetAsync($"api/World/9999");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion


    #region Post Test
    
    [Fact(DisplayName = "Create World")]
    public async Task Create_World()
    {
        // Arrange
        var request = new CreateWorldRequest
        {
            Name = "World of Tibia",
            Ip = "192.168.1.1",
            Port = 7171,
            Region = Region.Europe,
            PvpType = PvpType.HardCore,
            Type = Type.Regular,
            RequiresPremium = true,
            TransferEnabled = true,
            AntiCheatEnabled = true,
            MaxCapacity = 500
        };
    
         //Act
         var response = await NeoHttpClient.PostAsJsonAsync($"api/World", request);
    
        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
    
}