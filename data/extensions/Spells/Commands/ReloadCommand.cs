using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Common.Services;
using NeoServer.Game.Items.Factories;
using NeoServer.Loaders.Quest;
using NeoServer.Loaders.Vocations;
using NeoServer.Scripts.Lua;
using NeoServer.Scripts.Lua.Functions;
using NeoServer.Server.Helpers;
using NLua;

namespace NeoServer.Extensions.Spells.Commands;

public class ReloadCommand : CommandSpell
{
    private static Dictionary<string, Action> Modules => new()
    {
        ["vocations"] = IoC.GetInstance<VocationLoader>().Reload,
        ["lua"] = IoC.GetInstance<LuaGlobalRegister>().Register,
        ["quests"] = () =>
        {
            IoC.GetInstance<QuestDataLoader>().Load();
            QuestFunctions.RegisterQuests(IoC.GetInstance<Lua>());
        }
    };

    private IItem Item(ICombatActor actor, int amount)
    {
        if (ushort.TryParse(Params[0].ToString(), out var typeId))
            return ItemFactory.Instance.Create(typeId, actor.Location,
                new Dictionary<ItemAttribute, IConvertible> { { ItemAttribute.Count, amount } });

        var item = ItemFactory.Instance.Create(Params[0].ToString(), actor.Location,
            new Dictionary<ItemAttribute, IConvertible> { { ItemAttribute.Count, amount } });

        return item;
    }

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (Params is null || !Params.Any())
        {
            OperationFailService.Send(caster.CreatureId, "Invalid module");
            return Result.NotApplicable;
        }

        Console.Write(Params.Length);

        var module = Params[0].ToString();

        if (!Modules.TryGetValue(module, out var action))
        {
            OperationFailService.Send(caster.CreatureId, "Invalid module");
            return Result.NotApplicable;
        }

        action?.Invoke();
        return Result.Success;
    }
}