using System;
using System.Text;
using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Items;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class SpyPlayerCommand : CommandSpell
{
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        if (Params.Length == 0)
            return Result.NotApplicable;

        var ctx = IoC.GetInstance<IGameCreatureManager>();

        if (!ctx.TryGetPlayer(Params[0].ToString(), out var player))
            return Result.NotApplicable;

        var stringBuilder = new StringBuilder(1000);

        stringBuilder.AppendLine($"*** Name: {player.Name} *****");

        foreach (var inventoryDressingItem in player.Inventory.DressingItems)
            stringBuilder.AppendLine(
                $"ClientId: {inventoryDressingItem.Metadata.ClientId}-{inventoryDressingItem.FullName}");

        var item = new ItemType();
        item.SetClientId(2821);

        var window = new ListCommandsCommand.TextWindow(item, player.Location, stringBuilder.ToString());
        var serverConfiguration = IoC.GetInstance<ServerConfiguration>();

        window.WrittenBy = $"{serverConfiguration.ServerName} - SERVER";
        window.WrittenOn = DateTime.Now;

        player.Read(window);

        return Result.Success;
    }
}