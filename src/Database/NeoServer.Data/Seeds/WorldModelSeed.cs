using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Seeds;

public class WorldModelSeed
{
    public static void Seed(EntityTypeBuilder<WorldEntity> builder)
    {
        builder.HasData
        (
            new WorldEntity
            {
                Id = 1,
                Ip = "127.0.0.1",
                Port = 7172,
                Name = "OpenCore"
            }
        );
    }
}