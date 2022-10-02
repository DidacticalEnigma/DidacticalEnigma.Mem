using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Models;
using DidacticalEnigma.Mem.Translation.Contexts;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Projects;
using DidacticalEnigma.Mem.Translation.Translations;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DidacticalEnigma.Mem.Pages
{
    public class TranslationModel : PageModel
    {
        private readonly QueryTranslationsHandler queryTranslationsHandler;
        private readonly GetContextsHandler getContextsHandler;

        public TranslationModel(
            QueryTranslationsHandler queryTranslationsHandler,
            GetContextsHandler getContextsHandler)
        {
            this.queryTranslationsHandler = queryTranslationsHandler;
            this.getContextsHandler = getContextsHandler;
        }

        public string PageTitle => string.Join(" ", new [] { ProjectTitle, Context?.Text }.Where(x => x != null));

        public string ProjectTitle => Translation != null
            ? Translation.ProjectName
            : RequestedProjectName;
        
        public QueryTranslationResult? Translation { get; private set; }

        public IEnumerable<TranslationEntry> Children { get; private set; } = Array.Empty<TranslationEntry>();

        public string? ContextDataLink => Context?.HasData == true
            ? $"/mem/contexts/data?id={HttpUtility.UrlEncode(Context.Id.ToString())}"
            : null;
        
        public QueryContextResult? Context { get; private set; }
        
        public string RequestedProjectName { get; private set; }
        
        public string RequestedTranslation { get; private set; }

        public string? PaginationToken { get; private set; }
        
        public IEnumerable<KeyValuePair<string, string>> CorrelationIdComponents { get; private set; }

        public async Task OnGetAsync(string project, string translation, string? paginationToken = null)
        {
            var userName = this.Request.GetUserName();

            RequestedProjectName = project;
            RequestedTranslation = translation;
            
            var translationResult = await this.queryTranslationsHandler.Query(
                userName,
                project,
                translation,
                null,
                null,
                paginationToken,
                50);

            var contextResult = await this.getContextsHandler.Get(
                userName,
                null,
                project,
                translation);

            this.Translation = translationResult.Value?.Translations.FirstOrDefault(t => t.CorrelationId == translation);
            this.Children = translationResult.Value?.Translations
                .Where(t => t.CorrelationId != translation)
                .Select(t => new TranslationEntry()
                {
                    Project = t.ProjectName,
                    CorrelationId = t.CorrelationId,
                    Source = t.Source,
                    Target = t.Target,
                    Highlight = t.HighlighterSequence
                })
                .ToArray() ?? Array.Empty<TranslationEntry>();

            this.PaginationToken = translationResult.Value?.PaginationToken;
            
            this.Context = 
                contextResult.Value?.Contexts.FirstOrDefault(c => c.CorrelationId == translation) ??
                contextResult.Value?.Contexts.FirstOrDefault();

            var correlationIdComponents = new List<KeyValuePair<string, string>>();
            var components = translation.Split("/", StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder("/");
            foreach (var component in components)
            {
                sb.Append(component);
                correlationIdComponents.Add(KeyValuePair.Create(component, sb.ToString()));
                sb.Append("/");
            }

            CorrelationIdComponents = correlationIdComponents;
        }
    }
}