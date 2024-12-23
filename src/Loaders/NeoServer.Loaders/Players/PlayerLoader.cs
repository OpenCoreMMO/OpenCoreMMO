using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Data.Parsers;
using NeoServer.Game.Chats;
using NeoServer.Game.Combat.Conditions;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Creatures.Player;
using NeoServer.Game.Creatures.Player.Inventory;
using NeoServer.Loaders.Interfaces;
using NeoServer.Server.Services;
using Serilog;

namespace NeoServer.Loaders.Players;

public class PlayerLoader : IPlayerLoader
{
    private readonly GameConfiguration _gameConfiguration;
    protected readonly ChatChannelFactory ChatChannelFactory;
    protected readonly ICreatureFactory CreatureFactory;
    protected readonly IGuildStore GuildStore;
    protected readonly IItemFactory ItemFactory;
    protected readonly ILogger Logger;
    protected readonly IMapTool MapTool;
    protected readonly IVocationStore VocationStore;
    protected readonly Game.World.World World;

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public PlayerLoader(IItemFactory itemFactory, ICreatureFactory creatureFactory,
        ChatChannelFactory chatChannelFactory,
        IGuildStore guildStore,
        IVocationStore vocationStore,
        IMapTool mapTool,
        Game.World.World world,
        ILogger logger,
        GameConfiguration gameConfiguration)
    {
        ItemFactory = itemFactory;
        CreatureFactory = creatureFactory;
        ChatChannelFactory = chatChannelFactory;
        GuildStore = guildStore;
        VocationStore = vocationStore;
        MapTool = mapTool;
        World = world;
        Logger = logger;
        _gameConfiguration = gameConfiguration;
    }

    public virtual bool IsApplicable(PlayerEntity player)
    {
        return player?.PlayerType == 1;
    }

    public virtual IPlayer Load(PlayerEntity playerEntity)
    {
        if (Guard.IsNull(playerEntity)) return null;

        var vocation = GetVocation(playerEntity);
        var town = GetTown(playerEntity);

        var playerLocation =
            new Location((ushort)playerEntity.PosX, (ushort)playerEntity.PosY, (byte)playerEntity.PosZ);
        
        var currentTile = GetCurrentTile(playerLocation);
        
        var player = new Player(
            (uint)playerEntity.Id,
            playerEntity.Name,
            playerEntity.ChaseMode,
            playerEntity.Capacity,
            playerEntity.Health,
            playerEntity.MaxHealth,
            vocation,
            playerEntity.Gender,
            playerEntity.Online,
            playerEntity.Mana,
            playerEntity.MaxMana,
            playerEntity.FightMode,
            playerEntity.Soul,
            vocation.SoulMax,
            ConvertToSkills(playerEntity),
            playerEntity.StaminaMinutes,
            new Outfit
            {
                Addon = (byte)playerEntity.LookAddons,
                Body = (byte)playerEntity.LookBody,
                Feet = (byte)playerEntity.LookFeet,
                Head = (byte)playerEntity.LookHead,
                Legs = (byte)playerEntity.LookLegs,
                LookType = (byte)playerEntity.LookType
            },
            0,
            playerLocation,
            MapTool,
            town)
        {
            PremiumTime = playerEntity.Account?.PremiumTime ?? 0,
            AccountId = (uint)playerEntity.AccountId,
            Guild = GuildStore.Get((ushort)(playerEntity.GuildMember?.GuildId ?? 0)),
            GuildLevel = (ushort)(playerEntity.GuildMember?.RankId ?? 0)
        };

        player.SetCurrentTile(currentTile);

        AddRegenerationCondition(playerEntity, player);

        player.AddInventory(ConvertToInventory(player, playerEntity));

        AddExistingPersonalChannels(player);

        return CreatureFactory.CreatePlayer(player);
    }

