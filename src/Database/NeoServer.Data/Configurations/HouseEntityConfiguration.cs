using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class HouseEntityConfiguration : IEntityTypeConfiguration<HouseEntity>
{
    public void Configure(EntityTypeBuilder<HouseEntity> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).HasMaxLength(255).IsRequired();
        builder.Property(h => h.Paid).HasDefaultValue(0).IsRequired();
        builder.Property(h => h.Warnings).HasDefaultValue(0).IsRequired();
        builder.Property(h => h.Rent).HasDefaultValue(0).IsRequired();
        builder.Property(h => h.TownId).HasDefaultValue(0).IsRequired();
        builder.Property(h => h.Bid).HasDefaultValue(0).IsRequired();
        builder.Property(h => h.BidEnd).HasDefaultValue(0).IsRequired();
        builder.Property(h => h.LastBid).HasDefaultValue(0).IsRequired();
        builder.Property(h => h.HighestBidder).HasDefaultValue(0).IsRequired();
        builder.Property(h => h.Size).HasDefaultValue(0).IsRequired();
        builder.Property(h => h.Beds).HasDefaultValue(0).IsRequired();

        builder.HasIndex(h => h.Onwer);
        builder.HasIndex(h => h.TownId);
    }
}