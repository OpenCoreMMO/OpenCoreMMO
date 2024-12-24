using NeoServer.Data.Entities;
using NeoServer.Game.Chats;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Player;
using NeoServer.Game.World;
using NeoServer.Loaders.Players;
using Serilog;

namespace NeoServer.Extensions.Players.Loaders;

public class GodLoader : PlayerLoader
{
    public GodLoader(IItemFactory itemFactory, ICreatureFactory creatureFactory,
        ChatChannelFactory chatChannelFactory, IGuildStore guildStore,
        IVocationStore vocationStore, IMapTool mapTool, World world, ILogger logger,
        GameConfiguration gameConfiguration) :
        base(itemFactory, creatureFactory, chatChannelFactory, guildStore,
            vocationStore, mapTool, world, logger, gameConfiguration)
    {
    }

    public override bool IsApplicable(PlayerEntity player)
    {
        return player?.PlayerType == 3;
    }

    public override IPlayer Load(PlayerEntity playerEntity)
    {
        if (Guard.IsNull(playerEntity)) return null;

        var town = GetTown(playerEntity);

        var newPlayer = new God(
            (uint)playerEntity.Id,
            playerEntity.Name,
            VocationStore.Get(playerEntity.Vocation),
            playerEntity.Gender,
            playerEntity.Online,
            ConvertToSkills(playerEntity),
            new Outfit
            {
                Addon = (byte)playerEntity.LookAddons,
                Body = (byte)playerEntity.LookBody,
                Feet = (byte)playerEntity.LookFeet,
                Head = (byte)playerEntity.LookHead,
                Legs = (byte)playerEntity.LookLegs,
                LookType = (byte)playerEntity.LookType
            },
            playerEntity.Speed,
            new Location((ushort)playerEntity.PosX, (ushort)playerEntity.PosY, (byte)playerEntity.PosZ),
            MapTool,
            town)
        {
            AccountId = (uint)playerEntity.AccountId,
            Guild = GuildStore.Get((ushort)(playerEntity.GuildMember?.GuildId ?? 0)),
            GuildLevel = (ushort)(playerEntity.GuildMember?.RankId ?? 0)
        };

        //SetCurrentTile(newPlayer);
        var currentTile = GetCurrentTile(newPlayer.Location);
        newPlayer.SetCurrentTile(currentTile);

        newPlayer.AddInventory(ConvertToInventory(newPlayer, playerEntity));
        var god = CreatureFactory.CreatePlayer(newPlayer);

        return god;
    }
}