using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Helpers;

using System;
using System.Text.RegularExpressions;

namespace NeoServer.Extensions.Spells.Commands
{
    public class BroadcastCommand : CommandSpell
    {
        public override bool OnCast(ICombatActor actor, string words, out InvalidOperation error)
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

                        connection.OutgoingPackets.Enqueue(new TextMessagePacket(message, GetTextMessageOutgoingTypeFromColor(color)));
                        connection.Send();
                    }

                    error = InvalidOperation.None;
                    return true;
                }
            }

            error = InvalidOperation.NotPossible;
            return false;
        }

        private TextMessageOutgoingType GetTextMessageOutgoingTypeFromColor(string color)
        {
            return color switch
            {
                "white" => TextMessageOutgoingType.MESSAGE_EVENT_LEVEL_CHANGE,
                "red" => TextMessageOutgoingType.MESSAGE_STATUS_WARNING,
                "green" => TextMessageOutgoingType.Description,
                _ => TextMessageOutgoingType.Description
            };
        }
    }
}
