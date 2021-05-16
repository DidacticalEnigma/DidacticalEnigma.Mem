using System;
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
        
        public DbSet<User> Users { get; set; }
        
        public DbSet<Group> Groups { get; set; }
        
        public DbSet<UserGroupMembership> UserGroupMemberships { get; set; }
        
        public DbSet<GroupProjectClaim> GroupProjectClaims { get; set; }

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
                projectBuilder.Property(project => project.Name).HasMaxLength(128);
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
                
                translationPairBuilder
                    .HasOne(translationPair => translationPair.Context)
                    .WithMany();
                
                translationPairBuilder.HasIndex(translationPair => translationPair.CorrelationId);
                translationPairBuilder.HasIndex(translationPair => new{translationPair.ParentId,translationPair.CorrelationId}).IsUnique();
                
                translationPairBuilder
                    .HasOne(translationPair => translationPair.Author)
                    .WithMany();
            }
            {
                var contextBuilder = modelBuilder.Entity<Context>();
                contextBuilder.HasKey(context => context.Id);
                contextBuilder.Property(context => context.Content).HasColumnType("bytea").IsRequired(false);
                contextBuilder.Property(context => context.Text).HasMaxLength(512).IsRequired(false);
                
                contextBuilder
                    .HasOne(context => context.MediaType)
                    .WithMany();
                
                contextBuilder
                    .HasOne(context => context.Author)
                    .WithMany();
            }
            {
                var npgsqlQueryBuilder = modelBuilder.Entity<NpgsqlQuery>();
                npgsqlQueryBuilder.HasNoKey();
                npgsqlQueryBuilder.Property(context => context.Vec);
            }
            {
                var userGroupMembershipBuilder = modelBuilder.Entity<UserGroupMembership>();
                userGroupMembershipBuilder.HasKey(membership => new {membership.GroupId, membership.UserId});
                userGroupMembershipBuilder
                    .HasOne(membership => membership.Group)
                    .WithMany(group => group.Users)
                    .HasForeignKey(membership => membership.GroupId);
                userGroupMembershipBuilder
                    .HasOne(membership => membership.User)
                    .WithMany(user => user.Groups)
                    .HasForeignKey(membership => membership.UserId);
            }
            {
                var groupProjectClaimBuilder = modelBuilder.Entity<GroupProjectClaim>();
                groupProjectClaimBuilder.HasKey(claim => new {claim.GroupId, claim.ProjectId});
                groupProjectClaimBuilder
                    .HasOne(claim => claim.Group)
                    .WithMany(group => group.ProjectClaims)
                    .HasForeignKey(claim => claim.GroupId);
                groupProjectClaimBuilder
                    .HasOne(claim => claim.Project)
                    .WithMany()
                    .HasForeignKey(claim => claim.ProjectId);

                groupProjectClaimBuilder.Property(claim => claim.CanAddTranslations).IsRequired();
                groupProjectClaimBuilder.Property(claim => claim.CanDeleteTranslations).IsRequired();
                groupProjectClaimBuilder.Property(claim => claim.CanReadTranslations).IsRequired();
            }
            {
                var userBuilder = modelBuilder.Entity<User>();
                userBuilder.HasKey(user => user.Id);
                userBuilder.HasAlternateKey(user => user.Name);
                userBuilder.Property(user => user.Name).HasMaxLength(32);
                userBuilder.HasData(new User[]
                {
                    new User()
                    {
                        Id = User.AnonymousUserId,
                        Groups = new UserGroupMembership[0],
                        Name = "<anonymous user>",
                        IsSpecialUser = true
                    },
                    new User()
                    {
                        Id = User.AdminUserId,
                        Groups = new UserGroupMembership[0],
                        Name = "<administrator>",
                        IsSpecialUser = true
                    },
                });
            }
            {
                var groupBuilder = modelBuilder.Entity<Group>();
                groupBuilder.HasKey(group => group.Id);
                groupBuilder.HasAlternateKey(group => group.GroupName);
                groupBuilder.Property(group => group.CanAddContexts).IsRequired();
                groupBuilder.Property(group => group.CanDeleteContexts).IsRequired();
                groupBuilder.Property(group => group.CanAddProjects).IsRequired();
                groupBuilder.Property(group => group.CanDeleteProjects).IsRequired();

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