# Guild System - OpenCoreMMO

This document describes the guild commands and features implemented in OpenCoreMMO, based on the ForgottenServer system.

## Implemented Commands

### 1. Guild Creation

**Command:** `!createguild "Guild Name"` or `!foundguild "Guild Name"`

**Description:** Creates a new guild with the player as leader.

**Requirements:**
- Minimum level: 20
- Cannot be in a guild
- Guild name must be between 3 and 50 characters
- Guild name must be unique

**Example:**
```
!createguild "Knights of Honor"
!foundguild "Dark Brotherhood"
```

### 2. Guild Invitation

**Command:** `!guildinvite "Player Name"` or `!invite "Player Name"`

**Description:** Invites a player to the guild.

**Requirements:**
- Must be in a guild
- Must be leader (rank 1) or vice-leader (rank 2)

**Status:** ✅ Implemented (integration with player system)

**Example:**
```
!guildinvite "PlayerName"
!invite "PlayerName"
```

### 3. Leave Guild

**Command:** `!guildleave` or `!leave`

**Description:** Removes the player from current guild.

**Requirements:**
- Must be in a guild
- Cannot be the guild leader (must transfer leadership first)

**Status:** ✅ Implemented with proper validation

**Example:**
```
!guildleave
!leave
```

### 4. Transfer Guild Leadership

**Command:** `!passleadership "Player Name"` or `!guildpassleadership "Player Name"`

**Description:** Transfers guild leadership to another member.

**Requirements:**
- Must be in a guild
- Must be the guild leader (rank 1)
- Target player must be in the same guild
- Target player cannot be the current leader

**Status:** ✅ Newly Implemented

**Example:**
```
!passleadership "NewLeader"
!guildpassleadership "TrustedMember"
```

### 5. Kick Guild Member

**Command:** `!guildkick "Player Name"` or `!kick "Player Name"`

**Description:** Removes a member from the guild.

**Requirements:**
- Must be in a guild
- Must be leader (rank 1) or vice-leader (rank 2)
- Cannot kick players with equal or higher rank
- Cannot kick yourself

**Status:** ✅ Fixed - Members are now properly removed from guild on next login

**Example:**
```
!guildkick "PlayerName"
!kick "UnwantedMember"
```

### 6. Change Guild MOTD

**Command:** `!guildmotd "New message"` or `!motd "New message"`

**Description:** Changes the guild's message of the day (MOTD).

**Requirements:**
- Must be in a guild
- Must be leader (rank 1) or vice-leader (rank 2)
- Message cannot exceed 255 characters

**Status:** ✅ Implemented with proper display on guild channel join

**Example:**
```
!guildmotd "Welcome to our guild! Be respectful and have fun!"
!motd "Guild meeting tomorrow at 8 PM"
!motd ""  // To clear the MOTD
```

### 7. Declare Guild War

**Command:** `!guildwar "Enemy Guild Name"` or `!war "Enemy Guild Name"`

**Description:** Declares war against another guild.

**Requirements:**
- Must be in a guild
- Must be leader (rank 1)
- Enemy guild must exist
- Cannot declare war against own guild

**Example:**
```
!guildwar "Enemy Guild"
!war "Rival Knights"
```

### 8. End Guild War

**Command:** `!endwar "Enemy Guild Name"` or `!peace "Enemy Guild Name"`

**Description:** Ends a war between guilds.

**Requirements:**
- Must be in a guild
- Must be leader (rank 1)
- Must have an active war with the specified guild

**Example:**
```
!endwar "Enemy Guild"
!peace "Former Enemies"
```

### 9. Guild Information

**Command:** `!guildinfo` or `!ginfo`

**Description:** Shows information about the current guild.

**Requirements:**
- Must be in a guild

**Example:**
```
!guildinfo
!ginfo
```

**Output:**
```
Guild: Knights of Honor
Members: 15
Created: 2025-08-07
MOTD: Welcome to our guild!
```

### 10. Guild Members

**Command:** `!guildmembers` or `!gmembers`

**Description:** Lists guild members.

**Requirements:**
- Must be in a guild

**Status:** Not implemented (awaiting member listing system)

**Example:**
```
!guildmembers
!gmembers
```

## Rank System

