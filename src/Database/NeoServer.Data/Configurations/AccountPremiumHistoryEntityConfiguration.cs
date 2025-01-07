using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class AccountPremiumHistoryEntityConfiguration : IEntityTypeConfiguration<AccountPremiumHistoryEntity>
{
    public void Configure(EntityTypeBuilder<AccountPremiumHistoryEntity> builder)
    {
        builder.ToTable("AccountPremiumHistory");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.AccountId)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.EndAt)
            .IsRequired();

        builder.HasOne(e => e.Account)
            .WithMany() 
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
    private static void Seed(EntityTypeBuilder<AccountPremiumHistoryEntity> builder)
    {
        builder.HasData
        (
            new AccountPremiumHistoryEntity
            {
                Id = 1,
                AccountId = 1,
                Description = "VIP do GOD",
                CreatedAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddDays(30),
            }
        );
    }
}