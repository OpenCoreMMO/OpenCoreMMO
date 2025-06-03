using LuaNET;
using NeoServer.Data.Interfaces;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Loaders.Guilds;
using NeoServer.Loaders.Interfaces;
using NeoServer.Scripts.LuaJIT.Functions;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaHelperService(
    IGuildStore guildStore,
    IGuildRepository guildRepository,
    ICreatureGameInstance creatureGameInstance,
    IPlayerRepository playerRepository,
    GuildLoader guildLoader,
    IPlayerLoader playerLoader,
    ILogger logger) : LuaScriptInterface(nameof(BankFunctionBinder))
{
    public IGuild GetGuild(LuaState lua, int arg, bool allowOffline = false)
    {
        if (IsUserdata(lua, arg))
        {
            return GetUserdata<IGuild>(lua, arg, "Guild");
        }

        if (IsNumber(lua, arg))
        {
            //checking in-memory first
            var guild = guildStore.Get(GetNumber<ushort>(lua, arg));
            if (guild is not null) return guild;

            if (allowOffline)
            {
                return LoadGuild(GetNumber<int>(lua, arg));
            }

            return null;
        }

        if (IsString(lua, arg))
        {
            var guildRecord = guildRepository.GetByName(GetString(lua, arg)).Result;

            if (guildRecord is null) return null;
            var guild = guildStore.Get((ushort)guildRecord.Id);

            if (guild is not null) return guild;

            if (allowOffline)
            {
                return LoadGuild(guildRecord.Id);
            }

            return null;
        }

        logger.Warning("Lua::{MethodName}: Invalid argument", "getGuild");
        return null;

        IGuild LoadGuild(int id)
        {
            var guildRecord = guildRepository.GetById(id).Result;
            if (guildRecord is null) return null;

            return guildLoader.Load(guildRecord);
        }
    }

    public IPlayer GetPlayer(LuaState lua, int arg, bool allowOffline = false)
    {
        if (IsUserdata(lua, arg))
        {
            return GetUserdata<IPlayer>(lua, arg, "Player");
        }

        if (IsNumber(lua, arg))
        {
            creatureGameInstance.TryGetPlayer(GetNumber<uint>(lua, arg), out var player);

            if (player is not null) return player;

            if (allowOffline)
            {
                return LoadPlayer(GetNumber<int>(lua, arg));
            }

            return null;
        }

        if (IsString(lua, arg))
        {
            var playerEntity = playerRepository.GetByName(GetString(lua, arg)).Result;

            if (playerEntity is null) return null;
            
            creatureGameInstance.TryGetPlayer(GetNumber<uint>(lua, arg), out var player);

            if (player is not null) return player;

            if (allowOffline)
            {
                return LoadPlayer(playerEntity.Id);
            }

            return null;
        }

        logger.Warning("Lua::{MethodName}: Invalid argument", "getGuild");
        return null;

        IPlayer LoadPlayer(int id)
        {
            var playerRecord = playerRepository.GetById(id).Result;
            if(playerRecord is null) return null;

            return playerLoader.Load(playerRecord);
        }
    }
}