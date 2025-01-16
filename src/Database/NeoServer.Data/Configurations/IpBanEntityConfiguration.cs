using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class IpBanEntityConfiguration : IEntityTypeConfiguration<IpBanEntity>
{
    public void Configure(EntityTypeBuilder<IpBanEntity> builder)
    {
        builder.ToTable("IpBans");
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(b => b.Ip)
            .IsRequired()
            .HasMaxLength(45);
        
        builder.Property(b => b.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.BannedAt)
            .IsRequired();

        builder.Property(b => b.ExpiresAt)
            .IsRequired();

        builder.Property(b => b.BannedBy)
            .IsRequired();

        builder.HasIndex(b => b.Ip)
            .HasDatabaseName("IX_Bans_Ip");

        builder.HasIndex(b => b.ExpiresAt)
            .HasDatabaseName("IX_Bans_ExpiresAt");
        
        Seed(builder);
    }
    
    private static void Seed(EntityTypeBuilder<IpBanEntity> builder)
    {
        builder.HasData
        (
            new IpBanEntity
            {
                Id = 1,
                Ip = "172.0.0.195",
                BannedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(100),
                BannedBy = 1,
                Reason = "Using bot."
            }
        );
    }
}