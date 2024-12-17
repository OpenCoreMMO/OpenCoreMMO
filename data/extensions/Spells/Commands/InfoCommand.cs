using System;
using System.Linq;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Items;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class InfoCommand : CommandSpell
{
    public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
    {
        error = InvalidOperation.NotPossible;

        if (Params.Length != 1)
            return false;

        var ctx = IoC.GetInstance<IGameCreatureManager>();
        ctx.TryGetPlayer(Params[0].ToString(), out var target);

        if (target is null)
            return false;

        var info = CreateItemInfo();

        var message =
            $"""
             AccountId: {target.AccountId}
             Position: {target.Location.X}, {target.Location.Y}, {target.Location.Z}
             Capacity: {target.TotalCapacity}
             PremiumTime: {target.PremiumTime}
             Level: {target.Level}
             Skills:
             {target.Skills.Where(item => item.Key != SkillType.Level).Select(Item => "   * " + Item.Key + ": " + Item.Value.Level + "\n").Aggregate((a, b) => a + b)}
             """;

        var window = new ListCommandsCommand.TextWindow(info, target.Location, message);
        var serverConfiguration = IoC.GetInstance<ServerConfiguration>();

        window.WrittenBy = $"{serverConfiguration.ServerName} - SERVER";
        window.WrittenOn = DateTime.Now;

        var player = actor as IPlayer;

        player.Read(window);

        return true;
    }

    private static IItemType CreateItemInfo()
    {
        var item = new ItemType();
        item.SetName("Info Status");
        item.SetClientId(2821);
        return item;
    }
}