using System.Threading.Tasks;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Translation.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DidacticalEnigma.Mem.Pages
{
    public class InvitationRejectedModel : PageModel
    {
        private readonly RejectInvitationHandler rejectInvitationHandler;

        public InvitationRejectedModel(
            RejectInvitationHandler rejectInvitationHandler)
        {
            this.rejectInvitationHandler = rejectInvitationHandler;
        }
        
        public string Message { get; private set; }

        public async Task OnGetAsync(string message)
        {
            Message = message;
        }

        public async Task<ActionResult> OnPostAsync(string project, string user)
        {
            var userName = Request.GetUserName();

            var result = await this.rejectInvitationHandler.Reject(
                userName,
                project,
                user);

            if (result.Error != null)
            {
                return RedirectToPage(new
                {
                    message = result.Error.Message
                });
            }
            else
            {
                return RedirectToPage("/Index");
            }
            
        }
    }
}