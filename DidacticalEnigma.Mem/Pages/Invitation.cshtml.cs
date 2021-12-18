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
    public class InvitationModel : PageModel
    {
        private readonly SendInvitationHandler sendInvitationHandler;

        public InvitationModel(
            SendInvitationHandler sendInvitationHandler)
        {
            this.sendInvitationHandler = sendInvitationHandler;
        }

        public async Task<ActionResult> OnPost(string project, string? search, string invited)
        {
            var userName = this.Request.GetUserName();

            var result = await this.sendInvitationHandler.Send(
                userName,
                project,
                invited);

            if (result.Error == null)
            {
                return RedirectToPage("/Project", new
                {
                    project = project,
                    search = search,
                    inviteMessage = "Successfully sent an invite!",
                    inviteError = null as string
                });
            }
            else
            {
                return RedirectToPage("/Project", new
                {
                    project = project,
                    search = search,
                    inviteMessage = null as string,
                    inviteError = result.Error.Message
                });
            }
        }
    }
}