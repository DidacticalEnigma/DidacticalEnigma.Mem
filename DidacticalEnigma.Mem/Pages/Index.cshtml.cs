using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Projects;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DidacticalEnigma.Mem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly ListInvitationsHandler listInvitationsHandler;
        private readonly ListProjectsHandler listProjectsHandler;

        public IndexModel(
            ILogger<IndexModel> logger,
            ListInvitationsHandler listInvitationsHandler,
            ListProjectsHandler listProjectsHandler)
        {
            this.logger = logger;
            this.listInvitationsHandler = listInvitationsHandler;
            this.listProjectsHandler = listProjectsHandler;
        }
        
        public IReadOnlyCollection<QueryProjectResult> Projects { get; private set; }
        
        public IReadOnlyCollection<QueryInvitationReceivedResult> Invitations { get; private set; }

        public async Task OnGetAsync()
        {
            var userName = this.Request.GetUserName();
            
            var listResult = await this.listProjectsHandler.Query(
                userName);
            var listInvitations = await this.listInvitationsHandler.Query(
                userName);

            Projects = (listResult.Value?.Projects ?? Array.Empty<QueryProjectResult>())
                .Where(project => project.CanContribute)
                .ToList();
            
            Invitations = listInvitations.Value?.InvitationsReceived ?? Array.Empty<QueryInvitationReceivedResult>();
        }
    }
}