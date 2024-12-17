using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Creatures.Factories;
using NeoServer.Game.World.Map;

namespace NeoServer.Extensions.Spells.Commands;

public class NpcCreator : CommandSpell
{
    public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;
        if (Params?.Length == 0) return false;

        var npc = CreatureFactory.Instance.CreateNpc(Params[0].ToString());
        if (npc is null) return false;

        var map = Map.Instance;

        var tileToBorn = map[actor.Location.GetNextLocation(actor.Direction)];

        npc.SetNewLocation(tileToBorn.Location);

        if (tileToBorn is IDynamicTile)
        {
            if (tileToBorn.HasFlag(TileFlags.ProtectionZone))
            {
                error = InvalidOperation.NotEnoughRoom;
                return false;
            }

            map.PlaceCreature(npc);
            return true;
        }

        foreach (var neighbour in actor.Location.Neighbours)
            if (map[neighbour] is IDynamicTile { HasCreature: false })
            {
                map.PlaceCreature(npc);
                return true;
            }

        error = InvalidOperation.NotEnoughRoom;
        return false;
    }
}