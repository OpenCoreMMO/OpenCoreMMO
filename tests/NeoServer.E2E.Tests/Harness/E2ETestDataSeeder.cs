using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Modes;
using NeoServer.E2E.Tests.Login;

namespace NeoServer.E2E.Tests.Harness;

internal static class E2ETestDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var context = services.GetRequiredService<NeoContext>();

        var accountExists = await context.Accounts
            .AnyAsync(account => account.AccountName == LoginTestCredentials.Account, cancellationToken);

        if (accountExists)
        {
            return;
        }

        var account = new AccountEntity
        {
            AccountName = LoginTestCredentials.Account,
            EmailAddress = LoginTestCredentials.Account,
            Password = LoginTestCredentials.Password,
            AllowManyOnline = true,
            PremiumTimeEndAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);

        var player = CreatePlayer(account.Id);
        context.Players.Add(player);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static PlayerEntity CreatePlayer(int accountId)
    {
        return new PlayerEntity
        {
            AccountId = accountId,
            TownId = 1,
            WorldId = 1,
            Name = LoginTestCredentials.CharacterName,
            Group = 1,
            Capacity = 12770,
            Level = 8,
            Health = 185,
            MaxHealth = 185,
            Mana = 90,
            MaxMana = 90,
            ManaSpent = 0,
            Soul = 100,
            MaxSoul = 100,
            StaminaMinutes = 2520,
            Online = false,
            PosX = 1003,
            PosY = 1016,
            PosZ = 7,
            LookType = 131,
            LookBody = 69,
            LookFeet = 95,
            LookHead = 78,
            LookLegs = 58,
            SkillAxe = 10,
            SkillClub = 10,
            SkillSword = 10,
            SkillDist = 10,
            SkillShielding = 10,
            SkillFishing = 10,
            SkillFist = 10,
            MagicLevel = 10,
            Experience = 4200,
            ChaseMode = ChaseMode.Stand,
            FightMode = FightMode.Attack,
            Gender = Gender.Male,
            Vocation = 4,
            Skull = Skull.None,
            BankAmount = 0,
            Conditions = null
        };
    }
}
