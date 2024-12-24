using LuaNET;
using NeoServer.Game.Common.Location.Structs;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class PositionFunctions : LuaScriptInterface, IPositionFunctions
{
    public PositionFunctions(
        ILuaEnvironment luaEnvironment, ILogger logger) : base(nameof(PositionFunctions))
    {
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Position", "", LuaCreatePosition);
        RegisterMetaMethod(L, "Position", "__add", LuaPositionAdd);
        RegisterMetaMethod(L, "Position", "__sub", LuaPositionSub);
        RegisterMetaMethod(L, "Position", "__eq", LuaUserdataCompareStruct<Location>);
    }

    public static int LuaCreatePosition(LuaState L)
    {
        // Position([x = 0[, y = 0[, z = 0[, stackpos = 0]]]])
        // Position([position])
        if (Lua.GetTop(L) <= 1)
        {
            PushPosition(L, new Location());
            return 1;
        }

        int stackpos = 0;

        if (Lua.IsTable(L, 2))
        {
            var position = GetPosition(L, 2, out stackpos);
            PushPosition(L, position);
        }
        else
        {
            var x = GetNumber<ushort>(L, 2, 0);
            var y = GetNumber<ushort>(L, 3, 0);
            var z = GetNumber<byte>(L, 4, 0);
            stackpos = GetNumber<int>(L, 5, 0);

            var position = new Location(x, y, z);

            PushPosition(L, position);
        }

        return 1;
    }

    public static int LuaPositionAdd(LuaState L)
    {
        // positionValue = position + positionEx
        var position = GetPosition(L, 1, out var stackpos);
        Location positionEx;
        if (stackpos == 0)
            positionEx = GetPosition(L, 2, out stackpos);
        else
            positionEx = GetPosition(L, 2);

        position.X += positionEx.X;
        position.Y += positionEx.Y;
        position.Z += positionEx.Z;

        PushPosition(L, position, stackpos);

        return 1;
    }

    public static int LuaPositionSub(LuaState L)
    {
        // positionValue = position - positionEx
        var position = GetPosition(L, 1, out var stackpos);
        Location positionEx;
        if (stackpos == 0)
            positionEx = GetPosition(L, 2, out stackpos);
        else
            positionEx = GetPosition(L, 2);

        position.X -= positionEx.X;
        position.Y -= positionEx.Y;
        position.Z -= positionEx.Z;

        PushPosition(L, position, stackpos);

        return 1;
    }
}
