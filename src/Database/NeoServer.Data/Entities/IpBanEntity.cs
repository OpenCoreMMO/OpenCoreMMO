using System;

namespace NeoServer.Data.Entities;

public class IpBanEntity
{
    public int Id { get; set; }
    public string Ip { get; set; }
    public string Reason { get; set; }
    public DateTime BannedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ushort BannedBy { get; set; }
}