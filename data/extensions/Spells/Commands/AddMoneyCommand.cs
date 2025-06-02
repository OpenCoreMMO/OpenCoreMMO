using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Results;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class AddMoneyCommand: CommandSpell
{
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (Params.Length != 2)
            return Result.NotPossible;

        var ctx = IoC.GetInstance<IGameCreatureManager>();
        ctx.TryGetPlayer(Params[0].ToString(), out var targetPlayer);

        if (targetPlayer is null)
            return Result.NotPossible;
        
        ulong.TryParse(Params[1].ToString(), out var amount);

        targetPlayer.Bank?.Credit(amount);

        return Result.Success;
    }
}