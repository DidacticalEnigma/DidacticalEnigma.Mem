using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Projects;
using DidacticalEnigma.Mem.Translation.Translations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DidacticalEnigma.Mem.Pages
{
    public class ProjectModel : PageModel
    {
        private readonly QueryProjectShowcaseHandler queryProjectShowcaseHandler;
        private readonly SendInvitationHandler sendInvitationHandler;
        private readonly QueryTranslationsHandler queryTranslationsHandler;

        public ProjectModel(
            QueryProjectShowcaseHandler queryProjectShowcaseHandler,
            SendInvitationHandler sendInvitationHandler,
            QueryTranslationsHandler queryTranslationsHandler)
        {
            this.queryProjectShowcaseHandler = queryProjectShowcaseHandler;
            this.sendInvitationHandler = sendInvitationHandler;
            this.queryTranslationsHandler = queryTranslationsHandler;
        }

        public string ProjectTitle => Project?.ProjectName != null
            ? $"Project '{Project.ProjectName}'"
            : "Project not found";
        
        public string SearchText { get; private set; }
        
        public QueryProjectShowcaseResult? Project { get; private set; }

        public IEnumerable<QueryTranslationResult> Translations { get; private set; } =
            Array.Empty<QueryTranslationResult>();
        
        public bool IsContributor { get; private set; }

        public async Task OnGetAsync(
            string project,
            string? search)
        {
            var userName = this.Request.GetUserName();
            
            var showcaseResult = await this.queryProjectShowcaseHandler.Query(
                userName,
                project);

            var p = showcaseResult.Value;
            IsContributor = p?.Owner != null;
            Project = p;
            SearchText = search ?? "";

            if (search != null)
            {
                var result = await this.queryTranslationsHandler.Query(
                    userName,
                    project,
                    null,
                    search,
                    null,
                    null,
                    100);

                Translations = result.Value?.Translations ?? Array.Empty<QueryTranslationResult>();
            }
        }
    }
}