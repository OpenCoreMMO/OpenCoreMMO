using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using System;

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
                Ip = Environment.GetEnvironmentVariable("SERVER_GAME_IP") ?? "127.0.0.1",
                Port = int.TryParse(Environment.GetEnvironmentVariable("SERVER_GAME_PORT"), out var port) ? port : 7172,
                Name = Environment.GetEnvironmentVariable("SERVER_GAME_NAME") ?? "OpenCore"
            }
        );
    }
}