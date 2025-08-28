<h1 align="center">
  <img align="center" width="120px" src="https://github.com/OpenCoreMMO/OpenCoreMMO/blob/develop/ocmsquare.png?raw=true" target="_blank"  />
  <br>
  OPENCOREMMO</h1>
<p align="center">
  <a href="https://codecov.io/gh/caioavidal/OpenCoreMMO">
  <img align="center" src="https://codecov.io/gh/caioavidal/OpenCoreMMO/branch/develop/graph/badge.svg" />
</a>
<a href="https://www.codefactor.io/repository/github/opencoremmo/opencoremmo"><img src="https://www.codefactor.io/repository/github/opencoremmo/opencoremmo/badge" align="center" alt="CodeFactor" /></a><a href="https://discord.gg/Kazq9z2">
  <img align="center" src="https://badgen.net/badge/icon/discord?icon=discord&label" />
</a>
<a href="https://github.com/OpenCoreMMO/opencoremmo/stargazers">
  <img align="center" src="https://img.shields.io/github/stars/OpenCoreMMO/opencoremmo?label=stargazers&logoColor=yellow&style=social" />
  </a>
  <a href="https://github.com/OpenCoreMMO/OpenCoreMMO/blob/develop/LICENSE">
  <img align="center" src="https://badgen.net/github/license/OpenCoreMMO/opencoremmo" />
  </a>
</p>

<p align="center">
  <img align="center" src="https://sonarcloud.io/api/project_badges/measure?project=caioavidal_OpenCoreMMO&metric=sqale_index" />
  <img align="center" src="https://sonarcloud.io/api/project_badges/measure?project=caioavidal_OpenCoreMMO&metric=sqale_rating" />
  <img align="center" src="https://sonarcloud.io/api/project_badges/measure?project=caioavidal_OpenCoreMMO&metric=ncloc" />
  <img align="center" src="https://sonarcloud.io/api/project_badges/measure?project=caioavidal_OpenCoreMMO&metric=code_smells" />
  <img align="center" src="https://sonarcloud.io/api/project_badges/measure?project=caioavidal_OpenCoreMMO&metric=security_rating" />
</p>

> Modern, free, and open-source MMORPG server emulator written in C#.
> 
> It was written from scratch and development on the project began in January 2020.
> <br>To connect to the server, you can use either [OTClient](https://github.com/edubart/otclient), [OTCv8](https://github.com/OTCv8/otclientv8) or [OTCR](https://github.com/mehah/otclient) for version 8.6.

## Latest Builds

| Status |
|--------|
|[![OpenCoreMMO](https://github.com/OpenCoreMMO/OpenCoreMMO/actions/workflows/opencoremmo-validation.yaml/badge.svg?event=push&branch=version/860)](https://github.com/OpenCoreMMO/OpenCoreMMO/actions/workflows/opencoremmo-validation.yaml)|

## Usage

```sh
download and install .NET 9: https://dotnet.microsoft.com/download/dotnet/9.0
git clone https://github.com/OpenCoreMMO/OpenCoreMMO.git
cd src
dotnet run --project "Standalone"
```
To connect to the self-hosted server for development, please use the following connection details:
1. IP Address: 127.0.0.1
2. Port: 7171
3. Account Name: 1
4. Password: 1

## What we have done so far

- **Bank System**: :warning:
- **C# Scripting**: :warning:
- **Chats**: :heavy_check_mark:
- **Combat**
  - PvM Combat: :heavy_check_mark:
  - PvP Combat: :warning:
  - Skull System: :warning:
- **Depot**: :heavy_check_mark:
- **Guilds**: :heavy_check_mark:
- **House System**: :warning:
- **Royal Mail System**: :heavy_check_mark:
- **Monsters**: :heavy_check_mark:
- **NPC System**: :heavy_check_mark:
- **Party**: :heavy_check_mark:
  - Basics: :heavy_check_mark:
  - Share Loot: :heavy_check_mark:
  - Shared Experience: :heavy_check_mark:
- **Private Chat Channels**: :heavy_check_mark:
- **Public Channels**: :heavy_check_mark:
  - Loot and Death Channels: :heavy_check_mark:
  - Vip List: :heavy_check_mark:
- **Quest System**: :heavy_check_mark:
- **Report System**: :heavy_check_mark:
- **Safe Trade**: :heavy_check_mark:
- **Stamina System**: :warning:
- **Summon**: :heavy_check_mark:
- **War System**: :warning:
- **Lua Scripting (Revscript)**: :arrows_counterclockwise:
  - Action: :arrows_counterclockwise:
  - Bank: :arrows_counterclockwise:
  - Combat: :arrows_counterclockwise:
  - Condition: :arrows_counterclockwise:
  - Config: :heavy_check_mark:
  - Container: :heavy_check_mark:
  - Creature: :arrows_counterclockwise:
  - Creature Event: :heavy_check_mark:
  - DB: :arrows_counterclockwise:
  - Event Callback: :warning:
  - Events Scheduler: :warning:
  - Game: :arrows_counterclockwise:
  - Global: :arrows_counterclockwise:
  - Global Event: :heavy_check_mark:
  - Group: :heavy_check_mark:
  - Guild: :warning:
  - House: :warning:
  - Item: :arrows_counterclockwise:
  - Item Type: :arrows_counterclockwise:
  - Logger: :heavy_check_mark:
  - Loot: :warning:
  - Monster: :arrows_counterclockwise:
  - Monster Spell: :warning:
  - Monster Type: :arrows_counterclockwise:
  - Move Event: :arrows_counterclockwise:
  - Network Message: :warning:
  - NPC: :arrows_counterclockwise:
  - NPC Type: :arrows_counterclockwise:
  - Party: :warning:
  - Player: :arrows_counterclockwise:
  - Position: :arrows_counterclockwise:
  - Raids: :warning:
  - Result: :heavy_check_mark:
  - Spell: :arrows_counterclockwise:
  - Talk Action: :arrows_counterclockwise:
  - Teleport: :arrows_counterclockwise:
  - Tile: :arrows_counterclockwise:
  - Town: :heavy_check_mark:
  - Variant: :heavy_check_mark:
  - Vocation: :arrows_counterclockwise:
  - Weapon: :arrows_counterclockwise:
  - Webhook: :arrows_counterclockwise:
- **Lua Scripting Auto Reload**: :heavy_check_mark:


## Technologies

* C#
* .Net 9
* Database support: InMemory, PostgreSQL and SQLite
* Console Debug Logging
* XUnit Testing
* Docker

 [![My Skills](https://skillicons.dev/icons?i=dotnet,cs,docker,git,postgresql,sqlite,lua)](https://skillicons.dev)

## Links

* Documentation: https://opencoremmo.gitbook.io/opencoremmo/
* Discord Invite: https://discord.gg/Kazq9z2
* Lua Scripting (Revscript) Functions Readme: https://github.com/OpenCoreMMO/OpenCoreMMO/tree/develop/data#readme

## Author

👤 **Caio Vidal**

* Github: [@caioavidal](https://github.com/caioavidal)
* LinkedIn: [https:\/\/www.linkedin.com\/in\/caiovidal](https:\/\/www.linkedin.com\/in\/caiovidal)

## Show your support

Give a ⭐️ if this project helped you!
