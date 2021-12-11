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
        
        public DbSet<ContributorMembership> Memberships { get; set; }
        
        public DbSet<ContributorInvitation> Invitations { get; set; }

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
                projectBuilder.Property(project => project.PublicallyReadable);
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

                projectBuilder
                    .HasOne(project => project.Owner)
                    .WithMany(user => user.OwnedProjects)
                    .HasForeignKey(project => project.OwnerId);
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
                
                translationPairBuilder
                    .HasOne(translationPair => translationPair.CreatedBy)
                    .WithMany()
                    .HasForeignKey(translationPair => translationPair.CreatedById);
                
                translationPairBuilder
                    .HasOne(translationPair => translationPair.ModifiedBy)
                    .WithMany()
                    .HasForeignKey(translationPair => translationPair.ModifiedById);
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
                contextBuilder.Property(context => context.CreationTime);
                
                contextBuilder.HasIndex(context => new{context.ProjectId,context.CorrelationId}).IsUnique();

                contextBuilder
                    .HasOne(context => context.CreatedBy)
                    .WithMany()
                    .HasForeignKey(context => context.CreatedById);
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
            {
                var contributorMembershipBuilder = modelBuilder.Entity<ContributorMembership>();
                contributorMembershipBuilder.HasKey(membership => membership.Id);
                contributorMembershipBuilder
                    .HasOne(membership => membership.Project)
                    .WithMany(project => project.Contributors)
                    .HasForeignKey(membership => membership.ProjectId);
                contributorMembershipBuilder
                    .HasOne(membership => membership.User)
                    .WithMany(user => user.ContributedProjects)
                    .HasForeignKey(membership => membership.UserId);
                
                contributorMembershipBuilder.HasIndex(membership => new{membership.ProjectId, membership.UserId}).IsUnique();
            }
            {
                var contributorInvitationBuilder = modelBuilder.Entity<ContributorInvitation>();
                contributorInvitationBuilder.HasKey(invitation => invitation.Id);
                contributorInvitationBuilder
                    .HasOne(invitation => invitation.Project)
                    .WithMany(project => project.Invitations)
                    .HasForeignKey(invitation => invitation.ProjectId);
                contributorInvitationBuilder
                    .HasOne(invitation => invitation.InvitedUser)
                    .WithMany(user => user.InvitationsReceived)
                    .HasForeignKey(invitation => invitation.InvitedUserId);
                contributorInvitationBuilder
                    .HasOne(invitation => invitation.InvitingUser)
                    .WithMany(user => user.InvitationsSent)
                    .HasForeignKey(invitation => invitation.InvitingUserId);
                
                contributorInvitationBuilder.HasIndex(invitation => new{invitation.ProjectId, invitation.InvitingUserId, invitation.InvitedUserId}).IsUnique();
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