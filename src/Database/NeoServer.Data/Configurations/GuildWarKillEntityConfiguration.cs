using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class GuildWarKillEntityConfiguration : IEntityTypeConfiguration<GuildWarKillEntity>
{
    public void Configure(EntityTypeBuilder<GuildWarKillEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Killer)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Target)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.KillerGuildId)
            .IsRequired();

        builder.Property(e => e.TargetGuildId)
            .IsRequired();

        builder.Property(e => e.WarId)
            .IsRequired();

        builder.Property(e => e.Timestamp)
            .IsRequired();

        builder.HasOne(e => e.KillerGuild)
            .WithMany()
            .HasForeignKey(e => e.KillerGuildId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TargetGuild)
            .WithMany()
            .HasForeignKey(e => e.TargetGuildId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.War)
            .WithMany()
            .HasForeignKey(e => e.WarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.WarId);
        builder.HasIndex(e => e.KillerGuildId);
        builder.HasIndex(e => e.TargetGuildId);
    }
}