The system implements standard ranks compatible with ForgottenServer:

1. **Leader** - Rank 1
   - Can create and dissolve the guild
   - Can declare and end wars
   - Can invite and kick members
   - Can change MOTD
   - Can manage ranks
   - Can transfer leadership

2. **Vice-Leader** - Rank 2
   - Can invite members
   - Can kick lower-ranked members
   - Can change MOTD
   - Cannot declare wars
   - Cannot transfer leadership

3. **Member** - Rank 3
   - Regular guild member
   - Access only to basic functionalities

## Chat Channel System

### Administrator Access
- **Administrators** have universal access to all chat channels
- **Guild channels** are automatically accessible to administrators
- **Special staff colors** in chat:
  - Administrators: Red text
  - Community Managers: Red text
  - Game Masters: Red text
  - Senior Tutors: Red text (in help/chat channels only)
  - Junior Tutors: Orange text (in help/chat channels only)

### Channel Configuration
- **Help Channel**: Available for tutor support
- **Language Channels**: English Chat, World Chat, Polish Chat, Portuguese Chat, Spanish Chat
- **Guild Channels**: Automatically created for each guild with MOTD display

## Technical Integration

### Architecture

- **GuildCommandManager**: Manages all guild commands
- **PlayerSayCommand**: Intercepts messages and processes commands
- **GuildService**: Backend services for guild operations
- **ChatChannelFactory**: Handles guild channel creation and management
- **PlayerLoader**: Ensures guild membership sync on login
- **Dependency Injection**: Configured in `CommandInjection.cs`

### Command Processing

1. Player types command starting with `!`
2. `PlayerSayCommand` intercepts the message
3. `GuildCommandManager.ProcessGuildCommand()` is called
4. Appropriate command is executed
5. Response is sent to player

### Database

The system uses the following entities:
- `GuildEntity`: Basic guild information
- `GuildRankEntity`: Guild ranks
- `GuildMembershipEntity`: Guild members
- `GuildWarEntity`: Wars between guilds
- `GuildWarKillEntity`: Kills in wars
- `GuildInviteEntity`: Pending invites

### Key Fixes and Improvements

1. **Guild Kick Fix**: Members are now properly removed from guild membership on next login
2. **Leadership Transfer**: Implemented complete leadership transfer with rank management
3. **MOTD Display**: Guild MOTD is now displayed when joining guild channels
4. **Admin Channel Access**: Administrators can access all channels including guild channels
5. **Staff Color Coding**: Visual identification of staff members in chat channels

## Usage Examples

### Creating a Guild
```
Player: !createguild "Knights of Valor"
System: You have successfully created the guild 'Knights of Valor'!
```

### Transferring Leadership
```
Leader: !passleadership "TrustedMember"
System: You have successfully transferred leadership of the guild to TrustedMember!
```

### Declaring War
```
Leader: !guildwar "Enemy Forces"
System: War has been declared against Enemy Forces!
```

### Changing MOTD
```
Leader: !motd "Training session tonight at castle!"
System: Guild MOTD has been changed to: Training session tonight at castle!
```

### Kicking a Member
```
Leader: !guildkick "ProblematicMember"
System: ProblematicMember has been kicked from the guild!
```

## Implementation Notes

- All commands are **case-insensitive**
- Guild and player names must be quoted if they contain spaces
- The system is **fully asynchronous** for better performance
- Detailed logs are maintained for auditing
- Compatible with ForgottenServer guild system
- **Real-time updates**: Guild membership changes take effect on next login
- **Staff integration**: Administrative staff have enhanced channel access and visual identification

## Recent Implementations

### ✅ Completed Features

- **Guild Kick Fix**: Fixed issue where kicked members remained in guild
- **Leadership Transfer**: Complete implementation with rank management
- **MOTD Display**: Automatic display when joining guild channels
- **Administrator Channel Access**: Universal access to all channels including guild channels
- **Staff Color System**: Visual identification of staff in chat
- **Guild Channel Integration**: Proper integration with chat channel system
- **Database Synchronization**: Improved sync between guild membership and player login

### 🔄 Next Implementations

- Guild member listing system
- Custom rank permissions system
- Web interface for guild management
- War history and statistics
- Advanced guild alliance system
