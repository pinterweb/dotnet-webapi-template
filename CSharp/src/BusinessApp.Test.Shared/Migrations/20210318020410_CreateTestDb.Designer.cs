﻿// <auto-generated />
using System;
using BusinessApp.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BusinessApp.Test.Shared.Migrations
{
    [DbContext(typeof(BusinessAppTestDbContext))]
    [Migration("20210318020410_CreateTestDb")]
    partial class CreateTestDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.3")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BusinessApp.Data.EventMetadata<BusinessApp.WebApi.Delete+Event>", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("EventMetadataId");

                    b.Property<long?>("CausationId")
                        .HasColumnType("bigint")
                        .HasColumnName("CausationId");

                    b.Property<long?>("CorrelationId")
                        .HasColumnType("bigint")
                        .HasColumnName("CorrelationId");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasColumnType("varchar(500)");

                    b.Property<DateTimeOffset>("OccurredUtc")
                        .HasColumnType("datetimeoffset(0)");

                    b.HasKey("Id");

                    b.ToTable("DeleteEvent", "evt");
                });

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

            modelBuilder.Entity("BusinessApp.Test.Shared.AggregateRootStub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("AggregateRootStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ChildResponseStub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("ResponseStubId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ResponseStubId");

                    b.ToTable("ChildResponseStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.DomainEventStub", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("OccurredUtc")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("DomainEventStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.RequestStub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("RequestStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ResponseStub", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ResponseStubId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ResponseStubId");

                    b.ToTable("ResponseStub");
                });

            modelBuilder.Entity("BusinessApp.WebApi.EntityId", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("Id");

                    b.ToTable("EntityId");
                });

            modelBuilder.Entity("BusinessApp.Data.EventMetadata<BusinessApp.WebApi.Delete+Event>", b =>
                {
                    b.OwnsOne("BusinessApp.WebApi.Delete+Event", "Event", b1 =>
                        {
                            b1.Property<long>("EventMetadata<Event>Id")
                                .HasColumnType("bigint");

                            b1.Property<int?>("Id1")
                                .HasColumnType("int");

                            b1.HasKey("EventMetadata<Event>Id");

                            b1.HasIndex("Id1");

                            b1.ToTable("DeleteEvent");

                            b1.WithOwner()
                                .HasForeignKey("EventMetadata<Event>Id");

                            b1.HasOne("BusinessApp.WebApi.EntityId", "Id")
                                .WithMany()
                                .HasForeignKey("Id1");

                            b1.Navigation("Id");
                        });

                    b.Navigation("Event");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ChildResponseStub", b =>
                {
                    b.HasOne("BusinessApp.Test.Shared.ResponseStub", "ResponseStub")
                        .WithMany()
                        .HasForeignKey("ResponseStubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ResponseStub");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ResponseStub", b =>
                {
                    b.HasOne("BusinessApp.Test.Shared.ResponseStub", null)
                        .WithMany("ChildResponseStubs")
                        .HasForeignKey("ResponseStubId");
                });

            modelBuilder.Entity("BusinessApp.Test.Shared.ResponseStub", b =>
                {
                    b.Navigation("ChildResponseStubs");
                });
#pragma warning restore 612, 618
        }
    }
}
