using System;

namespace NeoServer.Data.Entities;

public sealed class AccountPremiumHistoryEntity
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime EndAt { get; set; }
    
    public AccountEntity Account { get; set; }
}