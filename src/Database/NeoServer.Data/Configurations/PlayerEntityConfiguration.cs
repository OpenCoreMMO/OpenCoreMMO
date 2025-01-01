using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Data.Seeds;

namespace NeoServer.Data.Configurations;

public class PlayerEntityConfiguration : IEntityTypeConfiguration<PlayerEntity>
{
    public void Configure(EntityTypeBuilder<PlayerEntity> entity)
    {
        entity.ToTable("Player");

        entity.HasKey(e => e.Id);

        entity.HasIndex(e => e.AccountId);

        entity.HasIndex(e => e.Name)
            .IsUnique();

        entity.HasIndex(e => e.Vocation);

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Group);

        ConfigureProperty(entity, e => e.AccountId, "int", "0");
        ConfigureProperty(entity, e => e.TownId, "int", "1");
        ConfigureProperty(entity, e => e.Capacity, "int", "0");
        ConfigureProperty(entity, e => e.Health, "int", "150");
        ConfigureProperty(entity, e => e.MaxHealth, "int", "150");
        ConfigureProperty(entity, e => e.Level, "int", "1");
        ConfigureProperty(entity, e => e.LookAddons, "int", "0");
        ConfigureProperty(entity, e => e.LookBody, "int", "0");
        ConfigureProperty(entity, e => e.LookFeet, "int", "0");
        ConfigureProperty(entity, e => e.LookHead, "int", "0");
        ConfigureProperty(entity, e => e.LookLegs, "int", "0");
        ConfigureProperty(entity, e => e.LookType, "int", "136");
        ConfigureProperty(entity, e => e.Mana, "int", "0");
        ConfigureProperty(entity, e => e.MaxMana, "int", "0");
        ConfigureProperty(entity, e => e.Name, "varchar(255)");
        ConfigureProperty(entity, e => e.PosX, "int", "0");
        ConfigureProperty(entity, e => e.PosY, "int", "0");
        ConfigureProperty(entity, e => e.PosZ, "int", "0");
        ConfigureProperty(entity, e => e.Gender, "int", "0");
        ConfigureProperty(entity, e => e.SkillAxe, null, "10");
        ConfigureProperty(entity, e => e.SkillAxeTries, null, "0");
        ConfigureProperty(entity, e => e.SkillClub, null, "10");
        ConfigureProperty(entity, e => e.SkillClubTries, null, "0");
        ConfigureProperty(entity, e => e.SkillDist, null, "10");
        ConfigureProperty(entity, e => e.SkillDistTries, null, "0");
        ConfigureProperty(entity, e => e.SkillFishing, null, "10");
        ConfigureProperty(entity, e => e.SkillFishingTries, null, "0");
        ConfigureProperty(entity, e => e.SkillFist, null, "10");
        ConfigureProperty(entity, e => e.SkillFistTries, null, "0");
        ConfigureProperty(entity, e => e.SkillShielding, null, "10");
        ConfigureProperty(entity, e => e.SkillShieldingTries, null, "0");
        ConfigureProperty(entity, e => e.Online, null, "false");
        ConfigureProperty(entity, e => e.SkillSword, null, "10");
        ConfigureProperty(entity, e => e.SkillSwordTries, null, "0");
        ConfigureProperty(entity, e => e.Vocation, "int", "0");
        ConfigureProperty(entity, e => e.RemainingRecoverySeconds, "int", "0");

        entity.HasOne(d => d.Account)
            .WithMany(p => p.Players)
            .HasForeignKey(d => d.AccountId)
            .HasConstraintName("players_ibfk_1");

        entity.HasOne(d => d.World)
            .WithMany()
            .HasForeignKey(d => d.WorldId);

        entity.HasOne(x => x.GuildMember).WithOne(x => x.Player);

        PlayerModelSeed.Seed(entity);
    }

    private static void ConfigureProperty<TProperty>(
        EntityTypeBuilder<PlayerEntity> entity,
        Expression<Func<PlayerEntity, TProperty>> property,
        string columnType,
        string defaultValueSql = null)
    {
        entity.Property(property)
            .HasColumnType(columnType)
            .HasDefaultValueSql(defaultValueSql);
    }
}