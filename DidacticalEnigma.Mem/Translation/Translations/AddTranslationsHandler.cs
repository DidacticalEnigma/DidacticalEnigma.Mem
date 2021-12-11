using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Mem.DatabaseModels;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Mappings;
using DidacticalEnigma.Mem.Services;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidacticalEnigma.Mem.Translation.Translations
{
    public class AddTranslationsHandler
    {
        private readonly MemContext dbContext;
        private readonly IMorphologicalAnalyzer<IpadicEntry> analyzer;
        private readonly ICurrentTimeProvider currentTimeProvider;
        private readonly UserManager<User> userManager;

        public AddTranslationsHandler(
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
        
        public async Task<Result<AddTranslationsResult, Unit>> Add(
            string? userName,
            string projectName,
            IReadOnlyCollection<AddTranslationParams> translations,
            bool allowPartialAdd = false)
        {
            if (userName == null)
            {
                return Result<AddTranslationsResult, Unit>.Failure(
                    HttpStatusCode.Forbidden,
                    "unregistered users can't add translations");
            }

            var user = await userManager.FindByNameAsync(userName);
            
            var project = await this.dbContext.Projects
                .FirstOrDefaultAsync(p =>
                    p.Name == projectName &&
                    (p.Owner.UserName == userName ||
                     p.Contributors.Any(contributor => contributor.User.UserName == userName)));
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

            var translationsToAdd = translations
                .Where(translation =>
                    !alreadyExistingTranslationIds.Contains(translation.CorrelationId));
            
            foreach (var translation in translationsToAdd)
            {
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
                        ""AssociatedData"",
                        ""CreatedById"",
                        ""ModifiedById"")
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
                        {JsonSerializer.Serialize(translation.AssociatedData)}::jsonb,
                        {user.Id},
                        {user.Id});");
            }
            
            await this.dbContext.SaveChangesAsync();
            return Result<AddTranslationsResult, Unit>.Ok(new AddTranslationsResult()
            {
                NotAdded = alreadyExistingTranslationIds
            });
        }
    }
}