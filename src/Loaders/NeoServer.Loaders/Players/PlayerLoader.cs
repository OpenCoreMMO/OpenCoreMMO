using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Data.Extensions;
using NeoServer.Data.Parsers;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Chat.Factory;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Creatures.Player.Outfit;
using NeoServer.Domain.Creatures.Player.Vocation;
using NeoServer.Domain.Guild;
using NeoServer.Loaders.Interfaces;
using Serilog;

namespace NeoServer.Loaders.Players;

[method: SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public class PlayerLoader(
    IItemFactory itemFactory,
    ICreatureFactory creatureFactory,
    ChatChannelFactory chatChannelFactory,
    IGuildStore guildStore,
    IVocationStore vocationStore,
    IGroupStore groupStore,
    IMapTool mapTool,
    Domain.World.World world,
    ILogger logger,
    GameConfiguration gameConfiguration)
    : IPlayerLoader
{
    protected readonly ChatChannelFactory ChatChannelFactory = chatChannelFactory;
    protected readonly ICreatureFactory CreatureFactory = creatureFactory;
    protected readonly IGroupStore GroupStore = groupStore;
    protected readonly IGuildStore GuildStore = guildStore;
    protected readonly IItemFactory ItemFactory = itemFactory;
    protected readonly ILogger Logger = logger;
    protected readonly IMapTool MapTool = mapTool;
    protected readonly IVocationStore VocationStore = vocationStore;
    protected readonly Domain.World.World World = world;

    public virtual bool IsApplicable(PlayerEntity player)
    {
        return player?.Group == 1;
    }

    public virtual IPlayer Load(PlayerEntity playerEntity)
    {
        if (Guard.IsNull(playerEntity)) return null;

        var vocation = GetVocation(playerEntity);
        var group = GetGroup(playerEntity);
        var town = GetTown(playerEntity);

        var playerLocation =
            new Location((ushort)playerEntity.PosX, (ushort)playerEntity.PosY, (byte)playerEntity.PosZ);

        var currentTile = GetCurrentTile(playerLocation);

        var premiumTimeDays = (ushort)(playerEntity.Account?.PremiumTimeEndAt is null
            ? 0
            : (playerEntity.Account.PremiumTimeEndAt.Value - DateTime.UtcNow).TotalDays);

        var player = new Player(
            (uint)playerEntity.Id,
            playerEntity.Name,
            playerEntity.ChaseMode,
            playerEntity.Capacity,
            playerEntity.Health,
            playerEntity.MaxHealth,
            vocation,
            group,
            playerEntity.Gender,
            playerEntity.Online,
            playerEntity.Mana,
            playerEntity.MaxMana,
            playerEntity.FightMode,
            playerEntity.Soul,
            vocation.SoulMax,
            ConvertToSkills(playerEntity),
            ConvertToStorages(playerEntity),
            playerEntity.StaminaMinutes,
            new Outfit
            {
                Addon = (byte)playerEntity.LookAddons,
                Body = (byte)playerEntity.LookBody,
                Feet = (byte)playerEntity.LookFeet,
                Head = (byte)playerEntity.LookHead,
                Legs = (byte)playerEntity.LookLegs,
                LookType = (ushort)playerEntity.LookType
            },
            0,
            playerLocation,
            MapTool,
            town)
        {
            PremiumDays = premiumTimeDays,
            AccountId = (uint)playerEntity.AccountId,
            WorldId = playerEntity.WorldId,
            Guild = GuildStore.Get((ushort)(playerEntity.GuildMember?.GuildId ?? 0)),
            GuildId = (ushort)(playerEntity.GuildMember?.GuildId ?? 0),
            GuildLevel = (ushort)(playerEntity.GuildMember?.RankId ?? 0),
            LastLogOut = playerEntity.LastLogOut
        };

        if (!gameConfiguration.StaminaEnabled) player.Group.EnableFlag(PlayerFlag.IgnoreStamina);

        player.PlayerSkull = new PlayerSkull(player, playerEntity.Skull, playerEntity.SkullEndsAt);

        player.SetCurrentTile(currentTile);

        // Set GuildRank if player is in a guild
        if (playerEntity.GuildMember?.Rank != null)
        {
            var guildRank = playerEntity.GuildMember.Rank;
            player.GuildRank = new GuildRankInfo((ushort)guildRank.Id, guildRank.Name, (byte)guildRank.Level);
        }

        player.LoadConditions(JsonExtensions.DeserializeConditions(playerEntity.Conditions));

        player.AddInventory(ConvertToInventory(player, playerEntity));

        SetNumberOfKills(playerEntity, player);

        AddExistingPersonalChannels(player);

        player.LoadBank(playerEntity.BankAmount);

        return CreatureFactory.CreatePlayer(player);
    }

    private static void SetNumberOfKills(PlayerEntity playerEntity, IPlayer player)
    {
        var killsLastDay = 0;
        var killsLastWeek = 0;
        var killsLastMonth = 0;
        foreach (var kill in playerEntity.KillsLastMonth)
        {
            if (kill.DeathDateTime >= DateTime.UtcNow.AddDays(-1)) killsLastDay++;
            if (kill.DeathDateTime >= DateTime.UtcNow.AddDays(-7)) killsLastWeek++;
            if (kill.DeathDateTime >= DateTime.UtcNow.AddMonths(-1)) killsLastMonth++;
        }

        player.SetNumberOfKills(killsLastDay, killsLastWeek, killsLastMonth);
    }

    protected ITown GetTown(PlayerEntity playerEntity)
    {
        if (!World.TryGetTown((ushort)playerEntity.TownId, out var town))
            Logger.Error("player town not found: {PlayerModelTownId}", playerEntity.TownId);
        return town;
    }

    protected Vocation GetVocation(PlayerEntity playerEntity)
    {
        if (!VocationStore.TryGetValue(playerEntity.Vocation, out var vocation))
            Logger.Error("Player vocation not found: {PlayerModelVocation}", playerEntity.Vocation);
        return vocation;
    }

    protected Group GetGroup(PlayerEntity playerEntity)
    {
        if (!GroupStore.TryGetValue(playerEntity.Group, out var group))
            Logger.Error("Player group not found: {PlayerModelGroup}", playerEntity.Group);
        return group;
    }

    protected IDynamicTile GetCurrentTile(Location location)
    {
        World.TryGetTile(ref location, out var dynamicTile);
        return dynamicTile as IDynamicTile;
    }

    /// <summary>
    ///     Adds all PersonalChatChannel assemblies to Player
    /// </summary>
    protected virtual void AddExistingPersonalChannels(IPlayer player)
    {
        if (player is null) return;

        var personalChannels = GameAssemblyCache.Cache
            .Where(x => typeof(PersonalChatChannel).IsAssignableFrom(x));
        foreach (var channel in personalChannels)
        {
            if (channel == typeof(PersonalChatChannel)) continue;

            var createdChannel = ChatChannelFactory.Create(channel, null, player);
            player.Channels.AddPersonalChannel(createdChannel);
        }
    }

    protected Dictionary<SkillType, Skill> ConvertToSkills(PlayerEntity playerRecord)
    {
        return new Dictionary<SkillType, Skill>
        {
            [SkillType.Axe] = new(SkillType.Axe, (ushort)playerRecord.SkillAxe, playerRecord.SkillAxeTries)
                { GetIncreaseRate = () => gameConfiguration.SkillsRate["axe"] },

            [SkillType.Club] = new(SkillType.Club, (ushort)playerRecord.SkillClub, playerRecord.SkillClubTries)
                { GetIncreaseRate = () => gameConfiguration.SkillsRate["club"] },

            [SkillType.Distance] = new(SkillType.Distance, (ushort)playerRecord.SkillDist,
                    playerRecord.SkillDistTries)
                { GetIncreaseRate = () => gameConfiguration.SkillsRate["distance"] },

            [SkillType.Fishing] = new(SkillType.Fishing, (ushort)playerRecord.SkillFishing,
                    playerRecord.SkillFishingTries)
                { GetIncreaseRate = () => gameConfiguration.SkillsRate["fishing"] },

            [SkillType.Fist] = new(SkillType.Fist, (ushort)playerRecord.SkillFist, playerRecord.SkillFistTries)
                { GetIncreaseRate = () => gameConfiguration.SkillsRate["fist"] },

            [SkillType.Shielding] = new(SkillType.Shielding, (ushort)playerRecord.SkillShielding,
                    playerRecord.SkillShieldingTries)
                { GetIncreaseRate = () => gameConfiguration.SkillsRate["shielding"] },

            [SkillType.Level] = new(SkillType.Level, playerRecord.Level, playerRecord.Experience),

            [SkillType.Magic] =
                new(SkillType.Magic, (ushort)playerRecord.MagicLevel, playerRecord.MagicLevelTries)
                    { GetIncreaseRate = () => gameConfiguration.SkillsRate["magic"] },

            [SkillType.Sword] =
                new(SkillType.Sword, (ushort)playerRecord.SkillSword, playerRecord.SkillSwordTries)
                    { GetIncreaseRate = () => gameConfiguration.SkillsRate["sword"] }
        };
    }

    protected Dictionary<uint, int> ConvertToStorages(PlayerEntity playerRecord)
    {
        return playerRecord.PlayerStorages?.ToDictionary(c => c.Key, c => c.Value);
    }

    protected IInventory ConvertToInventory(IPlayer player, PlayerEntity playerRecord)
    {
        var inventory = new Dictionary<Slot, (IItem Item, ushort Id)>();
        var attrs = new Dictionary<ItemTypeAttribute, IConvertible> { { ItemTypeAttribute.Count, 0 } };

        foreach (var item in playerRecord.PlayerInventoryItems)
        {
            attrs[ItemTypeAttribute.Count] = (byte)item.Amount;
            var location = item.SlotId <= 10 ? Location.Inventory((Slot)item.SlotId) : Location.Container(0, 0);

            //todo: check this, if need pass Metadata to itemFactory.Create
            var createdItem = ItemFactory.Create((ushort)item.ServerId, location, null, null, item.GetAttributes(),
                item.GetCustomAttributes());

            var createdItemIsPickupable = createdItem?.IsPickupable ?? false;

            if (!createdItemIsPickupable) continue;

            if (item.SlotId == (int)Slot.Backpack)
            {
                if (createdItem is not IContainer container) continue;

                ItemEntityParser.BuildContainer(container, playerRecord.PlayerItems.ToList(), location, ItemFactory);
            }

            inventory.Add((Slot)item.SlotId, (createdItem, (ushort)item.ServerId));
        }

        return new Inventory(player, inventory);
    }
}