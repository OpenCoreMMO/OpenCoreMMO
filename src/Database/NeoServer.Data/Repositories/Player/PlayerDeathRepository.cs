using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using Serilog;

namespace NeoServer.Data.Repositories.Player;

public class PlayerDeathRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger)
    : BaseRepository<PlayerDeathEntity>(contextOptions,
        logger), IPlayerDeathRepository
{
    public IEnumerable<PlayerDeathEntity> GetPlayerKills(int playerId)
    {
        using var context = NewDbContext;

        return context.PlayerDeathKillers
            .Include(x => x.PlayerDeath)
            .Where(x => x.PlayerId == playerId)
            .Select(x => x.PlayerDeath);
    }

    public void Save(IPlayer deadPlayer, IThing killedBy)
    {
        using var context = NewDbContext;

        var playerDeath = new PlayerDeathEntity
        {
            PlayerId = (int) deadPlayer.Id,
            Level = deadPlayer.Level,
            DeathDateTime = DateTime.Now,
            DeathLocation = deadPlayer.Location.ToString(CultureInfo.InvariantCulture),
            Unjustified = true,
            //Killers = 
        };
    }
}