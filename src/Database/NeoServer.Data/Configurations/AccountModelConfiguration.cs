﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class AccountModelConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(e => e.AccountId);

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("name")
            .IsUnique();

        builder.Property(e => e.AccountId)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e._creation)
            .HasColumnName("creation")
            .HasColumnType("int(11)")
            .HasAnnotation("Sqlite:Autoincrement", false)
            .HasDefaultValueSql("0");

        builder.Property(e => e.Email)
            .IsRequired()
            .HasColumnName("email")
            .HasColumnType("varchar(255)");

        builder.Property(e => e._lastday)
            .HasColumnName("lastday")
            .HasColumnType("int(10) unsigned")
            .HasAnnotation("Sqlite:Autoincrement", false).HasDefaultValueSql("0");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasColumnName("name")
            .HasColumnType("varchar(32)");

        builder.Property(e => e.Password)
            .IsRequired()
            .HasColumnName("password")
            .HasColumnType("char(255)");

        builder.Property(e => e.PremiumTime)
            .HasColumnName("premdays")
            .HasColumnType("int(11)")
            .HasAnnotation("Sqlite:Autoincrement", false).HasDefaultValueSql("0");

        builder.Property(e => e.Secret)
            .HasColumnName("secret")
            .HasColumnType("char(16)");

        builder.Property(e => e.AllowManyOnline)
            .HasDefaultValue(0)
            .HasColumnName("allow_many_online");

        builder.Property(e => e.Type)
            .HasColumnName("type")
            .HasColumnType("int(11)")
            .HasAnnotation("Sqlite:Autoincrement", false).HasDefaultValueSql("1");

        builder.HasMany(x => x.VipList).WithOne().HasForeignKey("AccountId");

        builder.Ignore(i => i.Creation);

        builder.Ignore(i => i.LastDay);

        builder.Property(e => e.BanishedAt)
            .HasColumnName("banishedAt")
            .HasColumnType("datetime");

        builder.Property(e => e.BanishmentReason)
            .HasColumnName("banishedReason")
            .HasColumnType("varchar(255)");

        builder.Property(e => e.BannedBy)
            .HasColumnName("BannedBy")
            .HasColumnType("int");

        Seed(builder);
    }

    private static void Seed(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.HasData
        (
            new AccountEntity
            {
                AccountId = 1,
                Name = "1",
                Email = "god@gmail.com",
                Password = "1",
                PremiumTime = 30,
                AllowManyOnline = true
            }
        );
    }
}