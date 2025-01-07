using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class AccountVipListEntityConfiguration : IEntityTypeConfiguration<AccountVipListEntity>
{
    public void Configure(EntityTypeBuilder<AccountVipListEntity> builder)
    {
        builder.HasKey(e => new { e.AccountId, e.PlayerId });

        builder.Property(e => e.AccountId);
        builder.Property(e => e.PlayerId);
        builder.Property(e => e.Description);
    }
}