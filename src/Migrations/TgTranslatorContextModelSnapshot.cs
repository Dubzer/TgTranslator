﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TgTranslator.Data;

#nullable disable

namespace TgTranslator.Migrations
{
    [DbContext(typeof(TgTranslatorContext))]
    partial class TgTranslatorContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TgTranslator.Models.Group", b =>
                {
                    b.Property<long>("GroupId")
                        .HasColumnType("bigint")
                        .HasColumnName("group_id");

                    b.Property<int>("Delay")
                        .HasColumnType("integer")
                        .HasColumnName("delay");

                    b.Property<string>("Language")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasColumnName("language")
                        .HasDefaultValueSql("'en'::text");

                    b.Property<int>("TranslationMode")
                        .HasColumnType("integer")
                        .HasColumnName("translation_mode");

                    b.HasKey("GroupId")
                        .HasName("groups_pk");

                    b.ToTable("groups", (string)null);
                });

            modelBuilder.Entity("TgTranslator.Models.GroupBlacklist", b =>
                {
                    b.Property<long>("GroupId")
                        .HasColumnType("bigint")
                        .HasColumnName("group_id");

                    b.Property<DateTime>("AddedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("added_at")
                        .HasDefaultValueSql("timezone('utc'::text, now())");

                    b.HasKey("GroupId")
                        .HasName("groups_blacklist_pk");

                    b.HasIndex("GroupId")
                        .IsUnique()
                        .HasDatabaseName("groups_blacklist_group_id_uindex");

                    b.ToTable("groups_blacklist", (string)null);
                });

            modelBuilder.Entity("TgTranslator.Models.User", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.Property<bool>("PmAllowed")
                        .HasColumnType("boolean")
                        .HasColumnName("pm_allowed");

                    b.Property<string>("Track")
                        .HasColumnType("text")
                        .HasColumnName("track");

                    b.HasKey("UserId")
                        .HasName("users_pkey");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("TgTranslator.Models.GroupBlacklist", b =>
                {
                    b.HasOne("TgTranslator.Models.Group", "Group")
                        .WithOne("GroupBlacklist")
                        .HasForeignKey("TgTranslator.Models.GroupBlacklist", "GroupId")
                        .IsRequired()
                        .HasConstraintName("groups_blacklist_groups_group_id_fk");

                    b.Navigation("Group");
                });

            modelBuilder.Entity("TgTranslator.Models.Group", b =>
                {
                    b.Navigation("GroupBlacklist");
                });
#pragma warning restore 612, 618
        }
    }
}
