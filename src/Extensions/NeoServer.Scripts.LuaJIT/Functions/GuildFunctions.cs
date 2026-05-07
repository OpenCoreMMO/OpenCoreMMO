using LuaNET;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Chat.Factory;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Creatures.Common;
using NeoServer.Domain.Guild;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GuildFunctions : LuaScriptInterface, IGuildFunctions
{
    private static ILogger _logger;

    public GuildFunctions(ILogger logger) : base(nameof(GuildFunctions))
    {
        _logger = logger;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Guild", "", LuaGuildCreate);
        RegisterMetaMethod(luaState, "Guild", "__eq", LuaUserdataCompare<Guild>);

        RegisterMethod(luaState, "Guild", "getId", LuaGuildGetId);
        RegisterMethod(luaState, "Guild", "getName", LuaGuildGetName);
        RegisterMethod(luaState, "Guild", "setName", LuaGuildSetName);
        RegisterMethod(luaState, "Guild", "getMotd", LuaGuildGetMotd);
        RegisterMethod(luaState, "Guild", "setMotd", LuaGuildSetMotd);
        RegisterMethod(luaState, "Guild", "getMemberCount", LuaGuildGetMemberCount);
        RegisterMethod(luaState, "Guild", "getMemberCountOnline", LuaGuildGetMemberCountOnline);
        RegisterMethod(luaState, "Guild", "getMembersOnline", LuaGuildGetMembersOnline);
        RegisterMethod(luaState, "Guild", "getMembers", LuaGuildGetMembers);
        RegisterMethod(luaState, "Guild", "addMember", LuaGuildAddMember);
        RegisterMethod(luaState, "Guild", "removeMember", LuaGuildRemoveMember);
        RegisterMethod(luaState, "Guild", "hasBank", LuaGuildHasBank);
        RegisterMethod(luaState, "Guild", "getBankAmount", LuaGuildGetBankAmount);
        RegisterMethod(luaState, "Guild", "setLeader", LuaGuildSetLeader);
        RegisterMethod(luaState, "Guild", "invitePlayer", LuaGuildInvitePlayer);
        RegisterMethod(luaState, "Guild", "hasInvitation", LuaGuildHasInvitation);
        RegisterMethod(luaState, "Guild", "revokeInvitation", LuaGuildRevokeInvitation);
        RegisterMethod(luaState, "Guild", "promoteMember", LuaGuildPromoteMember);
        RegisterMethod(luaState, "Guild", "demoteMember", LuaGuildDemoteMember);
        RegisterMethod(luaState, "Guild", "disband", LuaGuildDisband);

        // Register global function to check if guild exists
        RegisterGlobalMethod(luaState, "GuildExists", LuaGuildExists);
    }

    private static int LuaGuildCreate(LuaState luaState)
    {
        // Guild(name) - Create new guild with name
        var name = GetString(luaState, 2);
        if (string.IsNullOrEmpty(name))
        {
            _logger?.Warning("Guild creation failed: Empty name provided");
            Lua.PushNil(luaState);
            return 1;
        }

        try
        {
            // Get required services
            var guildRepository = Server.Helpers.IoC.GetInstance<IGuildRepository>();
            var guildStore = Server.Helpers.IoC.GetInstance<IGuildStore>();

            // Check if guild with this name already exists
            var existingGuild = guildRepository.GetByName(name);
            if (existingGuild != null)
            {
                _logger?.Warning(
                    "Guild creation failed: Guild with name '{GuildName}' already exists (ID: {ExistingGuildId})", name,
                    existingGuild.Id);
                Lua.PushNil(luaState);
                return 1;
            }

            // Create new guild entity for database
            var guildEntity = new GuildEntity
            {
                Name = name,
                CreatedAt = DateTime.UtcNow,
                Modt = $"Welcome to {name}!"
            };

            // Save to database
            guildRepository.Insert(guildEntity);
            _logger?.Information("Guild created successfully: '{GuildName}' with ID {GuildId}", name, guildEntity.Id);

            // Create default guild ranks in database
            var dbContext = Server.Helpers.IoC.GetInstance<NeoContext>();

            var memberRank = new GuildRankEntity
            {
                GuildId = guildEntity.Id,
                Name = "Member",
                Level = 1
            };

            var viceLeaderRank = new GuildRankEntity
            {
                GuildId = guildEntity.Id,
                Name = "Vice-Leader",
                Level = 2
            };

            var leaderRank = new GuildRankEntity
            {
                GuildId = guildEntity.Id,
                Name = "Leader",
                Level = 3
            };

            dbContext.GuildRanks.AddRange(memberRank, viceLeaderRank, leaderRank);
            dbContext.SaveChanges();

            // Refresh entities to get auto-generated IDs
            dbContext.Entry(memberRank).Reload();
            dbContext.Entry(viceLeaderRank).Reload();
            dbContext.Entry(leaderRank).Reload();

            _logger?.Information(
                "Default guild ranks created for guild '{GuildName}' - Member: {MemberId}, Vice: {ViceId}, Leader: {LeaderId}",
                name, memberRank.Id, viceLeaderRank.Id, leaderRank.Id);

            // Get chat channel factory to create guild channel
            var chatChannelFactory = Server.Helpers.IoC.GetInstance<ChatChannelFactory>();

            // Create guild domain object
            var guild = new Guild
            {
                Id = (ushort)guildEntity.Id,
                Name = name,
                CreatedDate = DateTime.UtcNow,
                Motd = $"Welcome to {name}!",
                Bank = new Bank(0) // Initialize required Bank property with 0 amount
            };

            // Add default guild ranks using the actual database IDs
            guild.AddRank((ushort)memberRank.Id, "Member", 1); // Member rank (level 1)
            guild.AddRank((ushort)viceLeaderRank.Id, "Vice-Leader", 2); // Vice-Leader rank (level 2)
            guild.AddRank((ushort)leaderRank.Id, "Leader", 3); // Leader rank (level 3)

            // Add to guild store for runtime access first
            guildStore.AddOrUpdate(guild.Id, guild);
            _logger?.Information("Guild '{GuildName}' added to runtime store with ID {GuildId}", name, guild.Id);

            // Create guild channel automatically with guild object
            guild.Channel = chatChannelFactory.CreateGuildChannel($"{name}'s Channel", guild);
            _logger?.Information("Guild channel created for '{GuildName}' with channel ID {ChannelId}", name,
                guild.Channel.Id);

            PushUserdata(luaState, guild);
            SetMetatable(luaState, -1, "Guild");
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to create guild: {GuildName}", name);
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaGuildGetId(LuaState luaState)
    {
        // guild:getId()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        Lua.PushNumber(luaState, guild.Id);
        return 1;
    }

    private static int LuaGuildGetName(LuaState luaState)
    {
        // guild:getName()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        PushString(luaState, guild.Name);
        return 1;
    }

    private static int LuaGuildSetName(LuaState luaState)
    {
        // guild:setName(name)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var name = GetString(luaState, 2);
        guild.Name = name;
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaGuildGetMotd(LuaState luaState)
    {
        // guild:getMotd()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        PushString(luaState, guild.Motd ?? "");
        return 1;
    }

    private static int LuaGuildSetMotd(LuaState luaState)
    {
        // guild:setMotd(motd)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var motd = GetString(luaState, 2);
        guild.Motd = motd;
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaGuildGetMemberCount(LuaState luaState)
    {
        // guild:getMemberCount()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        Lua.PushNumber(luaState, guild.MemberCount);
        return 1;
    }

    private static int LuaGuildGetMemberCountOnline(LuaState luaState)
    {
        // guild:getMemberCountOnline()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        Lua.PushNumber(luaState, guild.MembersOnline.Count);
        return 1;
    }

    private static int LuaGuildGetMembersOnline(LuaState luaState)
    {
        // guild:getMembersOnline()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.CreateTable(luaState, 0, 0);
            return 1;
        }

        Lua.CreateTable(luaState, guild.MembersOnline.Count, 0);
        var index = 1;
        foreach (var member in guild.MembersOnline)
        {
            Lua.PushNumber(luaState, index++);
            PushUserdata(luaState, member);
            SetMetatable(luaState, -1, "Player");
            Lua.SetTable(luaState, -3);
        }

        return 1;
    }

    private static int LuaGuildAddMember(LuaState luaState)
    {
        // guild:addMember(player)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(luaState, 2);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        guild.AddMember(player);
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaGuildRemoveMember(LuaState luaState)
    {
        // guild:removeMember(player)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(luaState, 2);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        guild.RemoveMember(player);
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaGuildHasBank(LuaState luaState)
    {
        // guild:hasBank()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        Lua.PushBoolean(luaState, guild.Bank != null);
        return 1;
    }

    private static int LuaGuildGetBankAmount(LuaState luaState)
    {
        // guild:getBankAmount()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        Lua.PushNumber(luaState, guild.BankAmount);
        return 1;
    }

    private static int LuaGuildGetMembers(LuaState luaState)
    {
        // guild:getMembers()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.CreateTable(luaState, 0, 0);
            return 1;
        }

        var members = guild.Members ?? new List<IPlayer>();
        Lua.CreateTable(luaState, members.Count, 0);

        for (var i = 0; i < members.Count; i++)
        {
            PushUserdata(luaState, members[i]);
            Lua.RawSetI(luaState, -2, i + 1);
        }

        return 1;
    }

    private static int LuaGuildSetLeader(LuaState luaState)
    {
        // guild:setLeader(player)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(luaState, 2);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        guild.SetLeader(player);
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaGuildInvitePlayer(LuaState luaState)
    {
        // guild:invitePlayer(player)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(luaState, 2);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var result = guild.InvitePlayer(player);
        Lua.PushBoolean(luaState, result);
        return 1;
    }

    private static int LuaGuildHasInvitation(LuaState luaState)
    {
        // guild:hasInvitation(player)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(luaState, 2);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var result = guild.HasInvitation(player);
        Lua.PushBoolean(luaState, result);
        return 1;
    }

    private static int LuaGuildRevokeInvitation(LuaState luaState)
    {
        // guild:revokeInvitation(player)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(luaState, 2);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var result = guild.RevokeInvitation(player);
        Lua.PushBoolean(luaState, result);
        return 1;
    }

    private static int LuaGuildPromoteMember(LuaState luaState)
    {
        // guild:promoteMember(player, newLevel)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(luaState, 2);
        var newLevel = GetNumber<int>(luaState, 3);

        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var result = guild.PromoteMember(player, newLevel);
        Lua.PushBoolean(luaState, result);
        return 1;
    }

    private static int LuaGuildDemoteMember(LuaState luaState)
    {
        // guild:demoteMember(player, newLevel)
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(luaState, 2);
        var newLevel = GetNumber<int>(luaState, 3);

        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var result = guild.DemoteMember(player, newLevel);
        Lua.PushBoolean(luaState, result);
        return 1;
    }

    private static int LuaGuildDisband(LuaState luaState)
    {
        // guild:disband()
        var guild = GetUserdata<Guild>(luaState, 1);
        if (guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var result = guild.Disband();
        Lua.PushBoolean(luaState, result);
        return 1;
    }

    private static int LuaGuildExists(LuaState luaState)
    {
        // Guild.exists(name)
        var name = GetString(luaState, 1);
        if (string.IsNullOrEmpty(name))
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        try
        {
            var guildRepository = Server.Helpers.IoC.GetInstance<IGuildRepository>();
            var existingGuild = guildRepository.GetByName(name);

            Lua.PushBoolean(luaState, existingGuild != null);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error checking if guild exists with name '{GuildName}'", name);
            Lua.PushBoolean(luaState, false);
        }

        return 1;
    }
}