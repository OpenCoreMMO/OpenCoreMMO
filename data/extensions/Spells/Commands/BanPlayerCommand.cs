using NeoServer.Data.Interfaces;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class BanPlayerCommand : CommandSpell
{
    private const string BANISH_REASON = "You have been banished by a gamemaster.";

    public override bool OnCast(ICombatActor caster, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;

        if (Params.Length == 0)
            return false;

        var ctx = IoC.GetInstance<IGameCreatureManager>();
        var playerLogOutCommand = IoC.GetInstance<PlayerLogOutCommand>();
        var accountRepository = IoC.GetInstance<IAccountRepository>();

        if (!ctx.TryGetPlayer(Params[0].ToString(), out var player))
            return false;

        if (player is null || player.CreatureId == caster.CreatureId)
            return false;

        var reason = Params[1]?.ToString() ?? BANISH_REASON;

        accountRepository.Ban(player.AccountId, reason, ((IPlayer)caster).AccountId).Wait();
        playerLogOutCommand.Execute(player, true);

        return true;
    }
}