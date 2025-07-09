using LuaNET;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Spells;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Models.Spell;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class SpellFunctions : LuaScriptInterface, ISpellFunctions
{
    private static ILogger _logger;
    private static IItemTypeStore _itemTypeStore;
    private static IVocationStore _vocationStore;
    private static SpellListManager _spellListManager;

    public SpellFunctions(ILogger logger,
        IItemTypeStore itemTypeStore,
        IVocationStore vocationStore,
        SpellListManager spellListManager) : base(nameof(SpellFunctions))
    {
        _logger = logger;
        _itemTypeStore = itemTypeStore;
        _vocationStore = vocationStore;
        _spellListManager = spellListManager;
    }

    public void Init(LuaState lua)
    {
        RegisterSharedClass(lua, "Spell", "", LuaSpellCreate);
        RegisterMetaMethod(lua, "Spell", "__eq", LuaUserdataCompare);

        RegisterMethod(lua, "Spell", "onCastSpell", LuaSpellCastSpell);
        RegisterMethod(lua, "Spell", "register", LuaSpellRegister);
        RegisterMethod(lua, "Spell", "name", LuaSpellName);
        RegisterMethod(lua, "Spell", "id", LuaSpellId);
        RegisterMethod(lua, "Spell", "group", LuaSpellGroup);
        RegisterMethod(lua, "Spell", "cooldown", LuaSpellCooldown);
        RegisterMethod(lua, "Spell", "groupCooldown", LuaSpellGroupCooldown);
        RegisterMethod(lua, "Spell", "level", LuaSpellLevel);
        RegisterMethod(lua, "Spell", "magicLevel", LuaSpellMagicLevel);
        RegisterMethod(lua, "Spell", "mana", LuaSpellMana);
        RegisterMethod(lua, "Spell", "manaPercente", LuaSpellManaPercent);
        RegisterMethod(lua, "Spell", "soul", LuaSpellSoul);
        RegisterMethod(lua, "Spell", "range", LuaSpellRange);
        RegisterMethod(lua, "Spell", "isPremium", LuaSpellIsPremium);
        RegisterMethod(lua, "Spell", "isEnabled", LuaSpellIsEnabled);
        RegisterMethod(lua, "Spell", "needTarget", LuaSpellNeedTarget);
        RegisterMethod(lua, "Spell", "needWeapon", LuaSpellNeedWeapon);
        RegisterMethod(lua, "Spell", "needLearn", LuaSpellNeedLearn);
        RegisterMethod(lua, "Spell", "isSelfTarget", LuaSpellIsSelfTarget);
        RegisterMethod(lua, "Spell", "isBlocking", LuaSpellIsBlocking);
        RegisterMethod(lua, "Spell", "isAggressive", LuaSpellIsAgressive);
        RegisterMethod(lua, "Spell", "vocation", LuaSpellVocation);

        // Only for InstantSpell.
        RegisterMethod(lua, "Spell", "words", LuaSpellWords);
        RegisterMethod(lua, "Spell", "needDirection", LuaSpellNeedDirection);
        RegisterMethod(lua, "Spell", "needCasterTargetOrDirection", LuaSpellNeedCasterTargetOrDirection);

        //only for rune spells
        RegisterMethod(lua, "Spell", "runeId", LuaSpellRuneId);
        RegisterMethod(lua, "Spell", "charges", LuaSpellCharges);
        RegisterMethod(lua, "Spell", "allowFarUse", LuaSpellAllowFarUse);
        RegisterMethod(lua, "Spell", "blockWalls", LuaSpellBlockWalls);
        RegisterMethod(lua, "Spell", "checkFloor", LuaSpellCheckFloor);
    }

    public static int LuaSpellCreate(LuaState lua)
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
            var rune = new LuaRuneSpell(GetScriptEnv().GetScriptInterface());
            PushUserdata(lua, rune);
            SetMetatable(lua, -1, "Spell");
            rune.SpellType = spellType;
            return 1;
        }
        else if (spellType == SpellType.Instant)
        {
            var spell = new LuaInstantSpell(GetScriptEnv().GetScriptInterface());
            PushUserdata(lua, spell);
            SetMetatable(lua, -1, "Spell");
            spell.SpellType = spellType;
            return 1;
        }

        Lua.PushNil(lua);
        return 1;
    }

    private static int LuaSpellCastSpell(LuaState lua)
    {
        // spell:onCastSpell(callback)
        var spell = GetUserdata<LuaSpell>(lua, 1);
        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (spell is LuaRuneSpell rune)
        {
            if (!rune.LoadCallback())
            {
                PushBoolean(lua, false);
                return 1;
            }

            rune.SetLoadedCallback(true);
            PushBoolean(lua, true);
        }

        if (spell is LuaInstantSpell instant)
        {
            if (!instant.LoadCallback())
            {
                PushBoolean(lua, false);
                return 1;
            }

            instant.SetLoadedCallback(true);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int LuaSpellRegister(LuaState lua)
    {	
        // spell:register()
        var spell = GetUserdata<LuaSpell>(lua, 1);
        if (spell is null)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_SPELL_NOT_FOUND));
            Lua.PushBoolean(lua, false);
            return 1;
        }

        if (spell is LuaRuneSpell rune)
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
                runeSpell.ManaConsumption = rune.Mana;
                runeSpell.ManaPercent = rune.ManaPercent;
                runeSpell.SoulConsumption = rune.Soul;
                runeSpell.Range = rune.Range;
                runeSpell.BlockWalls = rune.BlockWalls;
                runeSpell.BlockingSolid = rune.BlockingSolid;
                runeSpell.BlockingCreature = rune.BlockingCreature;
                runeSpell.NeedsTarget = rune.NeedTarget;
                runeSpell.NeedsPremium = rune.IsPremium;
                runeSpell.VocationIds = rune.VocationIds;
                runeSpell.MinLevel = rune.Level;
                runeSpell.MinMagicLevel = rune.MagicLevel;
                runeSpell.IsEnabled = rune.IsEnabled;
                runeSpell.IsSelfTarget = rune.IsSelfTarget;
                runeSpell.IsAggressive = rune.IsAggressive;
                runeSpell.NeedLearn = rune.NeedLearn;
                runeSpell.NeedWeapon = rune.NeedWeapon;
            }

            if (runeSpell is null)
                runeSpell = new RuneSpell
                {
                    ManaConsumption = rune.Mana,
                    ManaPercent = rune.ManaPercent,
                    SoulConsumption = rune.Soul,
                    Range = rune.Range,
                    BlockWalls = rune.BlockWalls,
                    BlockingSolid = rune.BlockingSolid,
                    BlockingCreature = rune.BlockingCreature,
                    NeedsTarget = rune.NeedTarget,
                    NeedsPremium = rune.IsPremium,
                    VocationIds = rune.VocationIds,
                    MinLevel = rune.Level,
                    MinMagicLevel = rune.MagicLevel,
                    IsEnabled = rune.IsEnabled,
                    IsSelfTarget = rune.IsSelfTarget,
                    IsAggressive = rune.IsAggressive,
                    NeedLearn = rune.NeedLearn,
                    NeedWeapon = rune.NeedWeapon,
                };

            ((RuneSpell)runeSpell).LuaRune = rune;

            item.Attributes.SetCustomAttribute("spell", runeSpell);
        }
        else if (spell is LuaInstantSpell instant)
        {
            //todo: registrar spell aqui

            ISpell instantSpell = null;
            if (_spellListManager.TryGet(spell.Name, out instantSpell))
            {
                //instantSpell.SoulConsumption = instant.SoulConsumption;
                //instantSpell.BlockWalls = instant.BlockWalls;
                instantSpell.BlockingSolid = instant.BlockingSolid;
                instantSpell.BlockingCreature = instant.BlockingCreature;
                instantSpell.NeedsTarget = instant.NeedTarget;
                instantSpell.Words = instant.Words;
                instantSpell.ManaConsumption = instant.Mana;
                instantSpell.ManaPercent = instant.Mana;
                instantSpell.SoulConsumption = instant.Soul;
                instantSpell.Range = instant.Range;
                instantSpell.Name = instant.Name;
                instantSpell.NeedDirection = instant.NeedDirection;
                instantSpell.NeedLearn = instant.NeedLearn;
                instantSpell.NeedsPremium = instant.IsPremium;
                instantSpell.VocationIds = instant.VocationIds;
                instantSpell.MinLevel = instant.Level;
                instantSpell.MinMagicLevel = instant.MagicLevel;
                instantSpell.IsEnabled = instant.IsEnabled;
                instantSpell.IsSelfTarget = instant.IsSelfTarget;
                instantSpell.IsAggressive = instant.IsAggressive;
                instantSpell.NeedCasterTargetOrDirection = instant.NeedCasterTargetOrDirection;
                instantSpell.NeedWeapon = instant.NeedWeapon;
            }

            if (instantSpell is null)
                instantSpell = new InstantSpell
                {
                    //SoulConsumption = instant.SoulConsumption,
                    //BlockWalls = instant.BlockWalls,
                    BlockingSolid = instant.BlockingSolid,
                    BlockingCreature = instant.BlockingCreature,
                    NeedsTarget = instant.NeedTarget,
                    Words = instant.Words,
                    Name = instant.Name,
                    ManaConsumption = instant.Mana,
                    ManaPercent = instant.ManaPercent,
                    SoulConsumption = instant.Soul,
                    Range = instant.Range,
                    NeedDirection = instant.NeedDirection,
                    NeedLearn = instant.NeedLearn,
                    NeedsPremium = instant.IsPremium,
                    VocationIds = instant.VocationIds,
                    MinLevel = instant.Level,
                    MinMagicLevel = instant.MagicLevel,
                    IsEnabled = instant.IsEnabled,
                    IsSelfTarget = instant.IsSelfTarget,
                    IsAggressive = instant.IsAggressive,
                    NeedCasterTargetOrDirection = instant.NeedCasterTargetOrDirection,
                    NeedWeapon = instant.NeedWeapon,
                };

            ((InstantSpell)instantSpell).LuaInstantSpell = instant;

            _spellListManager.Add(instantSpell.Words, instantSpell);
        }

        return 1;
    }

    public static int LuaSpellName(LuaState lua)
    {	
        // spell:name(name)
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

    public static int LuaSpellId(LuaState lua)
    {
        // spell:id(id)
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

    public static int LuaSpellGroup(LuaState lua)
    {
        // spell:group(primaryGroup[, secondaryGroup])
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

                //todo: LuaSpell other groups 
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

    public static int LuaSpellCooldown(LuaState lua)
    {
        // spell:cooldown(cooldown)
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

    public static int LuaSpellGroupCooldown(LuaState lua)
    {
        // spell:groupCooldown(primaryGroupCd[, secondaryGroupCd])
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

    public static int LuaSpellLevel(LuaState lua)
    {	
        // spell:level(lvl)
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
            spell.Level = GetNumber<ushort>(lua, 2);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int LuaSpellMagicLevel(LuaState lua)
    {
        // spell:magicLevel(lvl)
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
            spell.MagicLevel = GetNumber<ushort>(lua, 2);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int LuaSpellMana(LuaState lua)
    {
        // spell:mana(mana)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushNumber(lua, spell.Mana);
            }
            else
            {
                spell.Mana = GetNumber<ushort>(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellManaPercent(LuaState lua)
    {
        // spell:manaPercent(percent)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushNumber(lua, spell.ManaPercent);
            }
            else
            {
                spell.ManaPercent = GetNumber<ushort>(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellSoul(LuaState lua)
    {
        // spell:soul(soul)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushNumber(lua, spell.Soul);
            }
            else
            {
                spell.Soul = GetNumber<ushort>(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellRange(LuaState lua)
    {
        // spell:range(range)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushNumber(lua, spell.Range ?? 0);
            }
            else
            {
                spell.Range = GetNumber<byte>(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellIsPremium(LuaState lua)
    {
        // spell:isPremium(bool)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushBoolean(lua, spell.IsPremium);
            }
            else
            {
                spell.IsPremium = GetBoolean(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellIsEnabled(LuaState lua)
    {
        // spell:isEnabled(bool)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushBoolean(lua, spell.IsEnabled);
            }
            else
            {
                spell.IsEnabled = GetBoolean(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellNeedTarget(LuaState lua)
    {
        // spell:needTarget(bool)
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

    public static int LuaSpellNeedWeapon(LuaState lua)
    {
        // spell:needWeapon(bool)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (Lua.GetTop(lua) == 1)
        {
            Lua.PushBoolean(lua, spell.NeedWeapon);
        }
        else
        {
            spell.NeedWeapon = GetBoolean(lua, 2);
            PushBoolean(lua, true);
        }

        return 1;
    }

    public static int LuaSpellNeedLearn(LuaState lua)
    {
        // spell:needLearn(bool)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushBoolean(lua, spell.NeedLearn);
            }
            else
            {
                spell.NeedLearn = GetBoolean(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellIsSelfTarget(LuaState lua)
    {
        // spell:isSelfTarget(bool)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushBoolean(lua, spell.IsSelfTarget);
            }
            else
            {
                spell.IsSelfTarget = GetBoolean(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellIsBlocking(LuaState lua)
    {
        // spell:isBlocking(blockingSolid, blockingCreature)
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

    public static int LuaSpellIsAgressive(LuaState lua)
    {
        // spell:isAggressive(bool)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushBoolean(lua, spell.IsAggressive);
            }
            else
            {
                spell.IsAggressive = GetBoolean(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellVocation(LuaState lua)
    {
        // spell:vocation(vocation)
        var spell = GetUserdata<LuaSpell>(lua, 1);

        if (spell is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.CreateTable(lua, 0, 0);
                int it = 0;
                foreach (var id in spell.VocationIds) {
                    ++it;
                    var name = _vocationStore.Get(id).Name;
                    Lua.SetField(lua, it, name);
                }

                Lua.SetMetaTable(lua, "Spell");
            }
            else
            {
                var parameters = Lua.GetTop(lua) - 1; // // - 1 because self is a parameter aswell, which we want to skip ofc

                var vocations = new List<byte>();

                for (int i = 0; i < parameters; i++)
                {
                    if (GetString(lua, 2 + i).Contains(';'))
                    {
                        var vocList = GetString(lua, 2 + i).Split(";");

                        var vocation = _vocationStore.GetByName(vocList[0]);
                        if (vocList.Length > 0 && vocList[1] == "true")
                            vocations.Add(vocation.Id);
                    }
                }

                spell.VocationIds = vocations.Count > 0 ? vocations.ToArray() : null;

                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    //Instan Spells
    public static int LuaSpellWords(LuaState lua)
    {
        // spell:words(words[, separator = ""])
        var instant = GetUserdata<LuaInstantSpell>(lua, 1);

        if (instant is not null)
        {
            // if spell != SPELL_RUNE, it means that this actually is no RuneSpell, so we return nil
            if (instant.SpellType != SpellType.Instant)
            {
                Lua.PushNil(lua);
                return 1;
            }

            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushString(lua, instant.Words);
            }
            else
            {
                var words = GetString(lua, 2);
                instant.Words = words;
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellNeedDirection(LuaState lua)
    {
        // spell:needDirection(bool)
        var instant = GetUserdata<LuaInstantSpell>(lua, 1);

        if (instant is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushBoolean(lua, instant.NeedDirection);
            }
            else
            {
                instant.NeedDirection = GetBoolean(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellNeedCasterTargetOrDirection(LuaState lua)
    {
        // spell:needCasterTargetOrDirection(bool)
        var instant = GetUserdata<LuaInstantSpell>(lua, 1);

        if (instant is not null)
        {
            if (Lua.GetTop(lua) == 1)
            {
                Lua.PushBoolean(lua, instant.NeedCasterTargetOrDirection);
            }
            else
            {
                instant.NeedCasterTargetOrDirection = GetBoolean(lua, 2);
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    //Rune Spells
    public static int LuaSpellRuneId(LuaState lua)
    {
        // spell:runeId(id)
        var rune = GetUserdata<LuaRuneSpell>(lua, 1);

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
                rune.RuneId = runeId;
                PushBoolean(lua, true);
            }
        }
        else
        {
            Lua.PushNil(lua);
        }

        return 1;
    }

    public static int LuaSpellCharges(LuaState lua)
    {
        // spell:charges(charges)
        var rune = GetUserdata<LuaRuneSpell>(lua, 1);

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

    public static int LuaSpellAllowFarUse(LuaState lua)
    {
        // spell:allowFarUse(bool)
        var rune = GetUserdata<LuaRuneSpell>(lua, 1);

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

    public int LuaSpellBlockWalls(LuaState l)
    {
        // spell:blockWalls(bool)
        var rune = GetUserdata<LuaRuneSpell>(l, 1);
        if (rune is not null)
        {
            if (rune.SpellType != SpellType.Rune)
            {
                Lua.PushNil(l);
                return 1;
            }

            if (Lua.GetTop(l) == 1)
            {
                Lua.PushBoolean(l, rune.CheckLineOfSight);
            }
            else
            {
                rune.CheckLineOfSight = (GetBoolean(l, 2));
                Lua.PushBoolean(l, true);
            }
        }
        else
        {
            Lua.PushNil(l);
        }
        return 1;
    }

    public static int LuaSpellCheckFloor(LuaState l)
    {
        // spell:checkFloor(bool)
        var rune = GetUserdata<LuaRuneSpell>(l, 1);
        if (rune is not null)
        {
            if (rune.SpellType != SpellType.Rune)
            {
                Lua.PushNil(l);
                return 1;
            }

            if (Lua.GetTop(l) == 1)
            {
                Lua.PushBoolean(l, rune.CheckFloor);
            }
            else
            {
                rune.CheckFloor = (GetBoolean(l, 2));
                Lua.PushBoolean(l, true);
            }
        }
        else
        {
            Lua.PushNil(l);
        }

        return 1;
    }
}