using NeoServer.Data.Interfaces;
using NeoServer.Networking.Packets.Incoming.Chat;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Chat;

public class PlayerRemoveVipHandler : PacketHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly IGameServer _game;

    public PlayerRemoveVipHandler(IGameServer game, IAccountRepository accountRepository)
    {
        _game = game;
        _accountRepository = accountRepository;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        var removeVipPacket = new RemoveVipPacket(message);

        _game.Dispatcher.AddEvent(new Event(() => player.Vip.RemoveFromVip(removeVipPacket.PlayerId)));

        _accountRepository.RemoveFromVipList((int)player.AccountId, (int)removeVipPacket.PlayerId);
    }
}