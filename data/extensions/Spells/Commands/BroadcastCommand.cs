using System;
using System.Text.RegularExpressions;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Spells.Entities;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

namespace NeoServer.Extensions.Spells.Commands;

public class BroadcastCommand : CommandSpell
{
    private TextMessageOutgoingType GetTextMessageOutgoingTypeFromColor(string color)
    {
        return color switch
        {
            "white" => TextMessageOutgoingType.MESSAGE_EVENT_DEFAULT,
            "red" => TextMessageOutgoingType.MESSAGE_STATUS_WARNING,
            "green" => TextMessageOutgoingType.Description,
            _ => TextMessageOutgoingType.Description
        };
    }

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        var ctx = IoC.GetInstance<IGameCreatureManager>();

        if (Params.Length > 0)
        {
            var regex = new Regex("^(\\w+).\"(.+)\"$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
            var match = regex.Match(Params[0].ToString());

            if (match.Groups.Count == 3)
            {
                var (color, message) = (match.Groups[1].Value, match.Groups[2].Value);

                foreach (var player in ctx.GetAllLoggedPlayers())
                {
                    if (player is null)
                        continue;

                    if (ctx.GetPlayerConnection(player.CreatureId, out var connection) is false) continue;

                    connection.OutgoingPackets.Enqueue(new TextMessagePacket(message,
                        GetTextMessageOutgoingTypeFromColor(color)));
                    connection.Send();
                }

                return Result.Success;
            }
        }

        return Result.NotPossible;
    }
}