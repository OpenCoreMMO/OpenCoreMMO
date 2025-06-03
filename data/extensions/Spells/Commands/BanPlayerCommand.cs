using NeoServer.Data.Interfaces;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Results;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class BanPlayerCommand : CommandSpell
{
    private const string BANISH_REASON = "You have been banished by a gamemaster.";
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        
        if (Params.Length == 0)
            return Result.NotApplicable;

        var ctx = IoC.GetInstance<IGameCreatureManager>();
        var playerLogOutCommand = IoC.GetInstance<PlayerLogOutCommand>();
        var accountRepository = IoC.GetInstance<IAccountRepository>();

        if (!ctx.TryGetPlayer(Params[0].ToString(), out var player))
            return Result.NotApplicable;

        if (player is null || player.CreatureId == caster.CreatureId)
            return Result.NotApplicable;

        var reason = Params[1]?.ToString() ?? BANISH_REASON;

        accountRepository.Ban(player.AccountId, reason, ((IPlayer)caster).AccountId).Wait();
        playerLogOutCommand.Execute(player, true);

        return Result.Success;
    }
}