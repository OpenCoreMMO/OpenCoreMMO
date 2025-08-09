# Chat System and Staff Management - OpenCoreMMO

This document describes the chat channel system and staff management features implemented in OpenCoreMMO.

## Chat Channel System

### Available Channels

#### Public Channels
- **Help Channel** (ID: 1) - General help and support
- **English Chat** (ID: 7) - English language chat
- **World Chat** (ID: 8) - Global world chat

#### Language-Specific Channels
- **Polish Chat** (ID: 10) - Polish language chat
- **Portuguese Chat** (ID: 11) - Portuguese language chat  
- **Spanish Chat** (ID: 12) - Spanish language chat

#### Guild Channels
- **Automatically created** for each guild
- **MOTD display** when joining the channel
- **Private communication** for guild members

### Channel Access System

#### Administrator Access
Administrators (Group ID 6) have **universal access** to all chat channels:
- Can join any public channel regardless of restrictions
- Can access all guild channels for moderation purposes
- Bypass level, vocation, and other access requirements
- Automatic access granted through `ChatChannel.Validate()` method

#### Regular Player Access
Regular players are subject to channel-specific restrictions:
- Level requirements
- Vocation restrictions
- Channel-specific rules

## Staff Color System

### Visual Staff Identification

The system implements a comprehensive color coding system for staff members in chat channels:

#### Staff Hierarchy and Colors

1. **Administrator** (Group ID 6 - "god")
   - **Color**: Red text (`SpeechType.ChannelRed1`)
   - **Access**: Universal channel access
   - **Permissions**: All administrative functions

2. **Community Manager** (Group ID 5)
   - **Color**: Red text (`SpeechType.ChannelRed1`)
   - **Access**: Enhanced channel access
   - **Permissions**: Community management functions

3. **Game Master** (Group ID 4)
   - **Color**: Red text (`SpeechType.ChannelRed1`)
   - **Access**: Game management functions
   - **Permissions**: Player moderation and game management

4. **Senior Tutor** (Group ID 3)
   - **Color**: Red text (`SpeechType.ChannelRed1`) in specific channels only
   - **Channels**: Help, English Chat, World Chat, Polish Chat, Portuguese Chat, Spanish Chat
   - **Purpose**: Provide advanced support in help channels

5. **Junior Tutor** (Group ID 2)
   - **Color**: Orange text (`SpeechType.ChannelOrange`) in specific channels only
   - **Channels**: Help, English Chat, World Chat, Polish Chat, Portuguese Chat, Spanish Chat
   - **Purpose**: Provide basic support in help channels

### Color Implementation

```csharp
public virtual SpeechType GetTextColor(IPlayer player)
{
    if (player?.Group != null)
    {
        switch (player.Group.Id)
        {
            case 6: // Administrator - Red
                return SpeechType.ChannelRed1;
            case 5: // Community Manager - Red
                return SpeechType.ChannelRed1;
            case 4: // GameMaster - Red
                return SpeechType.ChannelRed1;
            case 3: // Senior Tutor - Red (channel-specific)
                if (IsTutorChannel())
                    return SpeechType.ChannelRed1;
                break;
            case 2: // Junior Tutor - Orange (channel-specific)
                if (IsTutorChannel())
                    return SpeechType.ChannelOrange;
                break;
        }
    }
    
    // Default color system for regular players
    return ChatColor;
}
```

## Guild Channel Integration

### Automatic Channel Creation

Guild channels are automatically created when a guild is formed:
- **Channel ID**: Based on guild ID
- **Channel Name**: Guild name
- **Access**: Limited to guild members and administrators
- **MOTD Display**: Shows guild MOTD when joining

### Guild Channel Features

#### MOTD Display
When a player joins a guild channel, the system displays:
```
[Guild MOTD] Welcome to our guild! Be respectful and have fun!
```

#### Administrator Access
Administrators can access any guild channel for moderation purposes:
```csharp
public override bool AddUser(IPlayer player)
{
    // Administrators can join any guild channel
    if (player.Group?.Access == true)
    {
        return users.TryAdd(player.Id, new UserChat { Player = player });
    }
    
    // Regular guild member validation
    return base.AddUser(player);
}
```

