using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Contexts
{
    public class GetContexts
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        
        public GetContexts(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<QueryContextsResult, Unit>> Get(
            Guid? id,
            string? projectName,
            string? correlationId)
        {
            if (id == null && correlationId == null && projectName == null)
            {
                return Result<QueryContextsResult, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "no parameters are provided");
            }
            
            if ((correlationId != null && projectName == null) || (correlationId == null && projectName != null))
            {
                return Result<QueryContextsResult, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "the search can be done either by direct id, or by projectName/correlationId combination");
            }
            
            var filteredContexts = this.dbContext.Contexts.AsQueryable();

            if (id != null)
            {
                filteredContexts = filteredContexts.Where(context => context.Id == id);
            }
            
            if (correlationId != null && projectName != null)
            {
                filteredContexts = filteredContexts.Where(context =>
                    context.Project.Name == projectName &&
                    context.CorrelationId.StartsWith(correlationId));
            }
            
            var contexts = (await filteredContexts
                    .Select(context => new
                    {
                        Id = context.Id,
                        HasData = context.ContentObjectId != null,
                        MediaType = context.MediaType != null ? context.MediaType.MediaType : null,
                        Text = context.Text,
                        CorrelationId = context.CorrelationId,
                        ProjectName = context.Project.Name
                    })
                    .ToListAsync())
                .Select(contextData => new QueryContextResult()
                {
                    Id = contextData.Id,
                    CorrelationId = contextData.CorrelationId,
                    MediaType = contextData.MediaType,
                    Text = contextData.Text,
                    ProjectName = contextData.ProjectName,
                    HasData = contextData.HasData,
                })
                .ToList();
            
            return Result<QueryContextsResult, Unit>.Ok(new QueryContextsResult()
            {
                Contexts = contexts
            });
        }
    }
}