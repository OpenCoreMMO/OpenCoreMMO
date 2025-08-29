using System;
using System.Text;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items;
using NeoServer.Domain.Spells;
using NeoServer.Domain.Spells.Entities;
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
        window.WrittenOn = DateTime.UtcNow;

        player.Read(window);

        return Result.Success;
    }
}