using LuaNET;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Item;
using NeoServer.Scripts.LuaJIT.DataManagers;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Models.Spell;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class SpellFunctions(ILogger logger, IItemTypeStore itemTypeStore, RuneManager runeManager) : LuaScriptInterface(nameof(SpellFunctions)), ISpellFunctions
{
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

        //todo: not implemented in 8.60
        RegisterMethod(lua, "Spell", "castSound", HandleNotImplementedMethod);
        RegisterMethod(lua, "Spell", "impactSound", HandleNotImplementedMethod);
    }

    private int HandleOnCastSpellMethod(LuaState lua)
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

    private int HandleRegisterMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);
        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (spell is LuaRune rune)
        {
            var item = itemTypeStore.Get((ushort)rune.RuneId);
            
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                item.UpdateName(rune.Name);
            }
            
            item.Attributes.SetAttribute(ItemAttribute.MinimumMagicLevel, rune.MagicLevel);
            item.Attributes.SetAttribute(ItemAttribute.MinimumLevel, rune.Level);
            item.Attributes.SetAttribute(ItemAttribute.Charges, rune.Charges);
            item.Attributes.SetAttribute(ItemAttribute.AllowFarUse, rune.AllowFarUse);
            item.Attributes.SetAttribute(ItemAttribute.CheckFloor, rune.CheckFloor);
            item.Attributes.SetAttribute(ItemAttribute.BlockWalls, rune.BlockWalls);
            
            runeManager.Register(rune);
        }
        
        return 1;
    }

    private int HandleIsBlockingMethod(LuaState lua)
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

    private int HandleNeedTargetMethod(LuaState lua)
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

    private int HandleGroupCooldownMethod(LuaState lua)
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
            Lua.PushNumber(lua, spell.GroupCooldown);
            Lua.PushNumber(lua, spell.SecondaryGroupCooldown);
            return 2;
        }

        if (numberOfArgs == 1)
        {
            spell.GroupCooldown = GetNumber<uint>(lua, 2);
            PushBoolean(lua, true);
        }

        if (numberOfArgs == 2)
        {
            spell.GroupCooldown = GetNumber<uint>(lua, 2);
            spell.SecondaryGroupCooldown = GetNumber<uint>(lua, 3);
            PushBoolean(lua, true);
        }

        return 1;
    }

    private int HandleCooldownMethod(LuaState lua)
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

    private int HandleMagicLevelMethod(LuaState lua)
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

    private int HandleLevelMethod(LuaState lua)
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

    private int HandleChargesMethod(LuaState lua)
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

    private int HandleAllowFarUseMethod(LuaState lua)
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

    private int HandleRuneIdMethod(LuaState lua)
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
                rune.RuneId = GetNumber<ushort>(lua, 2);
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
    /// Register spell name method -> spell:name(name)
    /// </summary>
    private static int HandleNameMethod(LuaState lua)
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

    private int HandleGroupMethod(LuaState lua)
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
            Lua.PushNumber(lua, (int)spell.Group);
            Lua.PushNumber(lua, (int)spell.SecondaryGroup);
            return 2;
        }

        if (numberOfArgs == 1)
        {
            if (IsNumber(lua, 2))
            {
                var group = GetNumber<SpellGroup>(lua, 2);
                spell.Group = group;
                PushBoolean(lua, true);
                return 1;
            }

            if (IsString(lua, 2))
            {
                var group = GetString(lua, 2);

                //todo: handle other groups 
                if (group == "attack")
                {
                    spell.Group = SpellGroup.Attack;
                }

                PushBoolean(lua, true);
                return 1;
            }
        }

        if (numberOfArgs == 2)
        {
            var primaryGroup = GetNumber<SpellGroup>(lua, 2);
            var secondaryGroup = GetNumber<SpellGroup>(lua, 2);
            spell.Group = primaryGroup;
            spell.SecondaryGroup = secondaryGroup;
            PushBoolean(lua, true);
        }

        return 1;
    }

    private int HandleIdMethod(LuaState lua)
    {
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (spell.SpellType == SpellType.Undefined)
        {
            logger.Error("The method: 'spell:id(id)' is only for use of instant spells and rune spells");
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

    private int HandleCreateSpellInstance(LuaState lua)
    {
        if (Lua.GetTop(lua) == 1)
        {
            logger.Error("[SpellFunctions::luaSpellCreate] - There is no parameter set!");
            Lua.PushNil(lua);
            return 1;
        }

        var spellType = SpellType.Undefined;

        if (IsNumber(lua, 2))
        {
            ushort id = GetNumber<ushort>(lua, 2);
            //todo
        }

        if (IsString(lua, 2))
        {
            string arg = GetString(lua, 2);

            if (arg.Equals("instant", StringComparison.InvariantCultureIgnoreCase))
            {
                spellType = SpellType.Instant;
            }

            if (arg.Equals("rune", StringComparison.InvariantCultureIgnoreCase))
            {
                spellType = SpellType.Rune;
            }
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

    private static int HandleSpellUserdataCompare(LuaState luaState)
    {
        PushBoolean(luaState,
            EqualityComparer<object>.Default.Equals(GetUserdata<object>(luaState, 1),
                GetUserdata<object>(luaState, 2)));
        return 1;
    }
}