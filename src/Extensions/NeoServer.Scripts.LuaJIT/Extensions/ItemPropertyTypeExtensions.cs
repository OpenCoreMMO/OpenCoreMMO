using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Extensions
{
    public static class ItemPropertyTypeExtensions
    {
        public static TileFlags ToTileFlag(this ItemPropertyType value)
        {
            return value switch
            {
                ItemPropertyType.CONST_PROP_BLOCKSOLID => TileFlags.BLockSolid,
                ItemPropertyType.CONST_PROP_HASHEIGHT => TileFlags.HasHeight,
                ItemPropertyType.CONST_PROP_BLOCKPROJECTILE => TileFlags.BlockProjecTile,
                ItemPropertyType.CONST_PROP_BLOCKPATH => TileFlags.BlockPath,
                ItemPropertyType.CONST_PROP_ISVERTICAL => TileFlags.IsVertical,
                ItemPropertyType.CONST_PROP_ISHORIZONTAL => TileFlags.IsHorizontal,
                ItemPropertyType.CONST_PROP_MOVEABLE => TileFlags.Moveable,
                ItemPropertyType.CONST_PROP_IMMOVABLEBLOCKSOLID => TileFlags.ImmovableBlockSolid,
                ItemPropertyType.CONST_PROP_IMMOVABLEBLOCKPATH => TileFlags.ImmovableBlockPath,
                ItemPropertyType.CONST_PROP_IMMOVABLENOFIELDBLOCKPATH => TileFlags.ImmovableNoFieldBlockPath,
                ItemPropertyType.CONST_PROP_NOFIELDBLOCKPATH => TileFlags.NoFieldBlockPath,
                ItemPropertyType.CONST_PROP_SUPPORTHANGABLE => TileFlags.SupportsHangable,
                _ => TileFlags.None
            };
        }

        public static ItemFlag ToItemFlag(this ItemPropertyType value)
        {
            return value switch
            {
                ItemPropertyType.CONST_PROP_BLOCKSOLID => ItemFlag.Unpassable,
                ItemPropertyType.CONST_PROP_HASHEIGHT => ItemFlag.HasHeight,
                ItemPropertyType.CONST_PROP_BLOCKPROJECTILE => ItemFlag.BlockProjectTile,
                ItemPropertyType.CONST_PROP_BLOCKPATH => ItemFlag.BlockPathFind,
                ItemPropertyType.CONST_PROP_ISVERTICAL => ItemFlag.Vertical,
                ItemPropertyType.CONST_PROP_ISHORIZONTAL => ItemFlag.Horizontal,
                ItemPropertyType.CONST_PROP_MOVEABLE => ItemFlag.Movable,
                ItemPropertyType.CONST_PROP_IMMOVABLEBLOCKSOLID => ItemFlag.Unpassable,
                ItemPropertyType.CONST_PROP_IMMOVABLEBLOCKPATH => ItemFlag.BlockPathFind,
                ItemPropertyType.CONST_PROP_IMMOVABLENOFIELDBLOCKPATH => ItemFlag.BlockPathFind,
                ItemPropertyType.CONST_PROP_NOFIELDBLOCKPATH => ItemFlag.BlockPathFind,
                ItemPropertyType.CONST_PROP_SUPPORTHANGABLE => ItemFlag.Horizontal | ItemFlag.Vertical,
                _ => default
            };
        }

        public static ItemPropertyType ToItemPropertyType(this TileFlags value)
        {
            return value switch
            {
                TileFlags.BLockSolid => ItemPropertyType.CONST_PROP_BLOCKSOLID,
                TileFlags.HasHeight => ItemPropertyType.CONST_PROP_HASHEIGHT,
                TileFlags.BlockProjecTile => ItemPropertyType.CONST_PROP_BLOCKPROJECTILE,
                TileFlags.BlockPath => ItemPropertyType.CONST_PROP_BLOCKPATH,
                TileFlags.IsVertical => ItemPropertyType.CONST_PROP_ISVERTICAL,
                TileFlags.IsHorizontal => ItemPropertyType.CONST_PROP_ISHORIZONTAL,
                TileFlags.Moveable => ItemPropertyType.CONST_PROP_MOVEABLE,
                TileFlags.ImmovableBlockSolid => ItemPropertyType.CONST_PROP_IMMOVABLEBLOCKSOLID,
                TileFlags.ImmovableBlockPath => ItemPropertyType.CONST_PROP_IMMOVABLEBLOCKPATH,
                TileFlags.ImmovableNoFieldBlockPath => ItemPropertyType.CONST_PROP_IMMOVABLENOFIELDBLOCKPATH,
                TileFlags.NoFieldBlockPath => ItemPropertyType.CONST_PROP_NOFIELDBLOCKPATH,
                TileFlags.SupportsHangable => ItemPropertyType.CONST_PROP_SUPPORTHANGABLE,
                _ => default
            };
        }


    }
}
