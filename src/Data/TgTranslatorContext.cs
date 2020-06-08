using Microsoft.EntityFrameworkCore;
using TgTranslator.Models;

namespace TgTranslator.Data
{
    public partial class TgTranslatorContext : DbContext
    {
        public TgTranslatorContext()
        {
        }

        public TgTranslatorContext(DbContextOptions<TgTranslatorContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupBlacklist> GroupsBlacklist { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.GroupId)
                    .HasName("groups_pk");

                entity.ToTable("groups");

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasColumnName("language")
                    .HasDefaultValueSql("'en'::text");

                entity.Property(e => e.TranslationMode).HasColumnName("translation_mode");
            });

            modelBuilder.Entity<GroupBlacklist>(entity =>
            {
                entity.HasKey(e => e.GroupId)
                    .HasName("groups_blacklist_pk");

                entity.ToTable("groups_blacklist");

                entity.HasIndex(e => e.GroupId)
                    .HasName("groups_blacklist_group_id_uindex")
                    .IsUnique();

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.AddedAt)
                    .HasColumnName("added_at")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("timezone('utc'::text, now())");

                entity.HasOne(d => d.Group)
                    .WithOne(p => p.GroupBlacklist)
                    .HasForeignKey<GroupBlacklist>(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("groups_blacklist_groups_group_id_fk");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("users_pkey");

                entity.ToTable("users");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .ValueGeneratedNever();

                entity.Property(e => e.PmAllowed).HasColumnName("pm_allowed");

                entity.Property(e => e.Track).HasColumnName("track");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
