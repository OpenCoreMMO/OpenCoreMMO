using Mediator;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Combat;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Weapons;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Results;
using NeoServer.Modules.Combat.Attacks.DistanceAttack;
using NeoServer.Modules.Combat.Attacks.MeleeAttack;

namespace NeoServer.Modules.Combat.PlayerAttack.WeaponAttack;

public record PlayerWeaponAttackCommand(IPlayer Player, IThing Victim, AttackParameter AttackParameter)
    : ICommand<Result>;

public class PlayerWeaponAttackCommandHandler(
    MeleeAttackStrategy meleeAttackStrategy,
    DistanceAttackStrategy distanceAttackStrategy,
    GameConfiguration gameConfiguration,
    IEventBus eventBus)
    : ICommandHandler<PlayerWeaponAttackCommand, Result>
{
    public ValueTask<Result> Handle(PlayerWeaponAttackCommand command, CancellationToken cancellationToken)
    {
        var (player, victim, attackParameter) = command;

        if (!player.CooldownHasExpired(CooldownType.Combat)) return ValueTask.FromResult(Result.NotPossible);

        ExecuteDistanceAttackIfApplicable(player, victim, attackParameter);
        ExecuteMeleeAttackIfApplicable(player, victim, attackParameter);

        return ValueTask.FromResult(Result.Success);
    }

    private static void IncreaseSkill(IPlayer player)
    {
        if (player.Inventory.Weapon is IMagicalWeapon) return;
        player.IncreaseSkillCounter(player.SkillInUse, 1);
    }

    private void ExecuteMeleeAttackIfApplicable(IPlayer player, IThing victim, AttackParameter attackParameter)
    {
        if (player.UsingDistanceWeapon) return;

        var result = meleeAttackStrategy.Execute(new AttackInput(player, victim)
        {
            Parameters = attackParameter
        });

        if (result.Failed) return;

        if (player.Inventory.Weapon is IMagicalWeapon magicalWeapon) player.ConsumeMana(magicalWeapon.ManaConsumption);

        IncreaseSkill(player);
    }

    private void ExecuteDistanceAttackIfApplicable(IPlayer player, IThing victim, AttackParameter attackParameter)
    {
        if (!player.UsingDistanceWeapon) return;

        var result = distanceAttackStrategy.Execute(new AttackInput(player, victim)
        {
            Parameters = attackParameter
        });

        if (result.Failed) return;

        if (result.Succeeded) ConsumeAmmo(player);
        IncreaseSkill(player);
    }

    private void ConsumeAmmo(ICombatActor aggressor)
    {
        if (aggressor is not IPlayer player) return;

        if (player.Inventory.Weapon is IDistanceWeapon && !gameConfiguration.Combat.InfiniteAmmo)
            player.Inventory.Ammo?.Reduce();

        if (player.Inventory.Weapon is IThrowableWeapon throwableDistanceWeapon &&
            throwableDistanceWeapon.ShouldBreak() &&
            !gameConfiguration.Combat.InfiniteThrowingWeapon)
            throwableDistanceWeapon.Reduce();
    }
}