using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using Serilog;

namespace NeoServer.Data.Repositories.Player;

public class PlayerRepository : BaseRepository<PlayerEntity>, IPlayerRepository
{
    #region constructors

    public PlayerRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger) : base(contextOptions,
        logger)
    {
    }

    #endregion

    public async Task UpdateAllPlayersToOffline()
    {
        const string sql = @"UPDATE Player SET Online = 0";

        await using var context = NewDbContext;

        if (!context.Database.IsRelational()) return;

        await using var connection = context.Database.GetDbConnection();

        await connection.ExecuteAsync(sql);
    }

    public async Task Add(PlayerEntity player)
    {
        await using var context = NewDbContext;
        await context.AddAsync(player);
        await context.SaveChangesAsync();
    }

    public async Task<List<PlayerOutfitAddonEntity>> GetOutfitAddons(int playerId)
    {
        await using var context = NewDbContext;
        return await context.PlayerOutfitAddons.Where(x => x.PlayerId == playerId).ToListAsync();
    }

    public async Task<PlayerEntity> GetPlayer(string playerName)
    {
        await using var context = NewDbContext;
        return await context.Players.FirstOrDefaultAsync(x => x.Name.Equals(playerName));
    }

    public async Task UpdatePlayers(IEnumerable<IPlayer> players)
    {
        var tasks = new List<Task>();

        foreach (var player in players)
        {
            tasks.Clear();
            tasks.Add(SavePlayer(player));
        }

        await Task.WhenAll(tasks);
    }

    public async Task UpdatePlayerOnlineStatus(uint playerId, bool status)
    {
        await using var context = NewDbContext;

        var player = await context.Players.SingleOrDefaultAsync(x => x.Id == playerId);
        if (player is null) return;

        player.Online = status;

        await context.SaveChangesAsync();
    }

    public async Task SavePlayer(IPlayer player)
    {
        await using var neoContext = NewDbContext;

        await UpdatePlayer(player, neoContext);
        await InventoryManager.SavePlayerInventory(player, neoContext);
        await InventoryManager.SaveBackpack(player, neoContext);
        await StorageManager.SaveStorages(player, neoContext);

        await neoContext.SaveChangesAsync();
    }

    private static async Task UpdatePlayer(IPlayer player, NeoContext neoContext)
    {
        var playerEntity = await neoContext.Players.FindAsync((int)player.Id);

        if (playerEntity is null) return;

        playerEntity.Capacity = player.TotalCapacity;
        playerEntity.Level = player.Level;
        playerEntity.Mana = player.Mana;
        playerEntity.MaxMana = player.MaxMana;
        playerEntity.Health = player.HealthPoints;
        playerEntity.MaxHealth = player.MaxHealthPoints;
        playerEntity.Soul = player.SoulPoints;
        playerEntity.MaxSoul = player.MaxSoulPoints;
        playerEntity.Speed = player.Speed;
        playerEntity.StaminaMinutes = player.StaminaMinutes;

        playerEntity.LookAddons = player.Outfit.Addon;
        playerEntity.LookBody = player.Outfit.Body;
        playerEntity.LookFeet = player.Outfit.Feet;
        playerEntity.LookHead = player.Outfit.Head;
        playerEntity.LookLegs = player.Outfit.Legs;
        playerEntity.LookType = player.Outfit.LookType;
        playerEntity.PosX = player.Location.X;
        playerEntity.PosY = player.Location.Y;
        playerEntity.PosZ = player.Location.Z;

        playerEntity.SkillFist = player.GetRawSkillLevel(SkillType.Fist);
        playerEntity.SkillFishingTries = player.GetSkillTries(SkillType.Fist);
        playerEntity.SkillClub = player.GetRawSkillLevel(SkillType.Club);
        playerEntity.SkillFishingTries = player.GetSkillTries(SkillType.Club);
        playerEntity.SkillSword = player.GetRawSkillLevel(SkillType.Sword);
        playerEntity.SkillSwordTries = player.GetSkillTries(SkillType.Sword);
        playerEntity.SkillAxe = player.GetRawSkillLevel(SkillType.Axe);
        playerEntity.SkillAxeTries = player.GetSkillTries(SkillType.Axe);
        playerEntity.SkillDist = player.GetRawSkillLevel(SkillType.Distance);
        playerEntity.SkillDistTries = player.GetSkillTries(SkillType.Distance);
        playerEntity.SkillShielding = player.GetRawSkillLevel(SkillType.Shielding);
        playerEntity.SkillShieldingTries = player.GetSkillTries(SkillType.Shielding);
        playerEntity.SkillFishing = player.GetRawSkillLevel(SkillType.Fishing);
        playerEntity.SkillFishingTries = player.GetSkillTries(SkillType.Fishing);
        playerEntity.MagicLevel = player.GetRawSkillLevel(SkillType.Magic);
        playerEntity.MagicLevelTries = player.GetSkillTries(SkillType.Magic);
        playerEntity.Experience = player.Experience;
        playerEntity.ChaseMode = player.ChaseMode;
        playerEntity.FightMode = player.FightMode;
        playerEntity.RemainingRecoverySeconds =
            (int)(player.Conditions.TryGetValue(ConditionType.Regeneration, out var condition)
                ? condition.RemainingTime / TimeSpan.TicksPerMillisecond
                : 0);
        playerEntity.Vocation = player.VocationType;

        neoContext.Update(playerEntity);
    }
}