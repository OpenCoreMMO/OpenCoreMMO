using MediatR;
using NeoServer.Data.Entities;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Web.API.Response;

namespace NeoServer.Web.API.Requests.Commands;

public class UpdatePlayerInfosRequest : IRequest<OutputResponse>, ICommandBase
{
    public int Id { get; private set; }
    public int TownId { get; set; }
    public string Name { get; set; }
    public byte Group { get; set; }
    public uint Capacity { get; set; }
    public ushort Level { get; set; }
    public ushort Mana { get; set; }
    public ushort MaxMana { get; set; }
    public uint Health { get; set; }
    public uint MaxHealth { get; set; }
    public byte Soul { get; set; }
    public byte MaxSoul { get; set; }
    public ushort Speed { get; set; }
    public ChaseMode ChaseMode { get; set; }
    public FightMode FightMode { get; set; }
    public Gender Gender { get; set; }
    public byte Vocation { get; set; }
    public int WorldId { get; set; }
    
    public void SetPlayerId(int id)
    {
        Id = id;
    }
}