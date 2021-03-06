﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable IDE0058
namespace BusinessApp.Infrastructure.Persistence
{
    /// <summary>
    /// Entity configuration to save request metadata
    /// </summary>
    public class RequestMetadataEntityConfiguration : IEntityTypeConfiguration<RequestMetadata>
    {
        public void Configure(EntityTypeBuilder<RequestMetadata> builder)
        {
            builder.ToTable("RequestMetadata", "dbo");

            builder.Property<int>("Id").HasColumnName("RequestMetadataId");

            builder.Property(p => p.RequestType)
                .HasColumnType("varchar(100)")
#nullable disable
                .HasConversion(
                    t => t.FullName,
                    str => Type.GetType(str))
#nullable enable
                .IsRequired();

            builder.Property(p => p.ResponseType)
                .HasColumnType("varchar(100)")
#nullable disable
                .HasConversion(
                    t => t.FullName,
                    str => Type.GetType(str))
#nullable enable
                .IsRequired();

            builder.Property(p => p.EventTriggers)
                .HasColumnType("varchar(max)")
#nullable disable
                .HasConversion(
                    t => string.Join(";", t.Select(t => t.FullName)),
                    strs => strs.Split(";", StringSplitOptions.TrimEntries).Select(str => Type.GetType(str)))
#nullable enable
                .IsRequired();
        }
    }
}
#pragma warning restore IDE0058
