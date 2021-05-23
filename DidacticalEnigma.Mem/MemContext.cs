using System.Threading.Tasks;
using DidacticalEnigma.Mem.Translation.StoredModels;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace DidacticalEnigma.Mem
{
    public class MemContext : DbContext
    {
        public DbSet<NpgsqlQuery> NpgsqlQueries { get; set; }
        
        public DbSet<Context> Contexts { get; set; }
        
        public DbSet<Project> Projects { get; set; }
        
        public DbSet<Translation.StoredModels.Translation> TranslationPairs { get; set; }
        
        public DbSet<AllowedMediaType> MediaTypes { get; set; }

        public MemContext(DbContextOptions<MemContext> dbOptions) :
            base(dbOptions)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            {
                var mediaTypeBuilder = modelBuilder.Entity<AllowedMediaType>();
                mediaTypeBuilder.HasKey(mediaType => mediaType.Id);
                mediaTypeBuilder.Property(mediaType => mediaType.MediaType).HasMaxLength(64).IsRequired();
                mediaTypeBuilder.HasIndex(mediaType => mediaType.MediaType).IsUnique();
                mediaTypeBuilder.HasData(AllowedMediaType.GetAllowedMediaTypes());
            }
            {
                var projectBuilder = modelBuilder.Entity<Project>();
                projectBuilder.HasKey(project => project.Id);
                projectBuilder.Property(project => project.Name).HasMaxLength(32);
                projectBuilder.HasIndex(project => project.Name).IsUnique();
                projectBuilder
                    .HasMany(project => project.Translations)
                    .WithOne(translationPair => translationPair.Parent)
                    .HasForeignKey(translationPair => translationPair.ParentId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
            {
                var translationPairBuilder = modelBuilder.Entity<Translation.StoredModels.Translation>();
                translationPairBuilder.HasKey(translationPair => translationPair.Id);
                translationPairBuilder.Property(translationPair => translationPair.Source).HasMaxLength(4096).IsRequired();
                translationPairBuilder.Property(translationPair => translationPair.SearchVector).HasMaxLength(8192).IsRequired();
                translationPairBuilder.Property(translationPair => translationPair.Target).HasMaxLength(4096).IsRequired(false);
                translationPairBuilder.Property(translationPair => translationPair.CorrelationId).HasMaxLength(256);
                translationPairBuilder.Property(translationPair => translationPair.CreationTime).IsRequired(true);
                translationPairBuilder.Property(translationPair => translationPair.ModificationTime).IsRequired(true);
                
                translationPairBuilder
                    .HasOne(translationPair => translationPair.Context)
                    .WithMany()
                    .HasForeignKey(translationPair => translationPair.ContextId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
                
                translationPairBuilder.HasIndex(translationPair => translationPair.CorrelationId);
                translationPairBuilder.HasIndex(translationPair => new{translationPair.ParentId,translationPair.CorrelationId}).IsUnique();
            }
            {
                var contextBuilder = modelBuilder.Entity<Context>();
                contextBuilder.HasKey(context => context.Id);
                contextBuilder.Property(context => context.Content).HasColumnType("bytea").IsRequired(false);
                contextBuilder.Property(context => context.Text).HasMaxLength(512).IsRequired(false);
                contextBuilder
                    .HasOne(context => context.MediaType)
                    .WithMany();
            }
            {
                var npgsqlQueryBuilder = modelBuilder.Entity<NpgsqlQuery>();
                npgsqlQueryBuilder.HasNoKey();
                npgsqlQueryBuilder.Property(context => context.Vec);
            }
        }

        public async Task<NpgsqlTsVector> ToTsVector(string s)
        {
            var m =
                await this.NpgsqlQueries.FromSqlInterpolated($"SELECT to_tsvector('simple', {s}) AS \"Vec\"").SingleAsync();
            return m.Vec;
        }
    }
}