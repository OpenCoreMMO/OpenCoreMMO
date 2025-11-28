-- Guild system configuration
GuildConfig = {
    -- Costs and requirements
    CREATION_COST = 10000, -- Gold required to create a guild
    MIN_LEVEL_CREATE = 20, -- Minimum level to create a guild
    MIN_LEVEL_JOIN = 8, -- Minimum level to join a guild

    -- Limits
    MAX_GUILD_NAME_LENGTH = 29,
    MIN_GUILD_NAME_LENGTH = 4,
    MAX_MOTD_LENGTH = 200,
    MAX_GUILD_NICK_LENGTH = 20,
    MIN_GUILD_NICK_LENGTH = 2,

    -- Guild levels
    GUILD_LEVEL_MEMBER = 1,
    GUILD_LEVEL_VICE_LEADER = 2,
    GUILD_LEVEL_LEADER = 3,

    -- Invitation system
    INVITATION_EXPIRE_TIME = 7 * 24 * 60 * 60, -- 7 days in seconds

    -- Validation patterns
    NAME_PATTERN = "^[%w%s]+$", -- Only alphanumeric and spaces
    NICK_PATTERN = "^[%w%s]+$", -- Only alphanumeric and spaces
}

-- Helper functions for guild validation
function isValidGuildName(name)
    if not name or name == "" then
        return false, "Guild name cannot be empty."
    end

    if #name < GuildConfig.MIN_GUILD_NAME_LENGTH then
        return false, string.format("Guild name must be at least %d characters.", GuildConfig.MIN_GUILD_NAME_LENGTH)
    end

    if #name > GuildConfig.MAX_GUILD_NAME_LENGTH then
        return false, string.format("Guild name cannot exceed %d characters.", GuildConfig.MAX_GUILD_NAME_LENGTH)
    end

    if not name:match(GuildConfig.NAME_PATTERN) then
        return false, "Guild name contains invalid characters. Only letters, numbers and spaces are allowed."
    end

    return true, "Valid guild name."
end

function isValidGuildNick(nick)
    if not nick then
        return false, "Guild nickname cannot be nil."
    end

    -- Empty nick is valid (removes nickname)
    if nick == "" then
        return true, "Valid guild nickname."
    end

    if #nick < GuildConfig.MIN_GUILD_NICK_LENGTH then
        return false, string.format("Guild nickname must be at least %d characters.", GuildConfig.MIN_GUILD_NICK_LENGTH)
    end

    if #nick > GuildConfig.MAX_GUILD_NICK_LENGTH then
        return false, string.format("Guild nickname cannot exceed %d characters.", GuildConfig.MAX_GUILD_NICK_LENGTH)
    end

    if not nick:match(GuildConfig.NICK_PATTERN) then
        return false, "Guild nickname contains invalid characters. Only letters, numbers and spaces are allowed."
    end

    return true, "Valid guild nickname."
end

function isValidMotd(motd)
    if not motd then
        return false, "Message of the day cannot be nil."
    end

    -- Empty motd is valid (removes motd)
    if motd == "" then
        return true, "Valid message of the day."
    end

    if #motd > GuildConfig.MAX_MOTD_LENGTH then
        return false, string.format("Message of the day cannot exceed %d characters.", GuildConfig.MAX_MOTD_LENGTH)
    end

    return true, "Valid message of the day."
end

function getGuildLevelName(level)
    if level == GuildConfig.GUILD_LEVEL_LEADER then
        return "Leader"
    elseif level == GuildConfig.GUILD_LEVEL_VICE_LEADER then
        return "Vice-Leader"
    elseif level == GuildConfig.GUILD_LEVEL_MEMBER then
        return "Member"
    else
        return "Unknown"
    end
end

function canManageGuild(guildLevel)
    return guildLevel >= GuildConfig.GUILD_LEVEL_VICE_LEADER
end

function canPromoteMembers(guildLevel)
    return guildLevel == GuildConfig.GUILD_LEVEL_LEADER
end

function canDisbandGuild(guildLevel)
    return guildLevel == GuildConfig.GUILD_LEVEL_LEADER
end
