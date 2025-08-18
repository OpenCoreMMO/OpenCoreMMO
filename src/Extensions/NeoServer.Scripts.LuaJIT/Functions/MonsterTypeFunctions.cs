using LuaNET;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class MonsterTypeFunctions : LuaScriptInterface, IMonsterTypeFunctions
{
    private static IGameCreatureManager _gameCreatureManager;
    private static IMonsterTypeStore _monsterTypeStore;
    private static IItemTypeStore _itemTypeStore;
    private static IScripts _scripts;
    private static INpcs _npcs;
    private static IMonsterFactory _monsterFactory;

    public MonsterTypeFunctions(
        IGameCreatureManager gameCreatureManager,
        IMonsterTypeStore monsterTypeStore,
        IItemTypeStore itemTypeStore,
        IScripts scripts,
        INpcs npcs) : base(nameof(MonsterTypeFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
        _monsterTypeStore = monsterTypeStore;
        _itemTypeStore = itemTypeStore;
        _scripts = scripts;
        _npcs = npcs;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "MonsterType", "", LuaMonsterTypeCreate);
        RegisterMetaMethod(luaState, "MonsterType", "__eq", LuaUserdataCompare<IMonsterType>);

        RegisterMethod(luaState, "MonsterType", "isAttackable", LuaMonsterTypeIsAttackable);
        RegisterMethod(luaState, "MonsterType", "isConvinceable", LuaMonsterTypeIsConvinceable);
        RegisterMethod(luaState, "MonsterType", "isSummonable", LuaMonsterTypeIsSummonable);
        RegisterMethod(luaState, "MonsterType", "isIllusionable", LuaMonsterTypeIsIllusionable);
        RegisterMethod(luaState, "MonsterType", "isHostile", LuaMonsterTypeIsHostile);
        RegisterMethod(luaState, "MonsterType", "isRewardBoss", LuaMonsterTypeIsRewardBoss);
        RegisterMethod(luaState, "MonsterType", "isPushable", LuaMonsterTypeIsPushable);
        RegisterMethod(luaState, "MonsterType", "canPushItems", LuaMonsterTypeCanPushItems);
        RegisterMethod(luaState, "MonsterType", "canPushCreatures", LuaMonsterTypeCanPushCreatures);

        RegisterMethod(luaState, "MonsterType", "name", LuaMonsterTypeName);
        RegisterMethod(luaState, "MonsterType", "nameDescription", LuaMonsterTypeNameDescription);
        RegisterMethod(luaState, "MonsterType", "health", LuaMonsterTypeHealth);
        RegisterMethod(luaState, "MonsterType", "maxHealth", LuaMonsterTypeMaxHealth);

        RegisterMethod(luaState, "MonsterType", "corpseId", LuaMonsterTypeCorpseId);
        RegisterMethod(luaState, "MonsterType", "manaCost", LuaMonsterTypeManaCost);
    }

    internal static int LuaMonsterTypeCreate(LuaState luaState)
    {
        // MonsterType(name)
        var monsterName = GetString(luaState, 2);
        _monsterTypeStore.TryGetValue(monsterName, out var monsterType);

        if (monsterType is null)
        {
            monsterType = new MonsterType
            {
                Name = monsterName
            };

            _monsterTypeStore.AddOrUpdate(monsterName, monsterType);
        }

        PushUserdata(luaState, monsterType);
        SetMetatable(luaState, -1, "MonsterType");
        return 1;
    }

    private static int LuaMonsterTypeIsAttackable(LuaState luaState)
    {
        // get: monsterType:isAttackable() set: monsterType:isAttackable(bool)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Flags.TryGetValue(CreatureFlagAttribute.Attackable, out var value);
                Lua.PushBoolean(luaState, value != 0);
            }
            else
            {
                var value = GetBoolean(luaState, 2);
                monsterType.Flags.AddOrUpdate(CreatureFlagAttribute.Attackable, (ushort)(value ? 1 : 0));
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeIsConvinceable(LuaState luaState)
    {
        // get: monsterType:isConvinceable() set: monsterType:isConvinceable(bool)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Flags.TryGetValue(CreatureFlagAttribute.Convinceable, out var value);
                Lua.PushBoolean(luaState, value != 0);
            }
            else
            {
                var value = GetBoolean(luaState, 2);
                monsterType.Flags.AddOrUpdate(CreatureFlagAttribute.Convinceable, (ushort)(value ? 1 : 0));
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeIsSummonable(LuaState luaState)
    {
        // get: monsterType:IsSummonable() set: monsterType:IsSummonable(bool)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Flags.TryGetValue(CreatureFlagAttribute.Summonable, out var value);
                Lua.PushBoolean(luaState, value != 0);
            }
            else
            {
                var value = GetBoolean(luaState, 2);
                monsterType.Flags.AddOrUpdate(CreatureFlagAttribute.Summonable, (ushort)(value ? 1 : 0));
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeIsIllusionable(LuaState luaState)
    {
        // get: monsterType:IsIllusionable() set: monsterType:IsIllusionable(bool)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Flags.TryGetValue(CreatureFlagAttribute.Illusionable, out var value);
                Lua.PushBoolean(luaState, value != 0);
            }
            else
            {
                var value = GetBoolean(luaState, 2);
                monsterType.Flags.AddOrUpdate(CreatureFlagAttribute.Illusionable, (ushort)(value ? 1 : 0));
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeIsHostile(LuaState luaState)
    {
        // get: monsterType:IsHostile() set: monsterType:IsHostile(bool)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Flags.TryGetValue(CreatureFlagAttribute.Hostile, out var value);
                Lua.PushBoolean(luaState, value != 0);
            }
            else
            {
                var value = GetBoolean(luaState, 2);
                monsterType.Flags.AddOrUpdate(CreatureFlagAttribute.Hostile, (ushort)(value ? 1 : 0));
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeIsRewardBoss(LuaState luaState)
    {
        // get: monsterType:IsRewardBoss() set: monsterType:IsRewardBoss(bool)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Flags.TryGetValue(CreatureFlagAttribute.RewardBoss, out var value);
                Lua.PushBoolean(luaState, value != 0);
            }
            else
            {
                var value = GetBoolean(luaState, 2);
                monsterType.Flags.AddOrUpdate(CreatureFlagAttribute.RewardBoss, (ushort)(value ? 1 : 0));
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeIsPushable(LuaState luaState)
    {
        // get: monsterType:IsPushable() set: monsterType:IsPushable(bool)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Flags.TryGetValue(CreatureFlagAttribute.Pushable, out var value);
                Lua.PushBoolean(luaState, value != 0);
            }
            else
            {
                var value = GetBoolean(luaState, 2);
                monsterType.Flags.AddOrUpdate(CreatureFlagAttribute.Pushable, (ushort)(value ? 1 : 0));
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeCanPushItems(LuaState luaState)
    {
        // get: monsterType:CanPushItems() set: monsterType:CanPushItems(bool)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Flags.TryGetValue(CreatureFlagAttribute.CanPushItems, out var value);
                Lua.PushBoolean(luaState, value != 0);
            }
            else
            {
                var value = GetBoolean(luaState, 2);
                monsterType.Flags.AddOrUpdate(CreatureFlagAttribute.CanPushItems, (ushort)(value ? 1 : 0));
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeCanPushCreatures(LuaState luaState)
    {
        // get: monsterType:CanPushCreatures() set: monsterType:CanPushCreatures(bool)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Flags.TryGetValue(CreatureFlagAttribute.CanPushCreatures, out var value);
                Lua.PushBoolean(luaState, value != 0);
            }
            else
            {
                var value = GetBoolean(luaState, 2);
                monsterType.Flags.AddOrUpdate(CreatureFlagAttribute.CanPushCreatures, (ushort)(value ? 1 : 0));
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeName(LuaState luaState)
    {
        // get: monsterType:name() set: monsterType:name(name)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushString(luaState, monsterType.Name);
            }
            else
            {
                monsterType.Name = GetString(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeNameDescription(LuaState luaState)
    {
        // get: monsterType:nameDescription() set: monsterType:nameDescription(desc)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushString(luaState, monsterType.Description);
            }
            else
            {
                monsterType.Description = GetString(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeHealth(LuaState luaState)
    {
        // get: monsterType:health() set: monsterType:health(health)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushNumber(luaState, monsterType.Health);
            }
            else
            {
                monsterType.Health = GetNumber<uint>(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeMaxHealth(LuaState luaState)
    {
        // get: monsterType:maxHealth() set: monsterType:maxHealth(health)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushNumber(luaState, monsterType.MaxHealth);
            }
            else
            {
                monsterType.MaxHealth = GetNumber<uint>(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeCorpseId(LuaState luaState)
    {
        // get: monsterType:corpseId() set: monsterType:corpseId(id)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                monsterType.Look.TryGetValue(LookType.Corpse, out var value);
                Lua.PushNumber(luaState, value);
            }
            else
            {
                var value = GetNumber<ushort>(luaState, 2);
                monsterType.Look.AddOrUpdate(LookType.Corpse, value);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaMonsterTypeManaCost(LuaState luaState)
    {
        // get: monsterType:manaCost() set: monsterType:manaCost(mana)
        var monsterType = GetUserdata<IMonsterType>(luaState, 1);
        if (monsterType is not null)
        {
            if (Lua.GetTop(luaState) == 1)
            {
                Lua.PushNumber(luaState, monsterType.ManaCost);
            }
            else
            {
                monsterType.ManaCost = GetNumber<ushort>(luaState, 2);
                Lua.PushBoolean(luaState, true);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}