using NeoServer.Data.Entities;

namespace NeoServer.Web.API.Response.World;

[Serializable]
public class WorldResponseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Ip { get; set; }
    public int Port { get; set; }
    
    public Continent Continent { get; set; }
    
    public PvpType PvpType { get; set; }
    
    public Mode Mode { get; set; }
    
    public bool RequiresPremium { get; set; }
    
    public bool TransferEnabled { get; set; }
    
    public bool AntiCheatEnabled { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int MaxCapacity { get; set; }
}