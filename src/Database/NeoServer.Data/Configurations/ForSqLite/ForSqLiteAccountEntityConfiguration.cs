using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations.ForSqLite;

public class ForSqLiteAccountEntityConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.EmailAddress)
            .IsUnique();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .IsRequired()
            .HasAnnotation("Sqlite:Autoincrement", false);

        builder.Property(e => e.CreatedAt)
            .HasAnnotation("Sqlite:Autoincrement", false);

        builder.Property(e => e.EmailAddress)
            .IsRequired()
            .HasColumnType("varchar(320)");

        builder.Property(e => e.AllowManyOnline)
            .HasDefaultValue(0)
            .HasColumnType("boolean");

        builder.Property(e => e.Password)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("char(20)");

        builder.Property(e => e.PremiumTime)
            .HasColumnType("int(11)")
            .HasAnnotation("Sqlite:Autoincrement", false)
            .HasDefaultValueSql("0");

        builder.Property(e => e.Secret)
            .HasColumnType("char(16)");

        builder.HasMany(x => x.VipList)
            .WithOne()
            .HasForeignKey("AccountId");

        builder.Property(e => e.BanishedAt)
            .HasColumnType("TEXT")
            .HasConversion<DateTime>();

        builder.Property(e => e.BanishmentReason)
            .HasColumnType("TEXT");

        builder.Property(e => e.BannedBy)
            .HasColumnType("INTEGER");

        Seed(builder);
    }

    private static void Seed(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.HasData
        (
            new AccountEntity
            {
                Id = 1,
                EmailAddress = "1",
                Password = "1",
                PremiumTime = 30,
                AllowManyOnline = true
            }
        );
    }
}