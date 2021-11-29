﻿// <auto-generated />
using System;
using DidacticalEnigma.Mem;
using DidacticalEnigma.Mem.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace DidacticalEnigma.Mem.Migrations
{
    [DbContext(typeof(MemContext))]
    [Migration("20211128092741_NewConcept")]
    partial class NewConcept
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DidacticalEnigma.Mem.Translation.StoredModels.AllowedMediaType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Extension")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MediaType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MediaType")
                        .IsUnique();

                    b.ToTable("MediaTypes");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Extension = "jpg",
                            MediaType = "image/jpeg"
                        },
                        new
                        {
                            Id = 2,
                            Extension = "png",
                            MediaType = "image/png"
                        });
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.Translation.StoredModels.Context", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long?>("ContentObjectId")
                        .HasColumnType("bigint");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("MediaTypeId")
                        .HasColumnType("integer");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MediaTypeId");

                    b.HasIndex("ProjectId", "CorrelationId")
                        .IsUnique();

                    b.ToTable("Contexts");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.Translation.StoredModels.NpgsqlQuery", b =>
                {
                    b.Property<NpgsqlTsVector>("Vec")
                        .IsRequired()
                        .HasColumnType("tsvector");

                    b.ToTable("NpgsqlQueries");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.Translation.StoredModels.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.Translation.StoredModels.Translation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AssociatedData")
                        .HasColumnType("jsonb");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ModificationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<NotesCollection>("Notes")
                        .HasColumnType("jsonb");

                    b.Property<int>("ParentId")
                        .HasColumnType("integer");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .IsRequired()
                        .HasColumnType("tsvector");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Target")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.HasIndex("SearchVector");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("SearchVector"), "GIN");

                    b.HasIndex("ParentId", "CorrelationId")
                        .IsUnique();

                    b.ToTable("TranslationPairs");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.Translation.StoredModels.Context", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.Translation.StoredModels.AllowedMediaType", "MediaType")
                        .WithMany()
                        .HasForeignKey("MediaTypeId");

                    b.HasOne("DidacticalEnigma.Mem.Translation.StoredModels.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MediaType");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.Translation.StoredModels.Translation", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.Translation.StoredModels.Project", "Parent")
                        .WithMany("Translations")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.Translation.StoredModels.Project", b =>
                {
                    b.Navigation("Translations");
                });
#pragma warning restore 612, 618
        }
    }
}
