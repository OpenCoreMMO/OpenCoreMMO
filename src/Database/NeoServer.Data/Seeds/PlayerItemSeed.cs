using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Seeds;

internal sealed class PlayerItemSeed
{
    public static void Seed(EntityTypeBuilder<PlayerItemEntity> builder)
    {
       
        
        builder.HasData(
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
            }
        );
        
        var id = -6;
        for (var playerId = 1; playerId <= 5; playerId++)
        {
            builder.HasData(new PlayerItemEntity
                {
                    Id = id--,
                    PlayerId = playerId,
                    ParentId = 0,
                    ServerId = 2554, //shovel
                    Amount = 1
                },
                new PlayerItemEntity
                {
                    Id =  id--,
                    PlayerId = playerId,
                    ParentId = 0,
                    ServerId = 2120, //rope
                    Amount = 1
                });
        }
    }
}