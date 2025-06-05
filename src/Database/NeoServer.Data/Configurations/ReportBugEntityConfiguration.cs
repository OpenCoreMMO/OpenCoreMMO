using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class ReportBugEntityConfiguration : IEntityTypeConfiguration<ReportBugEntity>
{
    public void Configure(EntityTypeBuilder<ReportBugEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.PlayerId).IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Ip).HasMaxLength(45);
        builder.Property(e => e.PosX).IsRequired();
        builder.Property(e => e.PosY).IsRequired();
        builder.Property(e => e.PosZ).IsRequired();
        builder.Property(e => e.ClosedAt);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}