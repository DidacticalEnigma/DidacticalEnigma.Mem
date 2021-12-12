using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DidacticalEnigma.Mem.Extensions;
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
            : "Translation not found";
        
        public QueryTranslationResult? Translation { get; private set; }

        public string? ContextDataLink => Context?.HasData == true
            ? $"/mem/contexts/data?id={HttpUtility.UrlEncode(Context.Id.ToString())}"
            : null;
        
        public QueryContextResult? Context { get; private set; }

        public async Task OnGetAsync(string project, string translation)
        {
            var userName = this.Request.GetUserName();
            
            var translationResult = await this.queryTranslationsHandler.Query(
                userName,
                project,
                translation,
                null,
                null,
                null,
                5);

            var contextResult = await this.getContextsHandler.Get(
                userName,
                null,
                project,
                translation);

            this.Translation = translationResult.Value?.Translations.FirstOrDefault();
            this.Context = 
                contextResult.Value?.Contexts.FirstOrDefault(c => c.CorrelationId == translation) ??
                contextResult.Value?.Contexts.FirstOrDefault();
        }
    }
}