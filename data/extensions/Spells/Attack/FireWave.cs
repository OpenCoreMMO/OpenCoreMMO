// using NeoServer.Game.Common;
// using NeoServer.Game.Common.Contracts.Creatures;
// using NeoServer.Game.Common.Item;
//
// namespace NeoServer.Extensions.Spells.Attack;
//
// public class FireWave : WaveSpell
// {
//     protected override string AreaName => "AREA_WAVE4";
//     public override DamageType DamageType => DamageType.Fire;
//     public override byte Range => 1;
//
//     public override MinMax CalculateDamage(ICombatActor actor)
//     {
//         return new MinMax(5, 100);
//     }
// }