using Microsoft.Extensions.DependencyInjection;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.WebApi.Tests.Factories;

namespace NeoServer.WebApi.Tests.Tests;

public class BaseIntegrationTests
{
    #region Constructors

    public BaseIntegrationTests()
    {
        lock (Lock)
        {
            NeoFactory = NeoServerWebApiWebApplicationFactory.GetInstance();
            NeoFactory.ConfigureAwait(true);
            NeoHttpClient = NeoFactory.CreateClient();

            var serviceScope = NeoFactory.Services.GetService<IServiceScopeFactory>().CreateScope();
            NeoContext = serviceScope?.ServiceProvider.GetService<NeoContext>();
        }
    }

    #endregion

    #region Protected Members

    protected readonly NeoContext NeoContext;
    protected readonly NeoServerWebApiWebApplicationFactory NeoFactory;
    protected readonly HttpClient NeoHttpClient;
    protected static readonly object Lock = new();

    #endregion

    #region Protected Methods

    private static string GenerateRandomString(int length, bool onlyNumbers = false, bool onlyLetters = false)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        if (onlyNumbers)
            chars = "0123456789";
        else if (onlyLetters)
            chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }


    private static string GenerateRandomEmail(int usernameLength)
    {
        var username = GenerateRandomString(usernameLength, onlyLetters: false);

        var domains = new[] { "example.com", "testmail.com", "demo.org", "sample.net" };

        var random = new Random();
        var domain = domains[random.Next(domains.Length)];

        return $"{username.ToLower()}@{domain}";
    }

    protected async Task<AccountEntity> CreateAccount()
    {
        var lastAccount = NeoContext.Accounts.OrderBy(c => c.Id).LastOrDefault();

        var lastId = 0;

        if (lastAccount != null)
            lastId = lastAccount.Id;

        var account = new AccountEntity
        {
            Id = ++lastId,
            EmailAddress = GenerateRandomEmail(10),
            Password = GenerateRandomString(10),
            AccountName = GenerateRandomString(10),
        };

        await NeoContext.Accounts.AddAsync(account);
        await NeoContext.SaveChangesAsync();

        return account;
    }

    protected async Task<WorldEntity> CreateWorld()
    {
        var lastWorld = NeoContext.Worlds.OrderBy(c => c.Id).LastOrDefault();

        var lastId = 0;

        if (lastWorld != null)
            lastId = lastWorld.Id;

        var world = new WorldEntity
        {
            Id = ++lastId,
            Name = GenerateRandomString(10),
            Ip = GenerateRandomString(10)
        };

        await NeoContext.Worlds.AddAsync(world);
        await NeoContext.SaveChangesAsync();

        return world;
    }

    protected async Task<PlayerEntity> CreatePlayer()
    {
        var lastAccount = NeoContext.Accounts.OrderBy(c => c.Id).LastOrDefault();

        if (lastAccount == null) lastAccount = await CreateAccount();

        var lastWorld = NeoContext.Worlds.OrderBy(c => c.Id).LastOrDefault();

        if (lastWorld == null) lastWorld = await CreateWorld();

        var lastPlayer = NeoContext.Players.OrderBy(c => c.Id).LastOrDefault();

        var lastId = 0;

        if (lastPlayer != null)
            lastId = lastPlayer.Id;

        var player = new PlayerEntity
        {
            AccountId = lastAccount.Id,
            WorldId = lastWorld.Id,
            Id = ++lastId,
            Name = GenerateRandomString(10),
            Level = 1
        };

        await NeoContext.Players.AddAsync(player);
        await NeoContext.SaveChangesAsync();

        return player;
    }

    #endregion
}