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
    public class ProjectModel : PageModel
    {
        private readonly QueryProjectShowcaseHandler queryProjectShowcaseHandler;
        private readonly SendInvitationHandler sendInvitationHandler;
        private readonly QueryTranslationsHandler queryTranslationsHandler;
        private readonly AddProjectHandler addProjectHandler;

        public ProjectModel(
            QueryProjectShowcaseHandler queryProjectShowcaseHandler,
            SendInvitationHandler sendInvitationHandler,
            QueryTranslationsHandler queryTranslationsHandler,
            AddProjectHandler addProjectHandler)
        {
            this.queryProjectShowcaseHandler = queryProjectShowcaseHandler;
            this.sendInvitationHandler = sendInvitationHandler;
            this.queryTranslationsHandler = queryTranslationsHandler;
            this.addProjectHandler = addProjectHandler;
        }

        public string ProjectTitle => Project?.ProjectName != null
            ? $"Project '{Project.ProjectName}'"
            : "Project not found";
        
        public string SearchText { get; private set; }
        
        public string? InviteMessage { get; private set; }
        
        public string? InviteError { get; private set; }
        
        public IEnumerable<TranslationEntry> RecentlyAdded { get; private set; }

        public IEnumerable<TranslationEntry> Untranslated { get; private set; }
        
        public QueryProjectShowcaseResult? Project { get; private set; }

        public IEnumerable<TranslationEntry> Translations { get; private set; } =
            Array.Empty<TranslationEntry>();
        
        public bool IsContributor { get; private set; }

        public async Task OnGetAsync(
            string project,
            string? search,
            string? inviteMessage,
            string? inviteError)
        {
            var userName = this.Request.GetUserName();
            
            var showcaseResult = await this.queryProjectShowcaseHandler.Query(
                userName,
                project);

            var p = showcaseResult.Value;
            IsContributor = p?.Owner != null;
            Project = p;
            SearchText = search ?? "";
            InviteMessage = inviteMessage;
            this.InviteError = inviteError;

            this.Untranslated = p?.UntranslatedLines
                .Select(t => new TranslationEntry()
                {
                    Project = project,
                    CorrelationId = t.CorrelationId,
                    Source = t.Source,
                    Target = t.Target,
                    Highlight = null
                }) ?? Array.Empty<TranslationEntry>();
            
            this.RecentlyAdded = p?.RecentAddedLines
                .Select(t => new TranslationEntry()
                {
                    Project = project,
                    CorrelationId = t.CorrelationId,
                    Source = t.Source,
                    Target = t.Target,
                    Highlight = null
                }) ?? Array.Empty<TranslationEntry>();

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

                Translations = (result.Value?.Translations ?? Array.Empty<QueryTranslationResult>())
                    .Select(t => new TranslationEntry()
                    {
                        Project = t.ProjectName,
                        CorrelationId = t.CorrelationId,
                        Source = t.Source,
                        Target = t.Target,
                        Highlight = t.HighlighterSequence
                    });
            }
        }

        public async Task<ActionResult> OnPost(string projectName)
        {
            var userName = this.Request.GetUserName();

            var result = await this.addProjectHandler.Add(
                userName,
                projectName,
                true);

            if (result.Error == null)
            {
                return RedirectToPage("/Project", new
                {
                    project = projectName
                });
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }
    }
}