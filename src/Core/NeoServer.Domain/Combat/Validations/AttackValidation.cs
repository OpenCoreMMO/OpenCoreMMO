using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Domain.Creatures.Player;

namespace NeoServer.Domain.Combat.Validations;

public class AttackValidation(
    IMapTool mapTool,
    IMap map,
    PvPConfiguration pvpConfiguration,
    CombatConfiguration combatConfiguration)
{
    private static readonly HashSet<InvalidOperation> OperationsThatStopAttack =
    [
        InvalidOperation.YouMayNotAttackThisPlayer,
        InvalidOperation.NotPermittedInNoPvpZone,
        InvalidOperation.CannotAttackPersonInProtectionZone,
        InvalidOperation.YouMayNotAttackThisCreature,
        InvalidOperation.CannotAttackWhileInProtectionZone,
        InvalidOperation.TargetLost
    ];

    public Result Validate(AttackInput attackInput)
    {
        var aggressor = attackInput.Aggressor as ICombatActor;
        var target = attackInput.Target;

        if (Guard.IsNull(aggressor))
            return Result.NotPossible;

        switch (target)
        {
            case IPlayer targetPlayer:
            {
                if (targetPlayer.Group.FlagIsEnabled(PlayerFlag.CannotBeAttacked))
                    return Result.Fail(InvalidOperation.YouMayNotAttackThisPlayer);

                switch (aggressor)
                {
                    //Player cannot attack a player
                    case IPlayer aggressorPlayer
                        when aggressorPlayer.Group.FlagIsEnabled(PlayerFlag.CannotAttackPlayer) &&
                             aggressorPlayer != targetPlayer:
                        return Result.Fail(InvalidOperation.YouMayNotAttackThisPlayer);

                    //Player cannot attack player in no pvp zone
                    case IPlayer aggressorPlayer when IsProtected(aggressorPlayer, targetPlayer):
                        return Result.Fail(InvalidOperation.YouMayNotAttackThisPlayer);

                    //Player cannot attack player in no pvp zone or protection zone
                    case IPlayer aggressorPlayer when aggressorPlayer.Tile.NoPvpZone &&
                                                      targetPlayer.Tile.HasFlag(TileFlags.NoPvpZone |
                                                          TileFlags.ProtectionZone):
                        return Result.Fail(InvalidOperation.NotPermittedInNoPvpZone);

                    case Summon { Master: IPlayer masterPlayer }
                        when masterPlayer.Group.FlagIsEnabled(PlayerFlag.CannotAttackPlayer) ||
                             IsProtected(masterPlayer, targetPlayer):
                        return Result.Fail(InvalidOperation.YouMayNotAttackThisPlayer);
                }

                break;
            }
            case IMonster monsterTarget:
                switch (aggressor)
                {
                    //Player cannot attack his own summons when can attack summon configuration is disabled
                    case IPlayer master when monsterTarget is Summon { Master: IPlayer summonMaster } &&
                                             master.Equals(summonMaster) && !combatConfiguration.CanAttackOwnSummon:
                        return Result.Fail(InvalidOperation.YouMayNotAttackThisCreature);

                    //Player cannot attack another player's summon if protection level applies
                    case IPlayer playerAggressor when monsterTarget is Summon { Master: IPlayer summonMaster } &&
                                                      !playerAggressor.Equals(summonMaster) &&
                                                      IsProtected(playerAggressor, summonMaster):
                        return Result.Fail(InvalidOperation.YouMayNotAttackThisCreature);

                    //Player cannot attack a monster
                    case IPlayer playerAggressor
                        when playerAggressor.Group.FlagIsEnabled(PlayerFlag.CannotAttackMonster):
                        return Result.Fail(InvalidOperation.YouMayNotAttackThisCreature);
                    //Player cannot attack monster in no pvp zone
                    case IPlayer when monsterTarget is Summon { Master: IPlayer } &&
                                      (monsterTarget.Tile?.NoPvpZone ?? false):
                        return Result.Fail(InvalidOperation.NotPermittedInNoPvpZone);
                    //Monster cannot attack another monster or summons monster
                    case IMonster monsterAggressor
                        when monsterTarget is Summon { Master: IMonster }:
                        return Result.Fail(InvalidOperation.YouMayNotAttackThisCreature);
                }

                break;
        }

        if (pvpConfiguration.PvpType == PvpType.OptionalPvP)
            if (!Equals(aggressor, target) && aggressor is IPlayer or Summon { Master: IPlayer } &&
                target is IPlayer or Summon { Master: IPlayer })
                if (!aggressor.Tile.PvpZone || !((ICreature)target).Tile.PvpZone)
                {
                    // Determine the appropriate error based on target type
                    var operation = target is Summon
                        ? InvalidOperation.YouMayNotAttackThisCreature
                        : InvalidOperation.YouMayNotAttackThisPlayer;
                    return Result.Fail(operation);
                }

        var attackValidationResult = aggressor.CanAttack(attackInput.Parameters);
        if (attackValidationResult.Failed) return attackValidationResult;

        if (!attackInput.HasTarget) return Result.Success;

        if (!aggressor.CanSee(target.Location) || !aggressor.Location.SameFloorAs(target.Location))
        {
            return Result.Fail(InvalidOperation.TargetLost);
        }

        // Extra check for area attacks to ensure visibility of creatures
        if (!attackInput.Parameters.IsAttackInArea && target is ICreature creatureTarget && !aggressor.CanSee(creatureTarget))
        {
            return Result.Fail(InvalidOperation.TargetLost);
        }

        switch (target)
        {
            case ICombatActor { IsDead: true }:
                return Result.Fail(InvalidOperation.CreatureIsDead);
            case ICombatActor victim when victim.Tile?.ProtectionZone ?? false:
            case ITile { ProtectionZone: true }:
                return Result.Fail(InvalidOperation.CannotAttackPersonInProtectionZone);
            case IItem item:
            {
                var tile = map[item.Location];
                if (tile.ProtectionZone) return Result.Fail(InvalidOperation.CannotAttackPersonInProtectionZone);
                break;
            }
        }

        if (mapTool.SightClearChecker?.Invoke(aggressor.Location, target.Location, true) is false)
            return Result.Fail(InvalidOperation.CannotThrowThere);

        return Result.Success;
    }

    public static bool ShouldStopAttackOnValidationFailure(InvalidOperation operation)
    {
        return OperationsThatStopAttack.Contains(operation);
    }

    private bool IsProtected(IPlayer playerAggressor, IPlayer playerTarget)
    {
        if (playerAggressor.Level < pvpConfiguration.ProtectionLevel ||
            playerTarget.Level < pvpConfiguration.ProtectionLevel)
            return true;

        if (playerAggressor.VocationType == 0 || playerTarget.VocationType == 0) return true;

        if (playerAggressor.Skull is Skull.Black && playerAggressor.GetSkull(playerTarget) is Skull.None) return true;

        return false;
    }
}