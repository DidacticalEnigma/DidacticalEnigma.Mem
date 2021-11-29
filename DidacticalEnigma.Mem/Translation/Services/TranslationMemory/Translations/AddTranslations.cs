using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.Translation.Extensions;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Mappings;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Services.TranslationMemory.Translations
{
    public class AddTranslations
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;

        public AddTranslations(
            MemContext dbContext,
            IMorphologicalAnalyzer<IpadicEntry> analyzer,
            ICurrentTimeProvider currentTimeProvider)
        {
            this.dbContext = dbContext;
            this.analyzer = analyzer;
            this.currentTimeProvider = currentTimeProvider;
        }
        
        public async Task<Result<AddTranslationsResult, Unit>> Add(
            string projectName,
            IReadOnlyCollection<AddTranslationParams> translations,
            bool allowPartialAdd = false)
        {
            var project = await this.dbContext.Projects.FirstOrDefaultAsync(p => p.Name == projectName);
            if (project == null)
            {
                return Result<AddTranslationsResult, Unit>.Failure(
                    HttpStatusCode.NotFound,
                    "no such project exists");
            }

            var inputTranslationIds = translations
                .Select(translation => translation.CorrelationId)
                .ToHashSet();

            if (inputTranslationIds.Count != translations.Count)
            {
                return Result<AddTranslationsResult, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "attempting to add multiple translations with the same id");
            }

            var alreadyExistingTranslationIds = (await this.dbContext.TranslationPairs
                    .Where(translation => translation.Parent.Name == projectName)
                    .Where(translation => inputTranslationIds.Contains(translation.CorrelationId))
                    .Select(translation => translation.CorrelationId)
                    .ToListAsync())
                .ToHashSet();

            if (!allowPartialAdd && alreadyExistingTranslationIds.Count != 0)
            {
                return Result<AddTranslationsResult, Unit>.Failure(
                    HttpStatusCode.BadRequest,
                    "there already exists a translation with given correlation id");
            }
            
            var currentTime = this.currentTimeProvider.GetCurrentTime();
            
            foreach (var translation in translations)
            {
                if (alreadyExistingTranslationIds.Contains(translation.CorrelationId))
                {
                    continue;
                }

                await this.dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $@"INSERT INTO ""TranslationPairs"" (
                        ""Id"",
                        ""CorrelationId"",
                        ""Source"",
                        ""SearchVector"",
                        ""Target"",
                        ""CategoryId"",
                        ""ParentId"",
                        ""CreationTime"",
                        ""ModificationTime"",
                        ""Notes"",
                        ""AssociatedData"")
                    VALUES (
                        {Guid.NewGuid()},
                        {translation.CorrelationId},
                        {translation.Source},
                        to_tsvector('simple', {analyzer.Normalize(translation.Source)}),
                        {translation.Target},
                        {translation.CategoryId},
                        {project.Id},
                        {currentTime},
                        {currentTime},
                        {JsonSerializer.Serialize(Mapper.Map(translation.TranslationNotes))}::jsonb,
                        {JsonSerializer.Serialize(translation.AssociatedData)}::jsonb);");
            }
            
            await this.dbContext.SaveChangesAsync();
            return Result<AddTranslationsResult, Unit>.Ok(new AddTranslationsResult()
            {
                NotAdded = alreadyExistingTranslationIds
            });
        }
    }
}