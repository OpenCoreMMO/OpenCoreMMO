using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.World.Map;
using NeoServer.Modules.Creatures;

namespace NeoServer.Extensions.Spells.Commands;

public class MonsterCreator : CommandSpell
{
    public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;
        if (Params?.Length == 0) return false;

        var monster = CreatureFactory.Instance.CreateMonster(Params[0].ToString());
        if (monster is null) return false;

        var map = Map.Instance;

        var locationToBorn = actor.Location.GetNextLocation(actor.Direction);
        var tileToBorn = map[actor.Location.GetNextLocation(actor.Direction)];

        if (tileToBorn is IDynamicTile)
        {
            if (tileToBorn.HasFlag(TileFlags.ProtectionZone))
            {
                error = InvalidOperation.NotEnoughRoom;
                return false;
            }

            monster.Born(locationToBorn);
            return true;
        }

        foreach (var neighbour in actor.Location.Neighbours)
            if (map[neighbour] is IDynamicTile { HasCreature: false })
            {
                monster.Born(neighbour);
                return true;
            }

        error = InvalidOperation.NotEnoughRoom;
        return false;
    }
}