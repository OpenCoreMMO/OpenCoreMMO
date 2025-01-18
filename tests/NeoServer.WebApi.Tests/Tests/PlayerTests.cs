using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response;
using NeoServer.Web.API.Response.Player;
using Xunit;

namespace NeoServer.WebApi.Tests.Tests;


[Collection("Non-Parallel PlayerTests")]
public class PlayerTests : BaseIntegrationTests
{
    #region Get Tests

    [Fact(DisplayName = "Get All Players")]
    public async Task Get_All_Players()
    {
        // Arrange
        var player = await CreatePlayer();

        var playerCount = await NeoContext.Players.CountAsync();

        //Act
        var response = await NeoHttpClient.GetFromJsonAsync<BasePagedResponseViewModel<IEnumerable<PlayerResponseViewModel>>>("api/Player");
        NeoContext.Players.Remove(player);

        //Assert
        Assert.NotNull(response);
        Assert.Equal(playerCount, response.TotalRecords);
        Assert.Equal(1, response.TotalPages);
        Assert.NotEmpty(response.Data);
    }


    [Fact(DisplayName = "Get Players with filter")]
    public async Task Get_All_Players_with_Filter()
    {
        // Arrange
        var player = await CreatePlayer();
        var player2 =await CreatePlayer();
        var playerCount = await NeoContext.Players.CountAsync();

        //Act
        var response = await NeoHttpClient.GetFromJsonAsync<BasePagedResponseViewModel<IEnumerable<PlayerResponseViewModel>>>($"api/Player?name={player.Name}");
        NeoContext.Players.Remove(player);
        NeoContext.Players.Remove(player2);

        //Assert
        Assert.NotNull(response);
        Assert.NotEqual(playerCount, response.TotalRecords);
        Assert.Equal(1, response.TotalPages);
        Assert.NotEmpty(response.Data);
        Assert.Equal(player.Id, response.Data.First().Id);
    }


    [Fact(DisplayName = "Get Player By Id")]
    public async Task Get_Player_By_Id()
    {
        // Arrange
        var player = await CreatePlayer();

        //Act
        var response = await NeoHttpClient.GetFromJsonAsync<PlayerResponseViewModel>($"api/Player/{player.Id}");
        NeoContext.Players.Remove(player);

        //Assert
        Assert.NotNull(response);
        Assert.Equal(player.Name, response.Name);
        Assert.Equal(player.AccountId, response.AccountId);
        Assert.Equal(player.SkillAxe, response.SkillAxe);
        Assert.Equal(player.SkillClub, response.SkillClub);
        Assert.Equal(player.SkillSword, response.SkillSword);
        Assert.Equal(player.Capacity, response.Capacity);
        Assert.Equal(player.ChaseMode, response.ChaseMode);
        Assert.Equal(player.FightMode, response.FightMode);
        Assert.Equal(player.Experience, response.Experience);
        Assert.Equal(player.Gender, response.Gender);
    }


    [Fact(DisplayName = "Get Player By Id when not found")]
    public async Task Get_Player_By_Id_NotFound()
    {
        //Act
        var response = await NeoHttpClient.GetAsync($"api/Player/9999");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion


    #region Post Test

    [Fact]
    public async Task Create_Player()
    {

        // Arrange
        var account = CreateAccount();


        var request = new CreatePlayerRequest
        {
            AccountId = account.Id,
            Name = "new player",
            PosX = 0,
            PosY = 0,
            PosZ = 0,
            Sex = 0,
            Town = 1,
            WorldId = account.Id,
            Vocation = 1,
        };

         //Act
         var response = await NeoHttpClient.PostAsJsonAsync($"api/Player", request);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion


    #region Patch Test

    [Fact]
    public async Task Edit_Skills_Player()
    {

        // Arrange
        var player = await CreatePlayer();


        var request = new UpdatePlayerSkillsRequest
        {
            SkillClub = 10,
            SkillAxe = 10,
            SkillDist = 10,
            SkillFishing = 10,
            SkillShielding = 19,
            SkillSword = 16,
            SkillFist = 17,
        };

        //Act
        var response = await NeoHttpClient.PatchAsJsonAsync($"api/Player/{player.Id}/skills", request);


        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Fact]
    public async Task Edit_Infos_Player()
    {

        // Arrange
        var player = await CreatePlayer();


        var request = new UpdatePlayerInfosRequest
        {
            Capacity = 500,
            ChaseMode = Game.Common.Creatures.Players.ChaseMode.Stand,
            FightMode = Game.Common.Creatures.Players.FightMode.Attack,
            Group = 1,
            Gender = Game.Common.Creatures.Players.Gender.Male,
            Health = 200,
            Level = 100,
            Mana = 200,
            MaxMana = 250,
            Soul = 100,
            MaxSoul = 100,
            Vocation = 11,
            TownId = 11,
            WorldId = 1,
            MaxHealth = 200,
            Name = "PlayerNNameNew",
            Speed = 250,
        };

        //Act
        var response = await NeoHttpClient.PatchAsJsonAsync($"api/Player/{player.Id}/infos", request);

        var playerOnDatabase = await NeoContext.Players.Where(p => p.Id == player.Id).FirstOrDefaultAsync();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    #endregion
}