using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeoServer.Data.Entities;
using NeoServer.Domain.Chat.Factory;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Creatures.Common;
using NeoServer.Domain.Guild;
using NeoServer.Loaders.Interfaces;
using Serilog;

namespace NeoServer.Loaders.Guilds;

public class GuildLoader(ILogger logger, ChatChannelFactory chatChannelFactory, IGuildStore guildStore)
    : ICustomLoader
{
    public async Task<Guild> LoadAsync(GuildEntity guildEntity)
    {
        if (guildEntity is null) return null;

        var guild = await GetOrCreateGuildAsync(guildEntity);
        if (guild == null) return null;

        // Load ranks
        if (guildEntity.Ranks?.Any() == true)
            foreach (var rank in guildEntity.Ranks)
                guild.AddRank((ushort)rank.Id, rank.Name, (byte)rank.Level);

        // Guild already added to store in GetOrCreateGuildAsync
        logger.Debug("Guild {Guild} loaded with {MemberCount} members", guildEntity.Name, guild.MemberCount);

        return guild;
    }

    public Guild Load(GuildEntity guildEntity)
    {
        // Synchronous wrapper for backward compatibility
        return LoadAsync(guildEntity).GetAwaiter().GetResult();
    }

    private async Task<Guild> GetOrCreateGuildAsync(GuildEntity guildEntity)
    {
        var existingGuild = guildStore.Get((ushort)guildEntity.Id);
        if (existingGuild != null) return existingGuild;

        // Create guild with all properties first
        var guild = new Guild
        {
            Id = (ushort)guildEntity.Id,
            Name = guildEntity.Name,
            Motd = guildEntity.Modt ?? string.Empty,
            CreatedDate = guildEntity.CreatedAt,
            OwnerId = (ushort)guildEntity.OwnerId,
            MemberCount = (uint)guildEntity.Members.Count,
            Bank = new Bank(guildEntity.BankAmount),
            GuildLevels = new Dictionary<ushort, GuildLevel>()
        };

        // Add guild to store first so CreateGuildChannel can find it
        guildStore.AddOrUpdate(guild.Id, guild);

        // Now create the guild channel with the complete guild
        guild.Channel = chatChannelFactory.CreateGuildChannel($"{guildEntity.Name ?? "Unknown Guild"}'s Channel",
            (ushort)guildEntity.Id);

        return guild;
    }

    private static void AddMembers(GuildEntity guildEntity, Guild guild)
    {
        foreach (var memberRank in guildEntity.Members.Select(x => x.Rank))
        {
            if (memberRank is null) continue;

            var level = (GuildRank)(memberRank.Level == 0 ? (int)GuildRank.Member : memberRank.Level);
            var guildLevel = new GuildLevel(level, memberRank.Name);

            guild.GuildLevels?.Add((ushort)memberRank.Id, guildLevel);
        }
    }
}