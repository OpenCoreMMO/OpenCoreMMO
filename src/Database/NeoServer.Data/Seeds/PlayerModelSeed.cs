using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Modes;

namespace NeoServer.Data.Seeds;

internal static class PlayerModelSeed
{
    public static void Seed(EntityTypeBuilder<PlayerEntity> builder)
    {
        builder.HasData(
            CreatePlayerEntity(1, "GOD", 11, 6, 1000, 4440, 4440, 1750, 1750, 1020, 1022, 7, 2520, 75),
            CreatePlayerEntity(2, "Sorcerer Sample", 1, 1, 500, 2645, 2645, 14850, 14850, 1020, 1022, 7, 2520, 130, 69,
                95, 78, 58),
            CreatePlayerEntity(3, "Knight Sample", 4, 1, 500, 4440, 4440, 1750, 1750, 1020, 1022, 7, 2520, 131, 69, 95,
                78, 58),
            CreatePlayerEntity(4, "Druid Sample", 2, 1, 500, 4440, 4440, 1750, 1750, 1020, 1022, 7, 2520, 130, 69, 95,
                78, 58),
            CreatePlayerEntity(5, "Paladin Sample", 3, 1, 500, 4440, 4440, 1750, 1750, 1020, 1022, 7, 2520, 137, 69, 95,
                78, 58),
            // Staff Characters - for testing role-based chat colors
            CreatePlayerEntity(6, "Junior Tutor", 1, 2, 200, 2645, 2645, 8850, 8850, 1020, 1022, 7, 2520, 130, 69, 95, 78, 58),
            CreatePlayerEntity(7, "Senior Tutor", 2, 3, 350, 3440, 3440, 12750, 12750, 1020, 1022, 7, 2520, 137, 69, 95, 78, 58),
            CreatePlayerEntity(8, "Game Master", 4, 4, 800, 4440, 4440, 1750, 1750, 1020, 1022, 7, 2520, 75, 69, 95, 78, 58),
            CreatePlayerEntity(9, "Community Manager", 1, 5, 600, 3440, 3440, 15750, 15750, 1020, 1022, 7, 2520, 130, 69, 95, 78, 58),
            CreatePlayerEntity(10, "Administrator", 11, 6, 1000, 4440, 4440, 1750, 1750, 1020, 1022, 7, 2520, 75, 69, 95, 78, 58)
        );
    }

    private static PlayerEntity CreatePlayerEntity(int playerId, string name, byte vocation, byte group,
        ushort level,
        ushort health, ushort maxHealth, ushort mana, ushort maxMana,
        int posX, int posY, int posZ, ushort staminaMinutes, int lookType, int lookBody = 0, int lookFeet = 0,
        int lookHead = 0, int lookLegs = 0, int lookAddons = 0, int accountId = 1)
    {
        return new PlayerEntity
        {
            Id = playerId,
            Group = group,
            AccountId = accountId,
            TownId = 1,
            Name = name,
            ChaseMode = ChaseMode.Follow,
            Capacity = CalculateCapacity(vocation),
            Level = level,
            Health = health,
            MaxHealth = maxHealth,
            Vocation = vocation,
            Gender = GetGender(vocation),
            Speed = 800,
            Online = false,
            Mana = mana,
            MaxMana = maxMana,
            ManaSpent = 0,
            Soul = 100,
            MaxSoul = 100,
            PosX = posX,
            PosY = posY,
            PosZ = posZ,
            StaminaMinutes = staminaMinutes,
            LookType = lookType,
            LookBody = lookBody,
            LookFeet = lookFeet,
            LookHead = lookHead,
            LookLegs = lookLegs,
            LookAddons = lookAddons,
            SkillAxe = 60,
            SkillSword = 60,
            SkillClub = 60,
            SkillDist = 60,
            SkillFishing = 60,
            SkillFist = 60,
            MagicLevel = 60,
            SkillShielding = 60,
            Experience = 2058474800,
            FightMode = FightMode.Attack,
            WorldId = 1,
            BankAmount = 10000000 // 10kk para garantir testes de guild
        };
    }

    private static uint CalculateCapacity(int vocation)
    {
        return vocation switch
        {
            1 => // Sorcerer
                5390,
            2 => // Druid
                5390,
            3 => // Paladin
                10310,
            4 => // Knight
                12770,
            _ => 90000
        };
    }

    private static Gender GetGender(int vocation)
    {
        return vocation switch
        {
            1 => // Sorcerer
                Gender.Male,
            2 => // Druid
                Gender.Male,
            3 => // Paladin
                Gender.Female,
            4 => // Knight
                Gender.Male,
            _ => Gender.Male
        };
    }
}