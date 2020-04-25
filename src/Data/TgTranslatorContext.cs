using Microsoft.EntityFrameworkCore;
using TgTranslator.Models;

namespace TgTranslator.Data
{
    public partial class TgTranslatorContext : DbContext
    {
        public TgTranslatorContext()
        {
            
        }

        public TgTranslatorContext(DbContextOptions<TgTranslatorContext> options) : base(options)
        {
        }

        public virtual DbSet<Group> Groups { get; set; }

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
                    .HasColumnName("language")
                    .HasMaxLength(10);

                entity.Property(e => e.TranslationMode).HasColumnName("translation_mode");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