## Technical Implementation

### Chat Channel Factory

The `ChatChannelFactory` manages channel creation and staff access:

```csharp
public ChatChannel CreateGuildChannel(IGuild guild)
{
    var channel = new GuildChatChannel(guild);
    
    // Add to channel store for administrator discovery
    ChatChannelStore.Add(channel.Id, channel);
    
    return channel;
}
```

### Staff Database Seeding

Staff characters are automatically seeded in the database:

```csharp
// Administrator (Level 1000, Group 6)
CreatePlayerEntity(6, "Administrator", 6, 1000, male: true),

// Community Manager (Level 600, Group 5)  
CreatePlayerEntity(7, "CommunityManager", 5, 600, male: true),

// Game Master (Level 800, Group 4)
CreatePlayerEntity(8, "GameMaster", 4, 800, male: false),

// Senior Tutor (Level 350, Group 3)
CreatePlayerEntity(9, "SeniorTutor", 3, 350, male: true),

// Junior Tutor (Level 200, Group 2)
CreatePlayerEntity(10, "JuniorTutor", 2, 200, male: false)
```

## Configuration

### Groups Configuration (`groups.json`)

```json
{
  "2": {
    "id": 2,
    "name": "tutor",
    "access": false,
    "maxVipDays": 0,
    "maxDepotItems": 2000,
    "maxVipList": 200
  },
  "3": {
    "id": 3, 
    "name": "senior tutor",
    "access": false,
    "maxVipDays": 0,
    "maxDepotItems": 2000,
    "maxVipList": 200
  },
  "4": {
    "id": 4,
    "name": "gamemaster", 
    "access": true,
    "maxVipDays": 0,
    "maxDepotItems": 2000,
    "maxVipList": 200
  },
  "5": {
    "id": 5,
    "name": "community manager",
    "access": true, 
    "maxVipDays": 0,
    "maxDepotItems": 2000,
    "maxVipList": 200
  },
  "6": {
    "id": 6,
    "name": "god",
    "access": true,
    "maxVipDays": 0, 
    "maxDepotItems": 2000,
    "maxVipList": 200
  }
}
```

### Channels Configuration (`channels.json`)

```json
{
  "10": {
    "id": 10,
    "name": "Polish Chat",
    "description": "Polish language chat channel"
  },
  "11": {
    "id": 11, 
    "name": "Portuguese Chat",
    "description": "Portuguese language chat channel"
  },
  "12": {
    "id": 12,
    "name": "Spanish Chat", 
    "description": "Spanish language chat channel"
  }
}
```

## Usage Examples

### Staff Access to Channels
```
Administrator joins Polish Chat
[System] Administrator has joined the channel
Administrator: Hello everyone! 
// Message appears in red text
```

### Tutor Support
```
Junior Tutor joins Help channel
JuniorTutor: How can I help you today?
// Message appears in orange text

Senior Tutor joins Help channel  
SeniorTutor: I can assist with advanced questions
// Message appears in red text
```

### Guild Channel with MOTD
```
Player joins guild channel "Knights of Honor"
[Guild MOTD] Welcome to our guild! Training tonight at 8 PM!
Player: Ready for training!
// Message appears in default color
```

## Security and Moderation

### Administrator Powers
- **Universal channel access** for moderation
- **Visual identification** through red text color
- **Bypass all restrictions** for emergency situations
- **Guild channel monitoring** capabilities

### Audit Trail
- All staff actions are logged
- Channel access is tracked
- Color assignments are validated
- Group permissions are enforced

## Compatibility

### OTClient Integration
- **SpeechType enum** compatibility with OTClient
- **Color codes** tested with standard clients
- **Message types** follow Tibia protocol standards
- **Channel IDs** maintain compatibility with existing systems

### Protocol Support
- Full support for standard Tibia chat protocol
- Compatible with ForgottenServer channel system
- Maintains backward compatibility with existing clients
