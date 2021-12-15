using System.Threading.Tasks;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Translation.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DidacticalEnigma.Mem.Pages
{
    public class InvitationAcceptedModel : PageModel
    {
        private readonly AcceptInvitationHandler acceptInvitationHandler;

        public InvitationAcceptedModel(
            AcceptInvitationHandler acceptInvitationHandler)
        {
            this.acceptInvitationHandler = acceptInvitationHandler;
        }
        
        public string Message { get; private set; }

        public async Task OnGetAsync(string message)
        {
            Message = message;
        }

        public async Task<ActionResult> OnPostAsync(string project, string user)
        {
            var userName = Request.GetUserName();

            var result = await this.acceptInvitationHandler.Accept(
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