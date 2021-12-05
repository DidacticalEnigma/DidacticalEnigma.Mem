using System.Threading.Tasks;
using DidacticalEnigma.Mem.DatabaseModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace DidacticalEnigma.Mem
{
    public class MemContext : IdentityDbContext<User>
    {
        public DbSet<NpgsqlQuery> NpgsqlQueries { get; set; }
        
        public DbSet<Context> Contexts { get; set; }
        
        public DbSet<Project> Projects { get; set; }
        
        public DbSet<DatabaseModels.Translation> TranslationPairs { get; set; }
        
        public DbSet<AllowedMediaType> MediaTypes { get; set; }
        
        public DbSet<Category> Categories { get; set; }

        public MemContext(DbContextOptions<MemContext> dbOptions) :
            base(dbOptions)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            {
                var mediaTypeBuilder = modelBuilder.Entity<AllowedMediaType>();
                mediaTypeBuilder.HasKey(mediaType => mediaType.Id);
                mediaTypeBuilder.Property(mediaType => mediaType.MediaType).IsRequired();
                mediaTypeBuilder.Property(mediaType => mediaType.Extension).IsRequired();
                mediaTypeBuilder.HasIndex(mediaType => mediaType.MediaType).IsUnique();
                mediaTypeBuilder.HasData(AllowedMediaType.GetAllowedMediaTypes());
            }
            {
                var projectBuilder = modelBuilder.Entity<Project>();
                projectBuilder.HasKey(project => project.Id);
                projectBuilder.Property(project => project.Name);
                projectBuilder.HasIndex(project => project.Name).IsUnique();
                projectBuilder
                    .HasMany(project => project.Translations)
                    .WithOne(translationPair => translationPair.Parent)
                    .HasForeignKey(translationPair => translationPair.ParentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                projectBuilder
                    .HasMany(project => project.Categories)
                    .WithOne(category => category.Parent)
                    .HasForeignKey(category => category.ParentId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
            {
                var translationPairBuilder = modelBuilder.Entity<DatabaseModels.Translation>();
                translationPairBuilder.HasKey(translationPair => translationPair.Id);
                translationPairBuilder.Property(translationPair => translationPair.Source).IsRequired();
                translationPairBuilder.Property(translationPair => translationPair.SearchVector).IsRequired();
                translationPairBuilder.Property(translationPair => translationPair.Target).IsRequired(false);
                translationPairBuilder.Property(translationPair => translationPair.CorrelationId);
                translationPairBuilder.Property(translationPair => translationPair.CreationTime).IsRequired();
                translationPairBuilder.Property(translationPair => translationPair.ModificationTime).IsRequired();
                translationPairBuilder.Property(translationPair => translationPair.Notes).HasColumnType("jsonb").IsRequired(false);
                translationPairBuilder.Property(translationPair => translationPair.AssociatedData).HasColumnType("jsonb").IsRequired(false);

                translationPairBuilder.HasIndex(translationPair => translationPair.CorrelationId);
                translationPairBuilder.HasIndex(translationPair => new{translationPair.ParentId,translationPair.CorrelationId}).IsUnique();
                translationPairBuilder.HasIndex(translationPair => translationPair.SearchVector).HasMethod("GIN");
            }
            {
                var contextBuilder = modelBuilder.Entity<Context>();
                contextBuilder.HasKey(context => context.Id);
                contextBuilder.Property(context => context.Text).IsRequired(false);
                contextBuilder
                    .HasOne(context => context.MediaType)
                    .WithMany();
                contextBuilder.Property(context => context.ContentObjectId).IsRequired(false);
                contextBuilder
                    .HasOne(context => context.Project)
                    .WithMany()
                    .HasForeignKey(context => context.ProjectId);
                
                contextBuilder.HasIndex(context => new{context.ProjectId,context.CorrelationId}).IsUnique();
            }
            {
                var npgsqlQueryBuilder = modelBuilder.Entity<NpgsqlQuery>();
                npgsqlQueryBuilder.HasNoKey();
                npgsqlQueryBuilder.Property(context => context.Vec);
            }
            {
                var categoryBuilder = modelBuilder.Entity<Category>();
                categoryBuilder.HasKey(category => category.Id);
                categoryBuilder.Property(category => category.Name);

                categoryBuilder
                    .HasMany(category => category.Translations)
                    .WithOne(translationPair => translationPair.Category)
                    .HasForeignKey(translationPair => translationPair.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                categoryBuilder.HasIndex(category => new { category.Name, category.ParentId }).IsUnique();
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