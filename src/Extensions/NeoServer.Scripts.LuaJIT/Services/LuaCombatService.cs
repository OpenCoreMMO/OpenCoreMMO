using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Models.Combat;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT.Services;

public class LuaCombatService(
    NonAggressiveCombatService nonAggressiveCombatService,
    IGameCreatureManager gameCreatureManager,
    IMap map,
    IAttackService attackService)
{
    public void Execute(LuaCombat combat, ICreature actor, LuaVariant variant)
    {
        IThing target = null;

        //if variant is a number, get the target creature
        if (variant.Type == LuaVariantType.VARIANT_NUMBER)
        {
            gameCreatureManager.TryGetCreature(variant.Number, out var targetCreature);
            target = targetCreature;
        }

        //if variant is a position, get the target tile
        if (variant.Type == LuaVariantType.VARIANT_POSITION) target = map.GetTile(variant.Pos) ?? new EmptyTile(variant.Pos);

        //if combat is not aggressive, execute non-aggressive combat
        if (combat.Parameters.TryGetValue(CombatParam.COMBAT_PARAM_AGGRESSIVE, out var aggressive) && aggressive == 0)
        {
            nonAggressiveCombatService.Execute(combat, actor, target);
            return;
        }

        //execute aggressive combat
        var combatParameter = combat.BuildCombatParameter(actor as IPlayer, target);
        attackService.Execute(new AttackInput(actor, target, combatParameter)
        {
            
        });
    }
}