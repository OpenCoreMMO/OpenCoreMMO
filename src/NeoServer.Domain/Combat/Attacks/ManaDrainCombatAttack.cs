namespace NeoServer.Domain.Combat.Attacks;

//public class ManaDrainCombatAttack : DistanceCombatAttack
//{
//    public ManaDrainCombatAttack(CombatAttackOption option) : base(DamageType.ManaDrain, option)
//    {
//        option.Target = 1;
//    }

//    public void Damage(IPlayer enemy)
//    {
//        var damage = CalculateDamage(enemy.AttackPower, enemy.MinimumAttackPower);
//        enemy.ReceiveManaAttack(enemy, damage);
//    }
//}