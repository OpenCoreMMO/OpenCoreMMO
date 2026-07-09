using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Locker;
using NeoServer.Domain.Repositories;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Configurations;
using Serilog;
using IPlayerRepository = NeoServer.Data.Interfaces.IPlayerRepository;

namespace NeoServer.Server.Routines.Persistence;

public class PlayerPersistenceRoutine(
    IGameServer gameServer,
    IPlayerRepository playerRepository,
    ILogger logger,
    IPlayerDepotRepository playerDepotItemRepository,
    IPlayerMailItemRepository playerMailItemRepository,
    IScriptManager scriptManager,
    ServerConfiguration serverConfiguration,
    LockerManager lockerManager)
{
    private readonly Stopwatch _stopwatch = new();

    private int _saveInterval;

    public void Start(CancellationToken token)
    {
        _saveInterval = (int)(serverConfiguration?.Save?.Players ?? (uint)_saveInterval);
        _saveInterval = (_saveInterval == 0 ? 3600 : _saveInterval) * 1000;
        Task.Factory.StartNew(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_saveInterval, token);
                gameServer.PersistenceDispatcher.AddEvent(async () => await SavePlayers());
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public async Task SavePlayers()
    {
        var players = gameServer.CreatureManager.GetAllLoggedPlayers().ToList();

        if (players.Count != 0)
        {
            logger.Information("Saving {NumPlayers} players...", players.Count);
            _stopwatch.Restart();

            await playerRepository.UpdatePlayers(players);

            await SaveDepots(players);
            await SaveMailInboxes(players);

            logger.Information("{NumPlayers} players saved in {Elapsed} ms", players.Count,
                _stopwatch.ElapsedMilliseconds);
        }

        scriptManager.GlobalEvents.ExecuteSave();
    }

    private async Task SaveDepots(List<IPlayer> players)
    {
        var depotSaveTasks = new List<Task>();

        foreach (var player in players)
        {
            if (!lockerManager.Get(player.Id, out var locker)) continue;

            var depotChest = locker.Items.FirstOrDefault() as IContainer;

            depotSaveTasks.Add(playerDepotItemRepository.Save(player, depotChest));
        }

        await Task.WhenAll(depotSaveTasks);
    }

    private async Task SaveMailInboxes(List<IPlayer> players)
    {
        var mailInboxSaveTasks = new List<Task>();

        foreach (var player in players)
        {
            if (!lockerManager.Get(player.Id, out var locker)) continue;

            var mailInbox = locker.Items.ElementAtOrDefault(1) as IContainer;

            mailInboxSaveTasks.Add(playerMailItemRepository.Save(player, mailInbox));
        }

        await Task.WhenAll(mailInboxSaveTasks);
    }
}