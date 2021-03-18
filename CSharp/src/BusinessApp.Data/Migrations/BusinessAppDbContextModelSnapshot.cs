﻿// <auto-generated />
using System;
using BusinessApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BusinessApp.Data.Migrations
{
    [DbContext(typeof(BusinessAppDbContext))]
    partial class BusinessAppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.3")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BusinessApp.Data.Metadata", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("MetadataId");

                    b.Property<string>("DataSetName")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasColumnName("DataSetName");

                    b.Property<DateTimeOffset>("OccurredUtc")
                        .HasColumnType("datetimeoffset(0)")
                        .HasColumnName("OccurredUtc");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasColumnName("TypeName");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasColumnName("Username");

                    b.HasKey("Id");

                    b.ToTable("Metadata", "dbo");
                });
#pragma warning restore 612, 618
        }
    }
}
