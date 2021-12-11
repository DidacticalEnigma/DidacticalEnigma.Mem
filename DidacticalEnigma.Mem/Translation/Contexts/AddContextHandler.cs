using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DidacticalEnigma.Mem.Translation.Contexts
{
    public class AddContextHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        private readonly UserManager<User> userManager;

        public AddContextHandler(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
            this.userManager = userManager;
        }
        
        public async Task<Result<Unit, Unit>> Add(
            string? userName,
            Guid id,
            string correlationId,
            string projectName,
            Stream? content,
            string? mediaType,
            string? text)
        {
            if (text == null && mediaType == null && content == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "either text or content (with its media type) must be specified");
            }
            
            var context = await this.dbContext.Contexts.FindAsync(id);
            if (context != null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "request with such a guid exists");
            }

            var user = await this.userManager.FindByNameAsync(userName);

            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(project =>
                    (project.Owner.UserName == userName
                     || project.Contributors.Any(contributor => contributor.User.UserName == userName)) &&
                    project.Name == projectName);

            if (project == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "project not found");
            }

            if (content != null && mediaType == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "if a request provides content, its media type must be specified");
            }
            
            if (content == null && mediaType != null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "no content provided");
            }

            AllowedMediaType? mediaTypeModel = null;
            if (mediaType != null)
            {
                mediaTypeModel = await this.dbContext.MediaTypes.FirstOrDefaultAsync(m => m.MediaType == mediaType);
                if (mediaTypeModel == null)
                {
                    return Result<Unit, Unit>.Failure(
                        HttpStatusCode.BadRequest,
                        "media type not acceptable");
                }
            }

            uint? contentObjectId = null;
            
            await this.dbContext.Database.OpenConnectionAsync();
            var connection = (NpgsqlConnection)this.dbContext.Database.GetDbConnection();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                if (content != null)
                {
                    var lobManager = new NpgsqlLargeObjectManager(connection);
                    var contentOid = await lobManager.CreateAsync(0);

                    using (var stream = await lobManager.OpenReadWriteAsync(contentOid))
                    {
                        await content.CopyToAsync(stream);
                    }

                    contentObjectId = contentOid;
                }

                await this.dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $@"INSERT INTO ""Contexts"" (
                        ""Id"",
                        ""ProjectId"",
                        ""CorrelationId"",
                        ""Text"",
                        ""ContentObjectId"",
                        ""MediaTypeId"",
                        ""CreatedById"",
                        ""CreationTime"")
                    VALUES (
                        {id},
                        {project.Id},
                        {correlationId},
                        {text},
                        {contentObjectId},
                        {mediaTypeModel?.Id},
                        {user.Id},
                        {currentTimeProvider.GetCurrentTime()});");

                await transaction.CommitAsync();
            }
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}