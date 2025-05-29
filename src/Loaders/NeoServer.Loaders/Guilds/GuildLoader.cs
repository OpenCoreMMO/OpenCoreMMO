using System.Collections.Generic;
using System.Linq;
using NeoServer.Data.Entities;
using NeoServer.Game.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Creatures.Guilds;
using NeoServer.Game.Creatures.Common;
using NeoServer.Game.Creatures.Guild;
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

    public IGuild Load(GuildEntity guildEntity)
    {
        if (guildEntity is null) return null;

        var guild = GetOrCreateGuild(guildEntity, out var shouldAddToStore);

        guild.Name = guildEntity.Name;
        guild.GuildLevels?.Clear();

        if ((guildEntity.Ranks?.Count ?? 0) > 0)
            guild.GuildLevels = new Dictionary<ushort, IGuildLevel>();

        AddMembers(guildEntity, guild);

        if (shouldAddToStore)
        {
            _guildStore.AddOrUpdate(guild.Id, guild);
            return guild;
        }

        _logger.Debug("Guild {Guild} loaded", guildEntity.Name);
        return guild;
    }

    private static void AddMembers(GuildEntity guildEntity, IGuild guild)
    {
        foreach (var memberRank in guildEntity.Members.Select(x => x.Rank))
        {
            if (memberRank is null) continue;

            var level = (GuildRank)(memberRank.Level == 0 ? (int)GuildRank.Member : memberRank.Level);
            var guildLevel = new GuildLevel(level, memberRank.Name);

            guild.GuildLevels?.Add((ushort)memberRank.Id, guildLevel);
        }
    }

    private IGuild GetOrCreateGuild(GuildEntity guildEntity, out bool shouldAddToStore)
    {
        var guild = _guildStore.Get((ushort)guildEntity.Id);

        shouldAddToStore = false;

        if (guild is not null) return guild;

        shouldAddToStore = true;

        guild = new Guild
        {
            Id = (ushort)guildEntity.Id,
            Channel = _chatChannelFactory.CreateGuildChannel($"{guildEntity.Name}'s Channel",
                (ushort)guildEntity.Id),
            Bank = new Bank(guildEntity.BankAmount)
        };

        return guild;
    }
}