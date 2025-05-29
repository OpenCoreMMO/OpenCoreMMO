<h1 align="center">
  <img align="center" width="120px" src="https://github.com/caioavidal/OpenCoreMMO/blob/develop/ocmsquare.png?raw=true" target="_blank"  />
  <br>
  OPENCOREMMO</h1>
<p align="center">
  <a href="https://codecov.io/gh/caioavidal/OpenCoreMMO">
  <img align="center" src="https://codecov.io/gh/caioavidal/OpenCoreMMO/branch/develop/graph/badge.svg" />
</a>
<a href="https://www.codefactor.io/repository/github/opencoremmo/opencoremmo"><img src="https://www.codefactor.io/repository/github/opencoremmo/opencoremmo/badge" align="center" alt="CodeFactor" /></a><a href="https://discord.gg/Kazq9z2">
  <img align="center" src="https://badgen.net/badge/icon/discord?icon=discord&label" />
</a>
<a href="https://github.com/caioavidal/opencoremmo/stargazers">
  <img align="center" src="https://img.shields.io/github/stars/caioavidal/opencoremmo?label=stargazers&logoColor=yellow&style=social" />
  </a>
  <a href="https://github.com/caioavidal/OpenCoreMMO/blob/develop/LICENSE">
  <img align="center" src="https://badgen.net/github/license/caioavidal/opencoremmo" />
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

| Enviroment | Status |
|------------|--------|
|![win](https://badgen.net/badge/icon/Windows,.NET%209?icon=windows&label&list=1)|[![OpenCoreMMO](https://github.com/caioavidal/OpenCoreMMO/actions/workflows/opencoremmo-validation.yaml/badge.svg?event=push)](https://github.com/caioavidal/OpenCoreMMO/actions/workflows/opencoremmo-validation.yaml)        |
|![linux](https://badgen.net/badge/icon/Ubuntu%20Linux%2022.04%20x64?icon=terminal&label&color=orange)|[![OpenCoreMMO](https://github.com/caioavidal/OpenCoreMMO/actions/workflows/opencoremmo-validation.yaml/badge.svg?event=push)](https://github.com/caioavidal/OpenCoreMMO/actions/workflows/opencoremmo-validation.yaml)|
|![mac](https://badgen.net/badge/icon/macOS%20Latest?icon=apple&label&color=purple&list=1)|[![OpenCoreMMO](https://github.com/caioavidal/OpenCoreMMO/actions/workflows/opencoremmo-validation.yaml/badge.svg?event=push)](https://github.com/caioavidal/OpenCoreMMO/actions/workflows/opencoremmo-validation.yaml)|

## Usage

```sh
download and install .NET 9: https://dotnet.microsoft.com/download/dotnet/9.0
git clone https://github.com/caioavidal/OpenCoreMMO.git
cd src
dotnet run --project "Standalone"
```
To connect to the self-hosted server for development, please use the following connection details:
1. IP Address: 127.0.0.1
2. Port: 7171
3. Account Name: 1
4. Password: 1

## What we have done so far

- Reading OTB and OTBM File structure: :heavy_check_mark:
- Loading OTBM Tile Area, Towns and Waypoints: :heavy_check_mark:
- Loading Spawn Areas: :heavy_check_mark:
- Loading OTBM House Tiles: :warning:
- Log In/Out Player on Game: :heavy_check_mark:
- All player movements: :heavy_check_mark:
- Multiplayer connection: :heavy_check_mark:
- Spawn and Respawn Monsters: :heavy_check_mark:
- PvM Combat: :heavy_check_mark:
- Depot: :heavy_check_mark:
- Chats: :heavy_check_mark:
- Guilds: :heavy_check_mark:
- War System: :warning:
- Public Channels: :heavy_check_mark:
  - Loot and Death Channels: :heavy_check_mark:
  - Vip List: :heavy_check_mark:
- PvP Combat: :warning:
- Party: :heavy_check_mark:
  - Basics: :heavy_check_mark:
  - Share Loot: :heavy_check_mark:
  - Shared Experience: :heavy_check_mark:
- NPC System: :heavy_check_mark:
- Lua Scripting: :heavy_check_mark:
- C# Extensions: :heavy_check_mark:
- Revscript LuaJIT Scripting: :warning:
- In-Memory Cache: :warning:

## Technologies

* C#
* .Net 9
* Database support: InMemory, PostgreSQL, and SQLite
* Console Debug Logging
* XUnit Testing

 [![My Skills](https://skillicons.dev/icons?i=dotnet,cs,docker,git,postgresql,sqlite,lua)](https://skillicons.dev)

## Links

* Documentation: https://opencoremmo.gitbook.io/opencoremmo/
* Discord Invite: https://discord.gg/Kazq9z2
* Revscript (LuaJIT) Functions Readme: https://github.com/caioavidal/OpenCoreMMO/tree/develop/data#readme

## Author

👤 **Caio Vidal**

* Github: [@caioavidal](https://github.com/caioavidal)
* LinkedIn: [https:\/\/www.linkedin.com\/in\/caiovidal](https:\/\/www.linkedin.com\/in\/caiovidal)

## Show your support

Give a ⭐️ if this project helped you!
