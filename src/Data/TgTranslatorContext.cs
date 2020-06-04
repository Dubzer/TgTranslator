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
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.GroupId)
                    .HasName("groups_pkey");

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
