using System;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Spells.Entities;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class AttrPlayerCommand : CommandSpell
{
    private static void AdjustExperience(IPlayer player, int level)
    {
        if (level < 0)
        {
            var expTarget = Skill.CalculateExpByLevel(player.Level - Math.Abs(level));
            player.LoseExperience((long)expTarget);
            return;
        }

        var expForNewLevel = Skill.CalculateExpByLevel(player.Level + level);
        player.GainExperience((long)(expForNewLevel - player.Experience));
    }

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (Params.Length != 2)
            return Result.NotApplicable;

        var ctx = IoC.GetInstance<IGameCreatureManager>();
        ctx.TryGetPlayer(Params[0].ToString(), out var player);

        if (player is null)
            return Result.NotApplicable;

        if (!int.TryParse((string)Params[1], out var level))
            return Result.NotApplicable;

        AdjustExperience(player, level);

        return Result.Success;
    }
}