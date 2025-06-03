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
}