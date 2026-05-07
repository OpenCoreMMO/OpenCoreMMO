using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using Serilog;

namespace NeoServer.Data.Repositories.Player;

public class PlayerRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger)
    : BaseRepository<PlayerEntity>(contextOptions,
        logger), IPlayerRepository, Domain.Repositories.IPlayerRepository
{
    public void UpdateAllPlayersToOffline()
    {
        const string sql = "UPDATE Player SET Online = false";

        using var context = NewDbContext;

        if (!context.Database.IsRelational()) return;

        using var connection = context.Database.GetDbConnection();

        connection.Execute(sql);
    }

    public List<PlayerOutfitAddonEntity> GetOutfitAddons(int playerId)
    {
        using var context = NewDbContext;
        return context.PlayerOutfitAddons.Where(x => x.PlayerId == playerId).ToList();
    }

    public PlayerEntity GetByName(string playerName)
    {
        using var context = NewDbContext;
        //todo: find a way to use invariant culture. it currently doesn't work with sqlite
        return context.Players.FirstOrDefault(x => x.Name.ToLower() == playerName.ToLower());
    }

    public PlayerEntity GetById(int id)
    {
        using var context = NewDbContext;
        return context.Players.FirstOrDefault(x => x.Id == id);
    }

    public void UpdatePlayers(IEnumerable<IPlayer> players)
    {
        foreach (var player in players)
        {
            SavePlayer(player);
        }
    }

    public void UpdatePlayerOnlineStatus(uint playerId, bool status)
    {
        using var context = NewDbContext;

        var player = context.Players.SingleOrDefault(x => x.Id == playerId);
        if (player is null) return;

        player.Online = status;

        context.SaveChanges();
    }

    public void SavePlayer(IPlayer player)
    {
        using var neoContext = NewDbContext;

        UpdatePlayer(player, neoContext);
        InventoryManager.SavePlayerInventory(player, neoContext);
        InventoryManager.SaveBackpack(player, neoContext);
        StorageManager.SaveStorages(player, neoContext);

        neoContext.SaveChanges();
    }

    public int GetIdByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return 0;

        using var context = NewDbContext;

        return context.Players.FirstOrDefault(x => x.Name.ToLower() == name.ToLower())?.Id ?? 0;
    }

    public void UpdateLastLogInDate(int playerId, DateTime lastLogIn)
    {
        using var context = NewDbContext;
        var playerEntity = context.Players.Find(playerId);

        if (playerEntity is null) return;
        playerEntity.LastLogIn = lastLogIn;
        context.SaveChanges();
    }

    private static void UpdatePlayer(IPlayer player, NeoContext neoContext)
    {
        var playerEntity = neoContext.Players.Find((int)player.Id);

        if (playerEntity is null) return;

        playerEntity.Capacity = player.TotalCapacity;
        playerEntity.Level = player.Level;
        playerEntity.Mana = player.Mana;
        playerEntity.MaxMana = player.MaxMana;
        playerEntity.ManaSpent = 0;
        playerEntity.Health = player.HealthPoints;
        playerEntity.MaxHealth = player.MaxHealthPoints;
        playerEntity.Soul = player.SoulPoints;
        playerEntity.MaxSoul = player.MaxSoulPoints;
        playerEntity.StaminaMinutes = player.StaminaMinutes;
        playerEntity.LightLevel = player.LightLevel;
        playerEntity.LightColor = player.LightColor;

        playerEntity.LookAddons = player.OriginalOutfit?.Addon ?? player.Outfit.Addon;
        playerEntity.LookBody = player.OriginalOutfit?.Body ?? player.Outfit.Body;
        playerEntity.LookFeet = player.OriginalOutfit?.Feet ?? player.Outfit.Feet;
        playerEntity.LookHead = player.OriginalOutfit?.Head ?? player.Outfit.Head;
        playerEntity.LookLegs = player.OriginalOutfit?.Legs ?? player.Outfit.Legs;
        playerEntity.LookType = player.OriginalOutfit?.LookType ?? player.Outfit.LookType;
        playerEntity.PosX = player.Location.X;
        playerEntity.PosY = player.Location.Y;
        playerEntity.PosZ = player.Location.Z;

        playerEntity.SkillFist = player.GetRawSkillLevel(SkillType.Fist);
        playerEntity.SkillFishingTries = player.GetSkillTries(SkillType.Fist);
        playerEntity.SkillClub = player.GetRawSkillLevel(SkillType.Club);
        playerEntity.SkillClubTries = player.GetSkillTries(SkillType.Club);
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
            (int)(player.GetCondition(ConditionType.Regeneration) is { } condition
                ? Math.Max(0, condition.RemainingTime / 1000)
                : 0);
        playerEntity.Vocation = player.VocationType;
        playerEntity.Skull = player.Skull;
        playerEntity.SkullEndsAt = player.SkullEndsAt;
        playerEntity.LastLogOut = player.LastLogOut;

        // Update guild membership
        UpdateGuildMembership(player, neoContext);

        neoContext.Update(playerEntity);
    }

    private static void UpdateGuildMembership(IPlayer player, NeoContext neoContext)
    {
        // First, remove any existing guild membership for this player
        var existingMembership = neoContext.GuildMemberships
            .FirstOrDefault(gm => gm.PlayerId == player.Id);

        if (existingMembership != null) neoContext.GuildMemberships.Remove(existingMembership);

        // If player has a guild, create new membership
        if (player.Guild != null && player.GuildId != 0)
        {
            var guildMembership = new GuildMembershipEntity
            {
                PlayerId = (int)player.Id,
                GuildId = player.GuildId,
                RankId = player.GuildRank?.Id ?? 1, // Default to rank 1 (member) if no rank set
                Nick = player.GuildNick ?? string.Empty
            };

            neoContext.GuildMemberships.Add(guildMembership);
        }
    }
}