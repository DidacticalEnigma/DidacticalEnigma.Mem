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
    [Migration("20211211105329_AddOwnersAndContributors")]
    partial class AddOwnersAndContributors
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.AllowedMediaType", b =>
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

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ParentId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.HasIndex("Name", "ParentId")
                        .IsUnique();

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Context", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long?>("ContentObjectId")
                        .HasColumnType("bigint");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("MediaTypeId")
                        .HasColumnType("integer");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("MediaTypeId");

                    b.HasIndex("ProjectId", "CorrelationId")
                        .IsUnique();

                    b.ToTable("Contexts");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.ContributorInvitation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("InvitedUserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("InvitingUserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("InvitedUserId");

                    b.HasIndex("InvitingUserId");

                    b.HasIndex("ProjectId", "InvitingUserId", "InvitedUserId")
                        .IsUnique();

                    b.ToTable("Invitations");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.ContributorMembership", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ProjectId")
                        .HasColumnType("integer");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("ProjectId", "UserId")
                        .IsUnique();

                    b.ToTable("Memberships");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.NpgsqlQuery", b =>
                {
                    b.Property<NpgsqlTsVector>("Vec")
                        .IsRequired()
                        .HasColumnType("tsvector");

                    b.ToTable("NpgsqlQueries");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("PublicallyReadable")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("OwnerId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Translation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AssociatedData")
                        .HasColumnType("jsonb");

                    b.Property<Guid?>("CategoryId")
                        .HasColumnType("uuid");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CreatedById")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ModificationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ModifiedById")
                        .IsRequired()
                        .HasColumnType("text");

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

                    b.HasIndex("CategoryId");

                    b.HasIndex("CorrelationId");

                    b.HasIndex("CreatedById");

                    b.HasIndex("ModifiedById");

                    b.HasIndex("SearchVector");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("SearchVector"), "GIN");

                    b.HasIndex("ParentId", "CorrelationId")
                        .IsUnique();

                    b.ToTable("TranslationPairs");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreApplication", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<string>("ClientId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("ClientSecret")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("ConsentType")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("DisplayName")
                        .HasColumnType("text");

                    b.Property<string>("DisplayNames")
                        .HasColumnType("text");

                    b.Property<string>("Permissions")
                        .HasColumnType("text");

                    b.Property<string>("PostLogoutRedirectUris")
                        .HasColumnType("text");

                    b.Property<string>("Properties")
                        .HasColumnType("text");

                    b.Property<string>("RedirectUris")
                        .HasColumnType("text");

                    b.Property<string>("Requirements")
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("ClientId")
                        .IsUnique();

                    b.ToTable("OpenIddictApplications", (string)null);
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreAuthorization", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<string>("ApplicationId")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Properties")
                        .HasColumnType("text");

                    b.Property<string>("Scopes")
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Subject")
                        .HasMaxLength(400)
                        .HasColumnType("character varying(400)");

                    b.Property<string>("Type")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId", "Status", "Subject", "Type");

                    b.ToTable("OpenIddictAuthorizations", (string)null);
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreScope", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Descriptions")
                        .HasColumnType("text");

                    b.Property<string>("DisplayName")
                        .HasColumnType("text");

                    b.Property<string>("DisplayNames")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Properties")
                        .HasColumnType("text");

                    b.Property<string>("Resources")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("OpenIddictScopes", (string)null);
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreToken", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<string>("ApplicationId")
                        .HasColumnType("text");

                    b.Property<string>("AuthorizationId")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Payload")
                        .HasColumnType("text");

                    b.Property<string>("Properties")
                        .HasColumnType("text");

                    b.Property<DateTime?>("RedemptionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ReferenceId")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Subject")
                        .HasMaxLength(400)
                        .HasColumnType("character varying(400)");

                    b.Property<string>("Type")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("AuthorizationId");

                    b.HasIndex("ReferenceId")
                        .IsUnique();

                    b.HasIndex("ApplicationId", "Status", "Subject", "Type");

                    b.ToTable("OpenIddictTokens", (string)null);
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Category", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.Project", "Parent")
                        .WithMany("Categories")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Context", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.AllowedMediaType", "MediaType")
                        .WithMany()
                        .HasForeignKey("MediaTypeId");

                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("MediaType");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.ContributorInvitation", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", "InvitedUser")
                        .WithMany("InvitationsReceived")
                        .HasForeignKey("InvitedUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", "InvitingUser")
                        .WithMany("InvitationsSent")
                        .HasForeignKey("InvitingUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.Project", "Project")
                        .WithMany("Invitations")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("InvitedUser");

                    b.Navigation("InvitingUser");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.ContributorMembership", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.Project", "Project")
                        .WithMany("Contributors")
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", "User")
                        .WithMany("ContributedProjects")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Project");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Project", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", "Owner")
                        .WithMany("OwnedProjects")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Translation", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.Category", "Category")
                        .WithMany("Translations")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", "ModifiedBy")
                        .WithMany()
                        .HasForeignKey("ModifiedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.Project", "Parent")
                        .WithMany("Translations")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("CreatedBy");

                    b.Navigation("ModifiedBy");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("DidacticalEnigma.Mem.DatabaseModels.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreAuthorization", b =>
                {
                    b.HasOne("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreApplication", "Application")
                        .WithMany("Authorizations")
                        .HasForeignKey("ApplicationId");

                    b.Navigation("Application");
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreToken", b =>
                {
                    b.HasOne("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreApplication", "Application")
                        .WithMany("Tokens")
                        .HasForeignKey("ApplicationId");

                    b.HasOne("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreAuthorization", "Authorization")
                        .WithMany("Tokens")
                        .HasForeignKey("AuthorizationId");

                    b.Navigation("Application");

                    b.Navigation("Authorization");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Category", b =>
                {
                    b.Navigation("Translations");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.Project", b =>
                {
                    b.Navigation("Categories");

                    b.Navigation("Contributors");

                    b.Navigation("Invitations");

                    b.Navigation("Translations");
                });

            modelBuilder.Entity("DidacticalEnigma.Mem.DatabaseModels.User", b =>
                {
                    b.Navigation("ContributedProjects");

                    b.Navigation("InvitationsReceived");

                    b.Navigation("InvitationsSent");

                    b.Navigation("OwnedProjects");
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreApplication", b =>
                {
                    b.Navigation("Authorizations");

                    b.Navigation("Tokens");
                });

            modelBuilder.Entity("OpenIddict.EntityFrameworkCore.Models.OpenIddictEntityFrameworkCoreAuthorization", b =>
                {
                    b.Navigation("Tokens");
                });
#pragma warning restore 612, 618
        }
    }
}
