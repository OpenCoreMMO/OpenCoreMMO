// using NeoServer.Game.Combat.Attacks;
// using NeoServer.Game.Combat.Spells;
// using NeoServer.Game.Common;
// using NeoServer.Game.Common.Contracts.Creatures;
// using NeoServer.Game.Common.Item;
//
// namespace NeoServer.Extensions.Spells.Attack;
//
// public class TerraStrike : AttackSpell
// {
//     public override DamageType DamageType => DamageType.Earth;
//     public override CombatAttack CombatAttack => new DistanceCombatAttack(Range, ShootType.Earth);
//     public override byte Range => 5;
//
//     public override MinMax CalculateDamage(ICombatActor actor)
//     {
//         return new MinMax(5, 100);
//     }
// }