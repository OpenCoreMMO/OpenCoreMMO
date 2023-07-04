﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class GuildRankModelConfiguration : IEntityTypeConfiguration<GuildRankEntity>
{
    public void Configure(EntityTypeBuilder<GuildRankEntity> builder)
    {
        builder.ToTable("guild_ranks");

        builder.HasKey(e => new { e.Id });

        builder.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("id");
        builder.Property(e => e.GuildId).HasColumnName("guild_id");
        builder.Property(e => e.Name).HasColumnName("name");
        builder.Property(e => e.Level).HasColumnName("level");
    }
}