    protected ITown GetTown(PlayerEntity playerEntity)
    {
        if (!World.TryGetTown((ushort)playerEntity.TownId, out var town))
            Logger.Error("player town not found: {PlayerModelTownId}", playerEntity.TownId);
        return town;
    }

    protected IVocation GetVocation(PlayerEntity playerEntity)
    {
        if (!VocationStore.TryGetValue(playerEntity.Vocation, out var vocation))
            Logger.Error("Player vocation not found: {PlayerModelVocation}", playerEntity.Vocation);
        return vocation;
    }

    protected IDynamicTile GetCurrentTile(Location location)
    {
        World.TryGetTile(ref location, out var dynamicTile);
        return dynamicTile as IDynamicTile;
    }

    private static void AddRegenerationCondition(PlayerEntity playerEntity, IPlayer player)
    {
        if (playerEntity.RemainingRecoverySeconds != 0)
        {
            player.AddCondition(
                new Condition(ConditionType.Regeneration, (uint)(playerEntity.RemainingRecoverySeconds * 1000),
                    player.SetAsHungry));
            return;
        }

        player.SetAsHungry();
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

    protected Dictionary<SkillType, ISkill> ConvertToSkills(PlayerEntity playerRecord)
    {
        return new Dictionary<SkillType, ISkill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, (ushort)playerRecord.SkillAxe, playerRecord.SkillAxeTries)
                { GetIncreaseRate = () => _gameConfiguration.SkillsRate["axe"] },

            [SkillType.Club] = new Skill(SkillType.Club, (ushort)playerRecord.SkillClub, playerRecord.SkillClubTries)
                { GetIncreaseRate = () => _gameConfiguration.SkillsRate["club"] },

            [SkillType.Distance] = new Skill(SkillType.Distance, (ushort)playerRecord.SkillDist,
                    playerRecord.SkillDistTries)
                { GetIncreaseRate = () => _gameConfiguration.SkillsRate["distance"] },

            [SkillType.Fishing] = new Skill(SkillType.Fishing, (ushort)playerRecord.SkillFishing,
                    playerRecord.SkillFishingTries)
                { GetIncreaseRate = () => _gameConfiguration.SkillsRate["fishing"] },

            [SkillType.Fist] = new Skill(SkillType.Fist, (ushort)playerRecord.SkillFist, playerRecord.SkillFistTries)
                { GetIncreaseRate = () => _gameConfiguration.SkillsRate["fist"] },

            [SkillType.Shielding] = new Skill(SkillType.Shielding, (ushort)playerRecord.SkillShielding,
                    playerRecord.SkillShieldingTries)
                { GetIncreaseRate = () => _gameConfiguration.SkillsRate["shielding"] },

            [SkillType.Level] = new Skill(SkillType.Level, playerRecord.Level, playerRecord.Experience),

            [SkillType.Magic] =
                new Skill(SkillType.Magic, (ushort)playerRecord.MagicLevel, playerRecord.MagicLevelTries)
                    { GetIncreaseRate = () => _gameConfiguration.SkillsRate["magic"] },

            [SkillType.Sword] =
                new Skill(SkillType.Sword, (ushort)playerRecord.SkillSword, playerRecord.SkillSwordTries)
                    { GetIncreaseRate = () => _gameConfiguration.SkillsRate["sword"] }
        };
    }

    protected IInventory ConvertToInventory(IPlayer player, PlayerEntity playerRecord)
    {
        var inventory = new Dictionary<Slot, (IItem Item, ushort Id)>();
        var attrs = new Dictionary<ItemAttribute, IConvertible> { { ItemAttribute.Count, 0 } };

        foreach (var item in playerRecord.PlayerInventoryItems)
        {
            attrs[ItemAttribute.Count] = (byte)item.Amount;
            var location = item.SlotId <= 10 ? Location.Inventory((Slot)item.SlotId) : Location.Container(0, 0);

            var createdItem = ItemFactory.Create((ushort)item.ServerId, location, attrs);
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