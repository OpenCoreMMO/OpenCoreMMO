using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Seeds;

internal sealed class PlayerItemSeed
{
    public static void Seed(EntityTypeBuilder<PlayerItemEntity> builder)
    {
        builder.HasData(
            // GOD items
            new PlayerItemEntity
            {
                Id = -1,
                ContainerId = 1,
                PlayerId = 1,
                ParentId = 0,
                ServerId = 1988,
                Amount = 1
            },
            new PlayerItemEntity
            {
                Id = -2,
                PlayerId = 1,
                ParentId = 0,
                ServerId = 2666,
                Amount = 10
            },
            new PlayerItemEntity
            {
                Id = -3,
                PlayerId = 1,
                ParentId = 0,
                ServerId = 7618,
                Amount = 10
            },
            new PlayerItemEntity
            {
                Id = -4,
                PlayerId = 1,
                ParentId = 0,
                ServerId = 2311,
                Amount = 10
            },
            new PlayerItemEntity
            {
                Id = -5,
                PlayerId = 1,
                ParentId = 1,
                ServerId = 2304,
                Amount = 10
            },
            
            // Junior Tutor (Player ID 6) - Basic equipment for level 200
            new PlayerItemEntity
            {
                Id = -6,
                PlayerId = 6,
                ParentId = 0,
                ServerId = 2160, // Crystal Coin
                Amount = 100
            },
            new PlayerItemEntity
            {
                Id = -7,
                PlayerId = 6,
                ParentId = 0,
                ServerId = 8911, // Mana Potion
                Amount = 50
            },
            
            // Senior Tutor (Player ID 7) - Level 350 equipment  
            new PlayerItemEntity
            {
                Id = -8,
                PlayerId = 7,
                ParentId = 0,
                ServerId = 2160, // Crystal Coin
                Amount = 200
            },
            new PlayerItemEntity
            {
                Id = -9,
                PlayerId = 7,
                ParentId = 0,
                ServerId = 2273, // Ultimate Health Potion
                Amount = 50
            },
            
            // Game Master (Player ID 8) - Level 800 equipment
            new PlayerItemEntity
            {
                Id = -10,
                PlayerId = 8,
                ParentId = 0,
                ServerId = 2160, // Crystal Coin
                Amount = 500
            },
            new PlayerItemEntity
            {
                Id = -11,
                PlayerId = 8,
                ParentId = 0,
                ServerId = 2520, // Demon Shield
                Amount = 1
            },
            
            // Community Manager (Player ID 9) - Level 600 equipment
            new PlayerItemEntity
            {
                Id = -12,
                PlayerId = 9,
                ParentId = 0,
                ServerId = 2160, // Crystal Coin  
                Amount = 300
            },
            new PlayerItemEntity
            {
                Id = -13,
                PlayerId = 9,
                ParentId = 0,
                ServerId = 2268, // Sudden Death Rune
                Amount = 100
            },
            
            // Administrator (Player ID 10) - Level 1000 equipment
            new PlayerItemEntity
            {
                Id = -14,
                PlayerId = 10,
                ParentId = 0,
                ServerId = 2160, // Crystal Coin
                Amount = 1000
            },
            new PlayerItemEntity
            {
                Id = -15,
                PlayerId = 10,
                ParentId = 0,
                ServerId = 1988, // Bag
                Amount = 1
            }
        );
    }
}