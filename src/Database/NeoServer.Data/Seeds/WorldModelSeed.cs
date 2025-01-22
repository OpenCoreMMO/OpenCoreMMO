using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using Type = NeoServer.Data.Entities.Type;

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
                Name = Environment.GetEnvironmentVariable("SERVER_GAME_NAME") ?? "OpenCore",
                Region = Region.SouthAmerica,
                PvpType = PvpType.HardCore,
                Type = Type.Experimental,
                RequiresPremium = true,
                TransferEnabled = false,
                AntiCheatEnabled = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}