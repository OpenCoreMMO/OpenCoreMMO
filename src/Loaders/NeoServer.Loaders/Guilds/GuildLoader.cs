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

public class GuildLoader : ICustomLoader
{
    private readonly ChatChannelFactory _chatChannelFactory;
    private readonly IGuildStore _guildStore;
    private readonly ILogger _logger;

    public GuildLoader(ILogger logger, ChatChannelFactory chatChannelFactory, IGuildStore guildStore)
    {
        _logger = logger;
        _chatChannelFactory = chatChannelFactory;
        _guildStore = guildStore;
    }

    public async Task<Guild> LoadAsync(GuildEntity guildEntity)
    {
        if (guildEntity is null) return null;

        var guild = await GetOrCreateGuildAsync(guildEntity);
        if (guild == null) return null;

        guild.Name = guildEntity.Name;
        guild.Motd = guildEntity.Modt ?? string.Empty;
        guild.OwnerId = (ushort)guildEntity.OwnerId;
        guild.CreationDate = guildEntity.CreatedAt;
        guild.MemberCount = (uint)(guildEntity.Members?.Count ?? 0);

        // Load ranks
        if (guildEntity.Ranks?.Any() == true)
        {
            foreach (var rank in guildEntity.Ranks)
            {
                guild.AddRank((ushort)rank.Id, rank.Name, (byte)rank.Level);
            }
        }

        _guildStore.AddOrUpdate(guild.Id, guild);
        _logger.Debug("Guild {Guild} loaded with {MemberCount} members", guildEntity.Name, guild.MemberCount);
        
        return guild;
    }

    public Guild Load(GuildEntity guildEntity)
    {
        // Synchronous wrapper for backward compatibility
        return LoadAsync(guildEntity).GetAwaiter().GetResult();
    }

    private async Task<Guild> GetOrCreateGuildAsync(GuildEntity guildEntity)
    {
        var existingGuild = _guildStore.Get((ushort)guildEntity.Id);
        if (existingGuild != null) return existingGuild;

        var guild = new Guild
        {
            Id = (ushort)guildEntity.Id,
            Channel = _chatChannelFactory.CreateGuildChannel($"{guildEntity.Name ?? "Unknown Guild"}'s Channel",
                (ushort)guildEntity.Id),
            Bank = new Bank(guildEntity.BankAmount),
            GuildLevels = new Dictionary<ushort, GuildLevel>()
        };

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