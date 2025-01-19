using MediatR;
using NeoServer.Data.Entities;
using NeoServer.Web.API.Response;
using Type = NeoServer.Data.Entities.Type;

namespace NeoServer.Web.API.Requests.Commands;

public class CreateWorldRequest : IRequest<OutputResponse>
{
    public string Name { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; }
    
    public Region Region { get; set; }
    
    public PvpType PvpType { get; set; }
    
    public Type Type { get; set; }
    
    public bool RequiresPremium { get; set; }
    
    public bool TransferEnabled { get; set; }
    
    public bool AntiCheatEnabled { get; set; }
    public int MaxCapacity { get; set; }
}