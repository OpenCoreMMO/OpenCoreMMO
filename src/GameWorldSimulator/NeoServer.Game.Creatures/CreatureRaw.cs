#define GAME_FEATURE_MESSAGE_LEVEL
using System;
using System.Collections.Generic;
using System.Text;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Party;

namespace NeoServer.Game.Creatures;

public static class CreatureRaw
{
    public static byte[] Convert(IPlayer playerRequesting, IWalkableCreature creature)
    {
        //optimize this method
        var cache = new List<byte>();

        var known = playerRequesting.KnowsCreatureWithId(creature.CreatureId);

        if (known)
        {
            cache.AddRange(BitConverter.GetBytes((ushort)0x62));
            cache.AddRange(BitConverter.GetBytes(creature.CreatureId));
        }
        else
        {
            playerRequesting.AddKnownCreature(creature.CreatureId);

            cache.AddRange(BitConverter.GetBytes((ushort)0x61));
            cache.AddRange(BitConverter.GetBytes(playerRequesting.ChooseToRemoveFromKnownSet()));
            cache.AddRange(BitConverter.GetBytes(creature.CreatureId));

            var creatureNameBytes = Encoding.Default.GetBytes(creature.Name);
            cache.AddRange(BitConverter.GetBytes((ushort)creatureNameBytes.Length));
            cache.AddRange(creatureNameBytes);
        }

        cache.Add((byte)Math.Min(100, creature.HealthPoints * 100 / creature.MaxHealthPoints));
        cache.Add((byte)creature.SafeDirection);

        if (playerRequesting.CanSee(creature))
        {
            // Add creature outfit
            cache.AddRange(BitConverter.GetBytes(creature.Outfit.LookType));

            if (creature.Outfit.LookType > 0)
            {
                cache.Add(creature.Outfit.Head);
                cache.Add(creature.Outfit.Body);
                cache.Add(creature.Outfit.Legs);
                cache.Add(creature.Outfit.Feet);
                cache.Add(creature.Outfit.Addon);
            }
            else
            {
                cache.AddRange(BitConverter.GetBytes(creature.Outfit.LookType));
            }
        }
        else
        {
            cache.AddRange(BitConverter.GetBytes((ushort)0));
            cache.AddRange(BitConverter.GetBytes((ushort)0));
        }

        cache.Add(creature.LightBrightness);
        cache.Add(creature.LightColor);

        cache.AddRange(BitConverter.GetBytes(creature.Speed));

        cache.Add((byte)((creature as IPlayer)?.Skull ?? 0x00));
        cache.Add((byte)GetPartyEmblem(playerRequesting, creature));

        if (!known)
        {
            if (creature is IPlayer player)
            {
                if (player.GuildId == 0) cache.Add((byte)GuildEmblem.None); //guild emblem
                else if (playerRequesting.GuildId == player.GuildId) cache.Add((byte)GuildEmblem.Ally);
                else cache.Add((byte)GuildEmblem.Neutral); //guild emblem
            }
            else
            {
                cache.Add((byte)GuildEmblem.None);
            }
        }

        cache.Add(creature is IPlayer ? (byte)0x00 : (byte)0x01);

        return cache.ToArray();
    }

    private static PartyEmblem GetPartyEmblem(IPlayer playerRequesting, IWalkableCreature creature)
    {
        if (creature is not IPlayer player) return PartyEmblem.None;

        if (playerRequesting.PlayerParty.Party?.IsInvited(player) ?? false) return PartyEmblem.Invited;

        if (player.PlayerParty.Party is not IParty party) return PartyEmblem.None;

        if (playerRequesting.PlayerParty.Party is not null && party != playerRequesting.PlayerParty.Party)
            return PartyEmblem.NotFromYourParty;

        if (party.IsLeader(player))
        {
            if (party.IsInvited(playerRequesting)) return PartyEmblem.LeaderInvited;
            if (party.IsMember(playerRequesting)) return PartyEmblem.Leader;
            return PartyEmblem.None;
        }

        if (playerRequesting.PlayerParty.Party is null) return PartyEmblem.None;
        return PartyEmblem.Member;
    }
}