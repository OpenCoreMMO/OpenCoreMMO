using Microsoft.EntityFrameworkCore;
using NeoServer.Web.API.Requests.Commands;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace NeoServer.WebApi.Tests.Tests;


[Collection("Non-Parallel AccountTests")]
public class AccountTests : BaseIntegrationTests
{
    #region Post Tests

    [Fact(DisplayName = "Create Account")]
    public async Task Create_Account()
    {
        //Arrange
        var requestModel = new CreateAccountRequest()
        {
            AccountName = "marcusviniciusS",
            Email = "marcus@opencoremmo.com",
            Password = "1234567890908mV",
        };

        // Act
        var response =  await NeoHttpClient.PostAsJsonAsync("api/Account", requestModel);

        var result = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(result);
    }

    [Fact(DisplayName = "Trying create account with invalid inputs")]
    public async Task Trying_Create_Account_With_Invalid_Inputs()
    {
        //Arrange
        var requestModel = new CreateAccountRequest()
        {
            AccountName = "11",
            Email = "1",
            Password = "1",
        };

        // Act
        var response = await NeoHttpClient.PostAsJsonAsync("api/Account", requestModel);

        var result = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotEmpty(result);
    }

    [Fact(DisplayName = "Trying create account with email already used.")]
    public async Task Trying_Create_Account_With_Email_AlreadyUse()
    {
        //Arrange
        var accountEntity = await CreateAccount();

        var requestModel = new CreateAccountRequest()
        {
            AccountName = accountEntity.AccountName,
            Email = accountEntity.EmailAddress,
            Password = "1234567890908mV",
        };

        // Act
        var response = await NeoHttpClient.PostAsJsonAsync("api/Account", requestModel);

        var result = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotEmpty(result);
    }


    [Fact(DisplayName = "Trying create account with accountName already used.")]
    public async Task Trying_Create_Account_With_AccountName_AlreadyUse()
    {
        //Arrange
        var accountEntity = await CreateAccount();

        var requestModel = new CreateAccountRequest()
        {
            AccountName = accountEntity.AccountName,
            Email = "marcus@opencoremmo.com",
            Password = "1234567890908mV",
        };

        // Act
        var response = await NeoHttpClient.PostAsJsonAsync("api/Account", requestModel);

        var result = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotEmpty(result);
    }
    #endregion

    #region Patch Tests

    [Fact(DisplayName = "Add Premium on Account")]
    public async Task Add_Premium_On_Account()
    {
        //Arrange
        var accountEntity = await CreateAccount();
        var requestModel = new AddPremioumDaysAccountRequest() { Days = 1, Description = "Promotion on OpenCoreMMO" };


        // Act
        var response = await NeoHttpClient.PatchAsJsonAsync($"api/Account/{accountEntity.Id}/premium", requestModel);
        var result = await response.Content.ReadAsStringAsync();
        var premiumHistory = await NeoContext.AccountPremiumHistories.Where(item => item.AccountId == accountEntity.Id).LastOrDefaultAsync();
        NeoContext.Accounts.Remove(accountEntity);


        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(result);
        Assert.NotNull(premiumHistory);
        Assert.True(premiumHistory.EndAt.AddDays(-requestModel.Days).Date == DateTime.UtcNow.Date);
        Assert.Equal(requestModel.Description, premiumHistory.Description);
    }


    [Fact(DisplayName = "Add Premium on not found Account")]
    public async Task Trying_Add_Premium_On_Account_When_NotFound()
    {
        //Arrange
        var requestModel = new AddPremioumDaysAccountRequest() { Days = 1, Description = "Promotion on OpenCoreMMO" };
        int notfoundId = 91232307;

        // Act
        var response = await NeoHttpClient.PatchAsJsonAsync($"api/Account/{notfoundId}/premium", requestModel);
        var result = await response.Content.ReadAsStringAsync();
        var premiumHistory = await NeoContext.AccountPremiumHistories.Where(item => item.AccountId == notfoundId).LastOrDefaultAsync();


        //Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotEmpty(result);
        Assert.Null(premiumHistory);
    }




    [Fact(DisplayName = "Change Password on Account")]
    public async Task Change_Password_On_Account()
    {
        //Arrange
        var accountEntity = await CreateAccount();
        var requestModel = new ChangePasswordRequest()
        {
            OldPassword = accountEntity.Password,
            NewPassword =  "1234567890908mV",
            ConfirmPassword = "1234567890908mV",
        };


        // Act
        var response = await NeoHttpClient.PatchAsJsonAsync($"api/Account/{accountEntity.Id}/change-password", requestModel);
        var result = await response.Content.ReadAsStringAsync();
        NeoContext.Accounts.Remove(accountEntity);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(result);
    }




    [Fact(DisplayName = "Change Password on with oldPasswordWrong Account")]
    public async Task Change_Password_With_WrongPassword_On_Account()
    {
        //Arrange
        var accountEntity = await CreateAccount();
        var requestModel = new ChangePasswordRequest()
        {
            OldPassword = "wrongpassword",
            NewPassword = "1234567890908mV",
            ConfirmPassword = "1234567890908mV",
        };


        // Act
        var response = await NeoHttpClient.PatchAsJsonAsync($"api/Account/{accountEntity.Id}/change-password", requestModel);
        var result = await response.Content.ReadAsStringAsync();
        NeoContext.Accounts.Remove(accountEntity);


        //Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotEmpty(result);
    }


    [Fact(DisplayName = "Ban account")]
    public async Task Ban_Account()
    {
        //Arrange
        var accountEntity = await CreateAccount();
        var requestModel = new BanAccountRequest() { Days = 1, Reason = "using ilegal software." };

        // Act
        var response = await NeoHttpClient.PatchAsJsonAsync($"api/Account/{accountEntity.Id}/ban", requestModel);
        var result = await response.Content.ReadAsStringAsync();
        NeoContext.Accounts.Remove(accountEntity);


        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(result);
    }

    #endregion
}