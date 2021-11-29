using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Translation.Extensions;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DidacticalEnigma.Mem.Translation.Services.TranslationMemory.Contexts
{
    public class GetContextData
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public GetContextData(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<FileResult, Unit>> Get(Guid id)
        {
            var contextData = await this.dbContext.Contexts
                .Select(context => new
                {
                    Id = context.Id,
                    ContentObjectId = context.ContentObjectId,
                    MediaType = context.MediaType != null ? context.MediaType.MediaType : null,
                    Extension = context.MediaType != null ? context.MediaType.Extension : null,
                })
                .FirstOrDefaultAsync(contextData => contextData.Id == id);
            
            if (contextData == null)
            {
                return Result<FileResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "context not found");
            }

            if (contextData.ContentObjectId == null || contextData.MediaType == null)
            {
                return Result<FileResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "context has no associated binary data");
            }
            
            await this.dbContext.Database.OpenConnectionAsync();
            var connection = (NpgsqlConnection)this.dbContext.Database.GetDbConnection();
            // we have to begin a transaction, in order for LOB access to work
            // (otherwise we get a "invalid large-object descriptor: 0" error)
            // but we can't dispose it in this method, so we wrap the created stream
            // in a wrapper that disposes another object after disposing the stream
            var transaction = await this.dbContext.Database.BeginTransactionAsync();

            var lobManager = new NpgsqlLargeObjectManager(connection);

            return Result<FileResult, Unit>.Ok(new FileResult()
            {
                Content = new DisposingStream(
                    await lobManager.OpenReadAsync(contextData.ContentObjectId.Value),
                    transaction),
                FileName = $"{contextData.Id}.{contextData.Extension}",
                MediaType = contextData.MediaType
            });
        }
    }
}