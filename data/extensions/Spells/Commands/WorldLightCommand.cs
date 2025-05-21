using System;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.World;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class WorldLightCommand : CommandSpell
{
    public override bool OnCast(ICombatActor caster, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;

        var input = Params?.Length > 0 ? Params[0].ToString() : string.Empty;
        var world = IoC.GetInstance<World>();
        
        if (world is null)
            return false;

        if (!Enum.TryParse(input, true, out Period period))
            return false;
        
        world.WorldLight.SetWorldLight(period);

        return true;
    }
}