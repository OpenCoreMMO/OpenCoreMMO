#define GAME_FEATURE_MESSAGE_LEVEL
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Guild;
using NeoServer.Domain.Party;
using System.Text;
using NeoServer.Domain.Creatures.Monster.Summon;

namespace NeoServer.Domain.Creatures;

public static class CreatureRaw
{
    public static byte[] Convert(IPlayer playerRequesting, IWalkableCreature creature)
    {
        //optimize this method
        var cache = new List<byte>();

        //todo: 1098 implements this?
        //enum CreatureType_t : uint8_t
        //{
        //    CREATURETYPE_PLAYER = 0,
        //    CREATURETYPE_MONSTER = 1,
        //    CREATURETYPE_NPC = 2,
        //    CREATURETYPE_SUMMON_OWN = 3,
        //    CREATURETYPE_SUMMON_OTHERS = 4,
        //};

        var creatureType = (byte)0x00;

        if (creature is IPlayer)
            creatureType = (byte)0x00;
        else if (creature is IMonster)
            creatureType = (byte)0x01;
        else if (creature is INpc)
            creatureType = (byte)0x02;
        else if (creature is Summon summon)
        {
            if (summon.Master.CreatureId == playerRequesting.CreatureId)
                creatureType = (byte)0x01;
            else
                creatureType = (byte)0x04;
        }

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

            cache.Add(creatureType);

            var creatureNameBytes = Encoding.Default.GetBytes(creature.Name);
            cache.AddRange(BitConverter.GetBytes((ushort)creatureNameBytes.Length));
            cache.AddRange(creatureNameBytes);
        }

        cache.Add((byte)Math.Ceiling((double)creature.HealthPoints / Math.Max(creature.MaxHealthPoints, 1) * 100));
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
                cache.AddRange(BitConverter.GetBytes((ushort)0)); //todo: implements this creature.Outfit.LookTypeEx
            }

            cache.AddRange(BitConverter.GetBytes((ushort)0));//todo: 1098 implements this mounts
        }
        else
        {
            cache.AddRange(BitConverter.GetBytes((ushort)0)); //LookType
            cache.AddRange(BitConverter.GetBytes((ushort)0)); //LookTypeEx
            cache.AddRange(BitConverter.GetBytes((ushort)0)); //Mount
        }

        cache.Add(creature.LightLevel);
        cache.Add(creature.LightColor);

        cache.AddRange(BitConverter.GetBytes((ushort)(creature.Speed / 2)));

        cache.Add((byte)((creature as IPlayer)?.Skull ?? 0x00));
        cache.Add((byte)GetPartyEmblem(playerRequesting, creature));

        if (!known)
        {
            if (creature is IPlayer playerUnknow)
            {
                if (playerUnknow.GuildId == 0) cache.Add((byte)GuildEmblem.None); //guild emblem
                else if (playerRequesting.GuildId == playerUnknow.GuildId) cache.Add((byte)GuildEmblem.Ally);
                else cache.Add((byte)GuildEmblem.Neutral); //guild emblem
            }
            else
            {
                cache.Add((byte)GuildEmblem.None);
            }
        }

        cache.Add(creatureType);

        cache.Add(0x00); //todo: 1098 implements this //getSpeechBubble
        cache.Add(0xFF); //todo: 1098 implements this // MARK_UNMARKED

        //todo: 1098 implements this
        if (creature is IPlayer player && player.CreatureId != playerRequesting.CreatureId)
        {
            cache.AddRange(BitConverter.GetBytes((ushort)(0))); //todo: 1098 implement this Player::getHelpers()
        }
        else
        {
            cache.AddRange(BitConverter.GetBytes((ushort)(0)));
        }

        //todo: 1098 implements this
        cache.Add(0x01); //msg.addByte(player->canWalkthroughEx(creature) ? 0x00 : 0x01);

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