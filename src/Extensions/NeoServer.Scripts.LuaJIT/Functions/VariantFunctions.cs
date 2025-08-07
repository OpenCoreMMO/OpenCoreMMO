using LuaNET;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class VariantFunctions : LuaScriptInterface, IVariantFunctions
{
    public VariantFunctions() : base(nameof(TeleportFunctions))
    {
    }

    public void Init(LuaState luaState)
    {
        RegisterClass(luaState, "Variant", "", LuaVariantCreate);

        RegisterMethod(luaState, "Variant", "getNumber", LuaVariantGetNumber);
        RegisterMethod(luaState, "Variant", "getString", LuaVariantGetString);
        RegisterMethod(luaState, "Variant", "getPosition", LuaVariantGetPosition);
    }

    private static int LuaVariantCreate(LuaState luaState)
    {
        // Variant(number or string or position or thing)
        var id = GetNumber<uint>(luaState, 2);

        LuaVariant variant = default;
        if (Lua.IsUserData(luaState, 2))
        {
            if (GetThing(luaState, 2) is IThing thing)
            {
                variant.Type = LuaVariantType.VARIANT_TARGETPOSITION;
                variant.Pos = thing.Location;
            }
        }
        else if (Lua.IsTable(luaState, 2))
        {
            variant.Type = LuaVariantType.VARIANT_POSITION;
            variant.Pos = GetPosition(luaState, 2);
        }
        else if (Lua.IsNumber(luaState, 2))
        {
            variant.Type = LuaVariantType.VARIANT_NUMBER;
            variant.Number = GetNumber<uint>(luaState, 2);
        }
        else if (Lua.IsString(luaState, 2))
        {
            variant.Type = LuaVariantType.VARIANT_STRING;
            variant.Text = GetString(luaState, 2);
        }

        PushVariant(luaState, variant);
        return 1;
    }

    private static int LuaVariantGetNumber(LuaState luaState)
    {
        // Variant:Lua::getNumber()
        var variant = GetVariant(luaState, 1);
        if (variant.Type == LuaVariantType.VARIANT_NUMBER)
            Lua.PushNumber(luaState, variant.Number);
        else
            Lua.PushNumber(luaState, 0);
        return 1;
    }

    private static int LuaVariantGetString(LuaState luaState)
    {
        // Variant:Lua::getString()
        var variant = GetVariant(luaState, 1);
        if (variant.Type == LuaVariantType.VARIANT_STRING)
            Lua.PushString(luaState, variant.Text);
        else
            Lua.PushString(luaState, string.Empty);
        return 1;
    }

    private static int LuaVariantGetPosition(LuaState luaState)
    {
        // Variant:Lua::getPosition()
        var variant = GetVariant(luaState, 1);
        if (variant.Type == LuaVariantType.VARIANT_POSITION || variant.Type == LuaVariantType.VARIANT_TARGETPOSITION)
            PushPosition(luaState, variant.Pos);
        else
            PushPosition(luaState, Location.Zero);
        return 1;
    }
}