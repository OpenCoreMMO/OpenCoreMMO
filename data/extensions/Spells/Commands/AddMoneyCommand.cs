using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class AddMoneyCommand: CommandSpell
{
    public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;

        if (Params.Length != 2)
            return false;

        var ctx = IoC.GetInstance<IGameCreatureManager>();
        ctx.TryGetPlayer(Params[0].ToString(), out var target);

        if (target is null)
            return false;
        
        ulong.TryParse(Params[1].ToString(), out var amount);

        target.Bank?.Credit(amount);

        return true;
    }
}