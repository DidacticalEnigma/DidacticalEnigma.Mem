using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Models;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Projects;
using DidacticalEnigma.Mem.Translation.Translations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DidacticalEnigma.Mem.Pages
{
    public class TranslationsModel : PageModel
    {
        private readonly QueryTranslationsHandler queryTranslationsHandler;

        public TranslationsModel(
            QueryTranslationsHandler queryTranslationsHandler)
        {
            this.queryTranslationsHandler = queryTranslationsHandler;
        }
        
        public string SearchText { get; private set; }
        
        public QueryProjectShowcaseResult? Project { get; private set; }
        
        public string? PaginationToken { get; private set; }

        public IEnumerable<TranslationEntry> Translations { get; private set; } =
            Array.Empty<TranslationEntry>();

        public async Task OnGetAsync(
            string search,
            string? paginationToken = null)
        {
            var userName = this.Request.GetUserName();
            
            var rawResult = await this.queryTranslationsHandler.Query(
                userName,
                null,
                null,
                search,
                null,
                paginationToken,
                limit: 100);
            
            SearchText = search ?? "";
            var result = rawResult.Value;
            Translations = (result?.Translations ?? Array.Empty<QueryTranslationResult>())
                .Select(t => new TranslationEntry()
                {
                    Project = t.ProjectName,
                    CorrelationId = t.CorrelationId,
                    Source = t.Source,
                    Target = t.Target,
                    Highlight = t.HighlighterSequence
                });
            PaginationToken = result?.PaginationToken;
        }
    }
}