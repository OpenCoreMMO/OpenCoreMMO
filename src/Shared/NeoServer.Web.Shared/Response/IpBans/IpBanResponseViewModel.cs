using NeoServer.Data.Entities;

namespace NeoServer.Web.API.Response.IpBans;

[Serializable]
public class IpBanResponseViewModel
{
    public int Id { get; set; }
    public string Ip { get; set; }
    public string Reason { get; set; }
    public DateTime BannedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ushort BannedBy { get; set; }
    
    public static implicit operator IpBanResponseViewModel(IpBanEntity entity) 
        => entity == null 
        ? null 
        : new IpBanResponseViewModel
    {
        Id = entity.Id,
        Ip = entity.Ip,
        Reason = entity.Reason,
        BannedAt = entity.BannedAt,
        ExpiresAt = entity.ExpiresAt,
        BannedBy = entity.BannedBy
    };
}