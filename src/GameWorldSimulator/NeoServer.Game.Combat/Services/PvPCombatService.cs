using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Combat.Services;

public class PvPCombatService: IPvpCombatService
{
    public PvPCombatService()
    {
    }
    
    public Result Attack(IPlayer aggressor, IPlayer victim)
    {
        if (Guard.AnyNull(aggressor, victim)) return Result.NotPossible;

        aggressor.Attack(victim);

        if (victim.HasSkull)
        {
            aggressor.AddPlayerToEnemyList(victim);
        }
        
        UpdateSkull(aggressor, enemy: victim);
        return Result.Success;
    }
    
    private void UpdateSkull(IPlayer aggressor, IPlayer enemy)
    {
        if (enemy.HasSkull)
        {
            if (aggressor.Skull is Skull.White) aggressor.SetSkull(Skull.White);

            if (!aggressor.HasSkull) aggressor.SetSkull(Skull.Yellow);
        }

        if (!enemy.HasSkull)
        {
            aggressor.SetSkull(Skull.White);
        }
    }
}

public interface IPvpCombatService
{
    Result Attack(IPlayer aggressor, IPlayer victim);
}
