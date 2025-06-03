using System;
using System.Linq;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Items;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class InfoCommand : CommandSpell
{
    private static IItemType CreateItemInfo()
    {
        var item = new ItemType();
        item.UpdateName("Info Status");
        item.SetClientId(2821);
        return item;
    }

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (Params.Length != 1)
            return Result.Success;

        var ctx = IoC.GetInstance<IGameCreatureManager>();
        ctx.TryGetPlayer(Params[0].ToString(), out var targetPlayer);

        if (targetPlayer is null)
            return Result.NotApplicable;

        var info = CreateItemInfo();

        var message =
            $"""
             AccountId: {targetPlayer.AccountId}
             Position: {targetPlayer.Location.X}, {targetPlayer.Location.Y}, {targetPlayer.Location.Z}
             Capacity: {targetPlayer.TotalCapacity}
             PremiumTime: {targetPlayer.PremiumTime}
             Level: {targetPlayer.Level}
             Skills:
             {targetPlayer.Skills.Where(item => item.Key != SkillType.Level).Select(Item => "   * " + Item.Key + ": " + Item.Value.Level + "\n").Aggregate((a, b) => a + b)}
             """;

        var window = new ListCommandsCommand.TextWindow(info, targetPlayer.Location, message);
        var serverConfiguration = IoC.GetInstance<ServerConfiguration>();

        window.WrittenBy = $"{serverConfiguration.ServerName} - SERVER";
        window.WrittenOn = DateTime.Now;

        var player = caster as IPlayer;

        player.Read(window);

        return Result.Success;
    }
}