using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Creatures.Factories;
using NeoServer.Game.World.Map;

namespace NeoServer.Extensions.Spells.Commands;

public class NpcCreator : CommandSpell
{
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (Params?.Length == 0) return Result.NotApplicable;

        var npc = CreatureFactory.Instance.CreateNpc(Params[0].ToString());
        if (npc is null) return Result.NotApplicable;

        var map = Map.Instance;

        var tileToBorn = map[caster.Location.GetNextLocation(caster.Direction)];

        npc.SetNewLocation(tileToBorn.Location);

        if (tileToBorn is IDynamicTile)
        {
            if (tileToBorn.HasFlag(TileFlags.ProtectionZone))
            {
                return Result.Fail(InvalidOperation.NotEnoughRoom);
            }

            map.PlaceCreature(npc);
            return Result.Success;
        }

        foreach (var neighbour in caster.Location.Neighbours)
        {
            if (map[neighbour] is IDynamicTile { HasCreature: false })
            {
                map.PlaceCreature(npc);
                return Result.Success;
            }
        }

        return Result.Fail(InvalidOperation.NotEnoughRoom);
    }
}