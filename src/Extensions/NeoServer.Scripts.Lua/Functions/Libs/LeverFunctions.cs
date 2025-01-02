using System;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Items.Factories;
using NeoServer.Game.World.Models.Tiles;
using NeoServer.Server.Helpers;

namespace NeoServer.Scripts.Lua.Functions.Libs;

public static class LeverFunctions
{
    public static void AddLibs(this NLua.Lua lua)
    {
        lua.DoString("lever_lib = {}");
        lua["lever_lib.switch"] = SwitchLever;
    }

    private static void SwitchLever(IItem item)
    {
        var map = IoC.GetInstance<IMap>();

        if (map[item.Location] is not DynamicTile dynamicTile) return;

        var newLeverId = (ushort)(item.Metadata.ServerId == 1946 ? 1945 : 1946);
        var newLever = ItemFactory.Instance.Create(newLeverId, item.Location,
            item.Metadata.Attributes.ToDictionary<ItemAttribute, IConvertible>());

        dynamicTile.RemoveItem(item, 1, out _);
        dynamicTile.AddItem(newLever);
    }
}