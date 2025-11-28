using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class GuildWarEntityConfiguration : IEntityTypeConfiguration<GuildWarEntity>
{
    public void Configure(EntityTypeBuilder<GuildWarEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Guild1Id)
            .IsRequired();

        builder.Property(e => e.Guild2Id)
            .IsRequired();

        builder.Property(e => e.Guild1Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Guild2Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.StartedAt)
            .IsRequired();

        builder.Property(e => e.EndedAt)
            .IsRequired(false);

        builder.HasOne(e => e.Guild1)
            .WithMany()
            .HasForeignKey(e => e.Guild1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Guild2)
            .WithMany()
            .HasForeignKey(e => e.Guild2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Guild1Id);
        builder.HasIndex(e => e.Guild2Id);
    }
}