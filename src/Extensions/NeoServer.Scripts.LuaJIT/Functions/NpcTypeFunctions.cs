using LuaNET;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Creatures.Monster.Combat;
using NeoServer.Game.Creatures.Npcs;
using NeoServer.Game.Creatures.Npcs.Shop;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class NpcTypeFunctions : LuaScriptInterface, INpcTypeFunctions
{
    private static IGameCreatureManager _gameCreatureManager;
    private static INpcStore _npcStore;
    private static IItemTypeStore _itemTypeStore;
    private static IScripts _scripts;
    private static INpcs _npcs;

    public NpcTypeFunctions(
        IGameCreatureManager gameCreatureManager,
        INpcStore npcStore,
        IItemTypeStore itemTypeStore,
        IScripts scripts,
        INpcs npcs) : base(nameof(NpcTypeFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
        _npcStore = npcStore;
        _itemTypeStore = itemTypeStore;
        _scripts = scripts;
        _npcs = npcs;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "NpcType", "", LuaNpcTypeCreate);
        RegisterMetaMethod(luaState, "NpcType", "__eq", LuaUserdataCompare<INpcType>);

        //RegisterMethod(luaState, "NpcType", "isPushable", LuaNpcTypeIsPushable);

        RegisterMethod(luaState, "NpcType", "name", LuaNpcTypeName);

        RegisterMethod(luaState, "NpcType", "nameDescription", LuaNpcTypeNameDescription);

        RegisterMethod(luaState, "NpcType", "health", LuaNpcTypeHealth);
        RegisterMethod(luaState, "NpcType", "maxHealth", LuaNpcTypeMaxHealth);

        RegisterMethod(luaState, "NpcType", "getVoices", LuaNpcTypeGetVoices);
        RegisterMethod(luaState, "NpcType", "addVoices", LuaNpcTypeAddVoices);

        RegisterMethod(luaState, "NpcType", "registerEvent", LuaNpcTypeRegisterEvent);

        RegisterMethod(luaState, "NpcType", "eventType", LuaNpcTypeEventType);
        RegisterMethod(luaState, "NpcType", "onThink", LuaNpcTypeEventOnCallback);
        RegisterMethod(luaState, "NpcType", "onAppear", LuaNpcTypeEventOnCallback);
        RegisterMethod(luaState, "NpcType", "onDisappear", LuaNpcTypeEventOnCallback);
        RegisterMethod(luaState, "NpcType", "onMove", LuaNpcTypeEventOnCallback);
        RegisterMethod(luaState, "NpcType", "onSay", LuaNpcTypeEventOnCallback);
        RegisterMethod(luaState, "NpcType", "onCloseChannel", LuaNpcTypeEventOnCallback);
        RegisterMethod(luaState, "NpcType", "onBuyItem", LuaNpcTypeEventOnCallback);
        RegisterMethod(luaState, "NpcType", "onSellItem", LuaNpcTypeEventOnCallback);
        RegisterMethod(luaState, "NpcType", "onCheckItem", LuaNpcTypeEventOnCallback);

        RegisterMethod(luaState, "NpcType", "outfit", LuaNpcTypeOutfit);
        RegisterMethod(luaState, "NpcType", "baseSpeed", LuaNpcTypeBaseSpeed);
        RegisterMethod(luaState, "NpcType", "walkInterval", LuaNpcTypeWalkInterval);
        RegisterMethod(luaState, "NpcType", "walkRadius", LuaNpcTypeWalkRadius);
        //RegisterMethod(luaState, "NpcType", "light", LuaNpcTypeLight);

        RegisterMethod(luaState, "NpcType", "addShopItem", LuaNpcTypeAddShopItem);
    }

    internal static int LuaNpcTypeCreate(LuaState luaState)
    {
        // NpcType(name)
        var npcName = GetString(luaState, 1);
        var npcType = _npcStore.GetByName(npcName);

        if (npcType is null)
        {
            npcType = new NpcType
            {
                Name = npcName,
            };

            _npcStore.Add(npcName, npcType);
        }

        PushUserdata(luaState, npcType);
        SetMetatable(luaState, -1, "NpcType");
        return 1;
    }

    private static int LuaNpcTypeRegisterEvent(LuaState luaState)
    {
        // npcType:registerEvent(name)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            npcType.Script = GetString(luaState, 2);
            //npcType.Scripts.insert(Lua::getString(luaState, 2));
            Lua.PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaNpcTypeEventOnCallback(LuaState luaState)
    {
        // npcType:onThink(callback)
        // npcType:onAppear(callback)
        // npcType:onDisappear(callback)
        // npcType:onMove(callback)
        // npcType:onSay(callback)
        // npcType:onBuyItem(callback)
        // npcType:onSellItem(callback)
        // npcType:onCheckItem(callback)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            //todo:
            //if (npcType->loadCallback(_scripts.GetScriptInterface()))
            if (_npcs.LoadCallback(npcType.Name))
            {
                Lua.PushBoolean(luaState, true);
                return 1;
            }
            Lua.PushBoolean(luaState, false);
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaNpcTypeEventType(LuaState luaState)
    {
        // npcType:eventType(event)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            var eventType = GetNumber<NpcsEventType>(luaState, 2);
            //todo:
            //npcType->info.eventType = GetNumber<NpcsEventType>(L, 2);
            _npcs.Add(npcType.Name, eventType, _scripts.GetScriptInterface());
            Lua.PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    //private static int LuaNpcTypeIsPushable(LuaState luaState)
    //{
    //    // get: npcType:isPushable() set: npcType:isPushable(bool)
    //    var npcType = GetUserdata<INpcType>(luaState, 1);
    //    if (npcType is not null)
    //    {
    //        if (Lua.GetTop(luaState) == 1)
    //        {
    //            Lua.PushBoolean(luaState, npcType.Pushable);
    //        }
    //        else
    //        {
    //            npcType.Pushable = Lua.GetBoolean(luaState, 2);
    //            Lua.PushBoolean(luaState, true);
    //        }
    //    }
    //    else
    //    {
    //        Lua.PushNil(luaState);
    //    }
    //    return 1;
    //}

    private static int LuaNpcTypeName(LuaState luaState)
    {
        // get: npcType:name() set: npcType:name(name)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushString(luaState, npcType.Name);
            }
            else
            {
                npcType.Name = GetString(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaNpcTypeNameDescription(LuaState luaState)
    {
        // get: npcType:nameDescription() set: npcType:nameDescription(description)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushString(luaState, npcType.Description);
            }
            else
            {
                npcType.Description = GetString(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaNpcTypeHealth(LuaState luaState)
    {
        // get: npcType:health() set: npcType:health(health)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushNumber(luaState, npcType.Health);
            }
            else
            {
                npcType.Health = GetNumber<uint>(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaNpcTypeMaxHealth(LuaState luaState)
    {
        // get: npcType:maxHealth() set: npcType:maxHealth(health)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushNumber(luaState, npcType.MaxHealth);
            }
            else
            {
                npcType.MaxHealth = GetNumber<uint>(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaNpcTypeGetVoices(LuaState luaState)
    {
        // npcType:getVoices()
        var npcType = GetUserdata<INpcType>(luaState, 1);

        if (npcType == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        Lua.CreateTable(luaState, npcType.Voices.Count, 0);
        for (var i = 0; i < npcType.Voices.Count; i++)
        {
            Lua.CreateTable(luaState, 0, 2);
            SetField(luaState, "text", npcType.Voices[i].Sentence);
            SetField(luaState, "yellText", npcType.Voices[i].SpeechType == SpeechType.Yell);
            Lua.RawSetI(luaState, -2, ++i);
        }

        return 1;
    }

    private static int LuaNpcTypeAddVoices(LuaState luaState)
    {
        // npcType:addVoices(interval, chance, [yell, sentence])
        var npcType = GetUserdata<INpcType>(luaState, 1);

        if (npcType != null)
        {
            var parameters = Lua.GetTop(luaState);

            var interval = GetNumber<ushort>(luaState, 2);
            var chance = GetNumber<byte>(luaState, 3);

            for (var i = 4; i < parameters; i += 2)
            {
                var yell = GetBoolean(luaState, i);
                var sentence = GetString(luaState, i + 1);
                var voice = new Voice(sentence, yell ? SpeechType.Yell : SpeechType.Say);
                npcType.Voices.Add(voice);
            }

            npcType.VoiceConfig = new IntervalChance(interval, chance);

            Lua.PushBoolean(luaState, true);
            return 1;
        }

        Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaNpcTypeOutfit(LuaState luaState)
    {
        // get: npcType:outfit() set: npcType:outfit(outfit)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                PushOutfitLook(luaState, npcType.Look);
            }
            else
            {
                npcType.Look = GetOutfitLook(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }

            return 1;
        }

        Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaNpcTypeBaseSpeed(LuaState luaState)
    {
        // get: npcType:baseSpeed() set: npcType:baseSpeed(speed)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushNumber(luaState, npcType.Speed);
            }
            else
            {
                npcType.Speed = GetNumber<ushort>(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }

            return 1;
        }

        Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaNpcTypeWalkInterval(LuaState luaState)
    {
        // get: npcType:walkInterval() set: npcType:walkInterval(interval)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushNumber(luaState, npcType.WalkInterval);
            }
            else
            {
                npcType.WalkInterval = GetNumber<ushort>(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }

            return 1;
        }

        Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaNpcTypeWalkRadius(LuaState luaState)
    {
        // get: npcType:walkRadius() set: npcType:walkRadius(id)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushNumber(luaState, npcType.WalkRadius);
            }
            else
            {
                npcType.WalkRadius = GetNumber<ushort>(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }

            return 1;
        }

        Lua.PushNil(luaState);
        return 1;
    }

    private static int LuaNpcTypeAddShopItem(LuaState luaState)
    {
        // npcType:addShopItem(itemId, buyPrice, sellPrice)
        var npcType = GetUserdata<INpcType>(luaState, 1);
        if (npcType is not null)
        {
            var itemId = GetNumber<ushort>(luaState, 2);
            var buyPrice = GetNumber<uint>(luaState, 3);
            var sellPrice = GetNumber<uint>(luaState, 4);

            var itemType = _itemTypeStore.Get(itemId);

            if (itemType != null)
            {
                var shopItem = new ShopItem(itemType, buyPrice, sellPrice);

                npcType.ShopItems.Add(itemId, shopItem);

                Lua.PushBoolean(luaState, true);

                return 1;
            }
        }

        Lua.PushNil(luaState);
        return 1;
    }
}