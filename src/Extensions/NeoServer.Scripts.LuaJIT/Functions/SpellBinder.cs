using LuaNET;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Spells;
using NeoServer.Scripts.LuaJIT.DataManagers;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Models.Spell;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class SpellBinder : LuaScriptInterface, ISpellFunctionMapper
{
    private static ILogger _logger;
    private static IItemTypeStore _itemTypeStore;
    private static RuneManager _runeManager;
    private static IItemClientServerIdMapStore _itemClientServerIdMapStore;
    private static SpellListManager _spellListManager;

    public SpellBinder(ILogger logger,
        IItemTypeStore itemTypeStore,
        RuneManager runeManager,
        IItemClientServerIdMapStore itemClientServerIdMapStore,
        SpellListManager spellListManager) : base(nameof(SpellBinder))
    {
        _logger = logger;
        _itemTypeStore = itemTypeStore;
        _runeManager = runeManager;
        _itemClientServerIdMapStore = itemClientServerIdMapStore;
        _spellListManager = spellListManager;
    }

    public void Init(LuaState lua)
    {
        RegisterSharedClass(lua, "Spell", "", HandleCreateSpellInstance);
        RegisterMetaMethod(lua, "Spell", "__eq", HandleSpellUserdataCompare);
        RegisterMethod(lua, "Spell", "id", HandleIdMethod);
        RegisterMethod(lua, "Spell", "group", HandleGroupMethod);
        RegisterMethod(lua, "Spell", "name", HandleNameMethod);

        RegisterMethod(lua, "Spell", "level", HandleLevelMethod);
        RegisterMethod(lua, "Spell", "magicLevel", HandleMagicLevelMethod);
        RegisterMethod(lua, "Spell", "cooldown", HandleCooldownMethod);
        RegisterMethod(lua, "Spell", "groupCooldown", HandleGroupCooldownMethod);
        RegisterMethod(lua, "Spell", "needTarget", HandleNeedTargetMethod);
        RegisterMethod(lua, "Spell", "isBlocking", HandleIsBlockingMethod);
        RegisterMethod(lua, "Spell", "onCastSpell", HandleOnCastSpellMethod);
        RegisterMethod(lua, "Spell", "register", HandleRegisterMethod);

        //only for rune spells
        RegisterMethod(lua, "Spell", "runeId", HandleRuneIdMethod);
        RegisterMethod(lua, "Spell", "allowFarUse", HandleAllowFarUseMethod);
        RegisterMethod(lua, "Spell", "charges", HandleChargesMethod);
        RegisterMethod(lua,"Spell", "blockWalls", HandleBlockWalls);
        RegisterMethod(lua,"Spell", "checkFloor", HandleCheckFloor);

        //todo: not implemented in 8.60
        RegisterMethod(lua, "Spell", "castSound", HandleNotImplementedFunction);
        RegisterMethod(lua, "Spell", "impactSound", HandleNotImplementedFunction);
    }

    private static int HandleOnCastSpellMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);
        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        //todo: implement instant

        if (spell is LuaRune rune)
        {
            if (!rune.LoadCallback())
            {
                PushBoolean(lua, false);
                return 1;
            }

            rune.SetLoadedCallback(true);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int HandleRegisterMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);
        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (spell is LuaRune rune)
        {
            var item = _itemTypeStore.Get((ushort)rune.RuneId);

            if (string.IsNullOrWhiteSpace(item.Name)) item.UpdateName(rune.Name);

            item.Attributes.SetAttribute(ItemAttribute.MinimumMagicLevel, rune.MagicLevel);
            item.Attributes.SetAttribute(ItemAttribute.MinimumLevel, rune.Level);
            item.Attributes.SetAttribute(ItemAttribute.Charges, rune.Charges);
            item.Attributes.SetAttribute(ItemAttribute.AllowFarUse, rune.AllowFarUse);
            item.Attributes.SetAttribute(ItemAttribute.CheckFloor, rune.CheckFloor);

            item.Attributes.SetAttribute(ItemAttribute.CooldownTime, rune.Cooldown);
            item.Attributes.SetAttribute(ItemAttribute.PrimaryGroupCooldown, rune.PrimaryGroupCooldown);
            item.Attributes.SetAttribute(ItemAttribute.PrimaryGroupCooldown, rune.SecondaryGroupCooldown);
            item.Attributes.SetAttribute(ItemAttribute.PrimaryGroup, rune.PrimaryGroup);
            item.Attributes.SetAttribute(ItemAttribute.SecondaryGroup, rune.SecondaryGroup);

            ISpell runeSpell = null;
            if (_spellListManager.TryGet(rune.Name, out runeSpell))
            {
                runeSpell.ManaConsumption = rune.ManaConsumption;
                runeSpell.SoulConsumption = rune.SoulConsumption;
                runeSpell.BlockWalls = rune.BlockWalls;
                runeSpell.BlockingSolid = rune.BlockingSolid;
                runeSpell.BlockingCreature = rune.BlockingCreature;
                runeSpell.NeedsTarget = rune.NeedTarget;
            }

            if (runeSpell is null)
                runeSpell = new RuneSpell
                {
                    ManaConsumption = rune.ManaConsumption,
                    SoulConsumption = rune.SoulConsumption,
                    BlockWalls = rune.BlockWalls,
                    BlockingSolid = rune.BlockingSolid,
                    BlockingCreature = rune.BlockingCreature,
                    NeedsTarget = rune.NeedTarget
                };

            ((RuneSpell)runeSpell).LuaRune = rune;

            item.Attributes.SetCustomAttribute("spell", runeSpell);

            _runeManager.Register(rune);
        }

        return 1;
    }

    public static int HandleIsBlockingMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (Lua.GetTop(lua) == 1)
        {
            Lua.PushBoolean(lua, spell.BlockingSolid);
            Lua.PushBoolean(lua, spell.BlockingCreature);
            return 2;
        }

        spell.BlockingSolid = GetBoolean(lua, 2);
        spell.BlockingCreature = GetBoolean(lua, 3);
        PushBoolean(lua, true);

        return 1;
    }

    public static int HandleNeedTargetMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (Lua.GetTop(lua) == 1)
        {
            Lua.PushBoolean(lua, spell.NeedTarget);
        }
        else
        {
            spell.NeedTarget = GetBoolean(lua, 2);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int HandleGroupCooldownMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        var numberOfArgs = GetArgsCount(lua);

        if (numberOfArgs == 0)
        {
            Lua.PushNumber(lua, spell.PrimaryGroupCooldown);
            Lua.PushNumber(lua, spell.SecondaryGroupCooldown);
            return 2;
        }

        if (numberOfArgs == 1)
        {
            spell.PrimaryGroupCooldown = GetNumber<uint>(lua, 2);
            PushBoolean(lua, true);
        }

        if (numberOfArgs == 2)
        {
            spell.PrimaryGroupCooldown = GetNumber<uint>(lua, 2);
            spell.SecondaryGroupCooldown = GetNumber<uint>(lua, 3);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int HandleCooldownMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (Lua.GetTop(lua) == 1)
        {
            Lua.PushNumber(lua, spell.Cooldown);
        }
        else
        {
            spell.Cooldown = GetNumber<uint>(lua, 2);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int HandleMagicLevelMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (Lua.GetTop(lua) == 1)
        {
            Lua.PushNumber(lua, spell.MagicLevel);
        }
        else
        {
            spell.MagicLevel = GetNumber<int>(lua, 2);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int HandleLevelMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (Lua.GetTop(lua) == 1)
        {
            Lua.PushNumber(lua, spell.Level);
        }
        else
        {
            spell.Level = GetNumber<int>(lua, 2);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int HandleChargesMethod(LuaState lua)
    {
        var rune = GetUserdata<LuaRune>(lua, 1);

        if (rune is not null)
        {
            // if spell != SPELL_RUNE, it means that this actually is no RuneSpell, so we return nil
            if (rune.SpellType != SpellType.Rune)
            {
                Lua.PushNil(lua);
                return 1;
            }

            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushNumber(lua, rune.Charges);
            }
            else
            {
                rune.Charges = GetNumber<int>(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int HandleAllowFarUseMethod(LuaState lua)
    {
        var rune = GetUserdata<LuaRune>(lua, 1);

        if (rune is not null)
        {
            // if spell != SPELL_RUNE, it means that this actually is no RuneSpell, so we return nil
            if (rune.SpellType != SpellType.Rune)
            {
                Lua.PushNil(lua);
                return 1;
            }

            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushBoolean(lua, rune.AllowFarUse);
            }
            else
            {
                rune.AllowFarUse = GetBoolean(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }
    
    public static int HandleCheckFloor(LuaState l)
    {
        var rune = GetUserdata<LuaRune>(l, 1);
        if (rune is not null) {
            if (rune.SpellType != SpellType.Rune) {
                Lua.PushNil(l);
                return 1;
            }

            if (Lua.GetTop(l) == 1)
            {
                Lua.PushBoolean(l, rune.CheckFloor);
            }
            else {
                rune.BlockWalls = (GetBoolean(l, 2));
                Lua.PushBoolean(l, true);
            }
        } else {
            Lua.PushNil(l);
        }
        
        return 1;
    }

    private int HandleBlockWalls(LuaState l)
    {
        var rune = GetUserdata<LuaRune>(l, 1);
        if (rune is not null) {
            if (rune.SpellType != SpellType.Rune) {
                Lua.PushNil(l);
                return 1;
            }

            if (Lua.GetTop(l) == 1) {
                Lua.PushBoolean(l, rune.CheckLineOfSight);
            } else {
                rune.CheckLineOfSight = (GetBoolean(l, 2));
                Lua.PushBoolean(l, true);
            }
        } else {
            Lua.PushNil(l);
        }
        return 1;
    }

    public static int HandleRuneIdMethod(LuaState lua)
    {
        var rune = GetUserdata<LuaRune>(lua, 1);

        if (rune is not null)
        {
            // if spell != SPELL_RUNE, it means that this actually is no RuneSpell, so we return nil
            if (rune.SpellType != SpellType.Rune)
            {
                Lua.PushNil(lua);
                return 1;
            }

            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushNumber(lua, rune.RuneId);
            }
            else
            {
                var runeId = GetNumber<ushort>(lua, 2);

                _itemClientServerIdMapStore.TryGetValue(runeId, out var serverId);
                rune.RuneId = serverId;

                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    /// <summary>
    ///     Register spell name method -> spell:name(name)
    /// </summary>
    public static int HandleNameMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);
        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                PushString(lua, spell.Name);
            }
            else
            {
                spell.Name = GetString(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int HandleGroupMethod(LuaState lua)
    {
        //todo: need to implement group spell name

        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        var numberOfArgs = GetArgsCount(lua);

        if (numberOfArgs == 0)
        {
            Lua.PushNumber(lua, (int)spell.PrimaryGroup);
            Lua.PushNumber(lua, (int)spell.SecondaryGroup);
            return 2;
        }

        if (numberOfArgs == 1)
        {
            if (IsNumber(lua, 2))
            {
                var group = GetNumber<SpellGroup>(lua, 2);
                spell.PrimaryGroup = group;
                PushBoolean(lua, true);
                return 1;
            }

            if (IsString(lua, 2))
            {
                var group = GetString(lua, 2);

                //todo: handle other groups 
                if (group == "attack") spell.PrimaryGroup = SpellGroup.Attack;

                PushBoolean(lua, true);
                return 1;
            }
        }

        if (numberOfArgs == 2)
        {
            var primaryGroup = GetNumber<SpellGroup>(lua, 2);
            var secondaryGroup = GetNumber<SpellGroup>(lua, 2);
            spell.PrimaryGroup = primaryGroup;
            spell.SecondaryGroup = secondaryGroup;
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int HandleIdMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (spell.SpellType == SpellType.Undefined)
        {
            _logger.Error("The method: 'spell:id(id)' is only for use of instant spells and rune spells");
            PushBoolean(lua, false);
            return 1;
        }

        if (Lua.GetTop(lua) == 1)
        {
            Lua.PushNumber(lua, spell.Id);
        }
        else
        {
            spell.Id = GetNumber<ushort>(lua, 2);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int HandleCreateSpellInstance(LuaState lua)
    {
        if (Lua.GetTop(lua) == 1)
        {
            _logger.Error("[SpellFunctions::luaSpellCreate] - There is no parameter set!");
            Lua.PushNil(lua);
            return 1;
        }

        var spellType = SpellType.Undefined;

        if (IsNumber(lua, 2))
        {
            var id = GetNumber<ushort>(lua, 2);
            //todo
        }

        if (IsString(lua, 2))
        {
            var arg = GetString(lua, 2);

            if (arg.Equals("instant", StringComparison.InvariantCultureIgnoreCase)) spellType = SpellType.Instant;

            if (arg.Equals("rune", StringComparison.InvariantCultureIgnoreCase)) spellType = SpellType.Rune;
        }

        if (spellType == SpellType.Rune)
        {
            var rune = new LuaRune(GetScriptEnv().GetScriptInterface());
            PushUserdata(lua, rune);
            SetMetatable(lua, -1, "Spell");
            rune.SpellType = spellType;
            return 1;
        }

        Lua.PushNil(lua);
        return 1;
    }

    public static int HandleSpellUserdataCompare(LuaState luaState)
    {
        PushBoolean(luaState,
            EqualityComparer<object>.Default.Equals(GetUserdata<object>(luaState, 1),
                GetUserdata<object>(luaState, 2)));
        return 1;
    }
}