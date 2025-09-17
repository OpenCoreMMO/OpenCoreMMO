using System;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Spells.Entities;
using NeoServer.Domain.World;
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