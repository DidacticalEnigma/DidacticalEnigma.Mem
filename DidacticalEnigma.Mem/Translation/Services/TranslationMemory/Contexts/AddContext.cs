using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.StoredModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DidacticalEnigma.Mem.Translation.Services.TranslationMemory.Contexts
{
    public class AddContext
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public AddContext(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<Unit, Unit>> Add(
            Guid id,
            string correlationId,
            string projectName,
            Stream? content,
            string? mediaType,
            string? text)
        {
            var context = await this.dbContext.Contexts.FindAsync(id);
            if (context != null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "request with such a guid exists");
            }

            if (text == null && mediaType == null && content == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "either text or content (with its media type) must be specified");
            }
            
            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(project => project.Name == projectName);

            if (project == null)
            {
                return Result<Unit, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "project does not exist");
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
                        ""MediaTypeId"")
                    VALUES (
                        {id},
                        {project.Id},
                        {correlationId},
                        {text},
                        {contentObjectId},
                        {mediaTypeModel?.Id});");

                await transaction.CommitAsync();
            }
            return Result<Unit, Unit>.Ok(Unit.Value);
        }
    }
}