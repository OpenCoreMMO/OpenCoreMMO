using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Creatures.Player.Modes;
using NeoServer.Domain.Creatures.Player.Outfit;
using NeoServer.Domain.Creatures.Player.Vocation;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.World.Models;
using NeoServer.Domain.World.Services;
using PathFinder = NeoServer.Domain.World.Map.PathFinder;

namespace NeoServer.Domain.Tests.Helpers.Player;

public static class PlayerTestDataBuilder
{
    public static IPlayer Build(
        uint id = 1,
        string name = "PlayerA",
        uint capacity = 100,
        ushort hp = 100,
        ushort mana = 30,
        ushort speed = 200,
        Dictionary<Slot, (IItem Item, ushort Id)> inventoryMap = null,
        Dictionary<SkillType, ISkill> skills = null,
        Dictionary<uint, int> storages = null,
        byte vocationType = 1,
        byte groupId = 1,
        IPathFinder pathFinder = null,
        IVocationStore vocationStore = null,
        IGroupStore groupStore = null,
        Guild.Guild guild = null,
        ITown town = null,
        ushort stamina = 42 * 60,
        int premiumTime = 0,
        int experience = 1,
        ushort level = 10,
        ushort attackSpeed = 2000)
    {
        if (vocationStore is null)
        {
            var vocation = new Vocation
            {
                Id = vocationType,
                Name = "Knight",
                AttackSpeed = attackSpeed
            };

            vocationStore = new VocationStore();
            vocationStore.AddOrUpdate(vocationType, vocation);
        }

        if (groupStore is null)
        {
            var group = new Group
            {
                Id = groupId,
                Name = "player"
            };

            groupStore = new GroupStore();
            groupStore.AddOrUpdate(groupId, group);
        }

        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        pathFinder ??= new PathFinder(map);
        var mapTool = new MapTool(map, pathFinder);

        var player = new Domain.Creatures.Player.Player(
            id,
            name,
            ChaseMode.Stand,
            capacity,
            hp,
            hp,
            vocationStore.Get(vocationType),
            groupStore.Get(groupId),
            Gender.Male, true, mana,
            mana,
            FightMode.Attack,
            100, 100,
            skills ?? new Dictionary<SkillType, ISkill>
            {
                {
                    SkillType.Level, new Skill(SkillType.Level, level, experience)
                    {
                        GetIncreaseRate = () => 1
                    }
                },
                {
                    SkillType.Fist, new Skill(SkillType.Fist, 10, 1)
                    {
                        GetIncreaseRate = () => 1
                    }
                },
                {
                    SkillType.Distance, new Skill(SkillType.Distance, 10, 1)
                    {
                        GetIncreaseRate = () => 1
                    }
                },
                {
                    SkillType.Shielding, new Skill(SkillType.Shielding, 10, 1)
                    {
                        GetIncreaseRate = () => 1
                    }
                }
            },
            storages ?? new Dictionary<uint, int>(),
            stamina,
            new Outfit(),
            speed,
            new Location(100, 100, 7),
            mapTool,
            town ??= new Town { Id = 1, Name = "Teste", Coordinate = new Coordinate(1011, 1008, 7) }
        )
        {
            Guild = guild,
            PremiumTime = premiumTime,
            LastLogOut = DateTime.UtcNow
        };

        if (inventoryMap is not null)
        {
            var inventory = InventoryTestDataBuilder.Build(player, inventoryMap);
            player.AddInventory(inventory);
        }

        return player;
    }

    public static Dictionary<SkillType, ISkill> GenerateSkills(ushort level)
    {
        return new Dictionary<SkillType, ISkill>
        {
            [SkillType.Axe] = new Skill(SkillType.Axe, level),
            [SkillType.Sword] = new Skill(SkillType.Sword, level),
            [SkillType.Club] = new Skill(SkillType.Club, level),
            [SkillType.Distance] = new Skill(SkillType.Distance, level),
            [SkillType.Fishing] = new Skill(SkillType.Fishing, level),
            [SkillType.Fist] = new Skill(SkillType.Fist, level),
            [SkillType.Level] = new Skill(SkillType.Level, level),
            [SkillType.Magic] = new Skill(SkillType.Magic, level),
            [SkillType.Shielding] = new Skill(SkillType.Shielding, level),
            [SkillType.Speed] = new Skill(SkillType.Speed, level)
        };
    }

    public static Dictionary<Slot, (IItem Item, ushort Id)> GenerateInventory()
    {
        return new Dictionary<Slot, (IItem Item, ushort Id)>
        {
            [Slot.Backpack] = (ItemTestDataBuilder.CreateBackpack(), 1),
            [Slot.Ammo] = (ItemTestDataBuilder.CreateAmmo(2, 10), 2),
            [Slot.Head] = (ItemTestDataBuilder.CreateBodyEquipmentItem(3, "head"), 3),
            [Slot.Left] = (ItemTestDataBuilder.CreateWeaponItem(4, "axe"), 4),
            [Slot.Body] = (ItemTestDataBuilder.CreateBodyEquipmentItem(5, "body"), 5),
            [Slot.Feet] = (ItemTestDataBuilder.CreateBodyEquipmentItem(6, "feet"), 6),
            [Slot.Right] = (ItemTestDataBuilder.CreateBodyEquipmentItem(7, "", "shield"), 7),
            [Slot.Ring] =
                (ItemTestDataBuilder.CreateDefenseEquipmentItem(8, "ring"), 8),
            [Slot.Necklace] =
                (ItemTestDataBuilder.CreateDefenseEquipmentItem(10, "necklace"),
                    10),
            [Slot.Legs] = (ItemTestDataBuilder.CreateBodyEquipmentItem(11, "legs"), 11)
        };
    }
}