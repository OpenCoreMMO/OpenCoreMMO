using System.Net;
using System.Net.Http.Json;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response.IpBans;
using Xunit;

namespace NeoServer.WebApi.Tests.Tests;


[Collection("Non-Parallel IpBansTests")]
public class IpBansTests : BaseIntegrationTests
{
    #region Get Tests


    [Fact(DisplayName = "Get Ban By IP")]
    public async Task Get_Ban_By_Ip()
    {
        // Arrange
        var ipBan = await CreateIpBan("127.0.0.9");

        //Act
        var response = await NeoHttpClient.GetFromJsonAsync<IpBanResponseViewModel>($"api/IpBan/{ipBan.Ip}");
        NeoContext.IpBans.Remove(ipBan);

        //Assert
        Assert.NotNull(response);
        Assert.Equal(ipBan.Ip, response.Ip);
        Assert.Equal(ipBan.Reason, response.Reason);
        Assert.Equal(ipBan.BannedAt, response.BannedAt);
        Assert.Equal(ipBan.BannedBy, response.BannedBy);
        Assert.Equal(ipBan.ExpiresAt, response.ExpiresAt);
    }


    [Fact(DisplayName = "Get Ban By IP when not found")]
    public async Task Get_Ban_By_IP_NotFound()
    {
        //Arrange
        var Ip = "198.0.10.190";

        //Act
        var response = await NeoHttpClient.GetAsync($"api/IpBan/{Ip}");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Post Test

    [Fact]
    public async Task Add_Ban_Ip()
    {
        // Arrange
        var request = new BanIpRequest()
        {
            Days = 1,
            Reason = "using bot.",
            Ip = "198.0.10.194",
        };

         //Act
         var response = await NeoHttpClient.PostAsJsonAsync($"api/IpBan", request);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}