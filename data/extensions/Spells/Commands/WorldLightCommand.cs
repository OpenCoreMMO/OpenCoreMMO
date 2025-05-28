using System;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Results;
using NeoServer.Game.World;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class WorldLightCommand : CommandSpell
{
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        var input = Params?.Length > 0 ? Params[0].ToString() : string.Empty;
        var world = IoC.GetInstance<World>();
        
        if (world is null)
            return Result.NotApplicable;

        if (!Enum.TryParse(input, true, out Period period))
            return Result.NotApplicable;
        
        world.WorldLight.SetWorldLight(period);

        return Result.Success;
    }
}