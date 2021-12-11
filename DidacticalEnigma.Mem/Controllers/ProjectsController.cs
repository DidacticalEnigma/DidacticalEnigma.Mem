using System.Threading.Tasks;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Mappings;
using DidacticalEnigma.Mem.Translation;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Projects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DidacticalEnigma.Mem.Controllers
{
    [ApiController]
    [Route("mem")]
    public class ProjectsController : ControllerBase
    {
        [SwaggerOperation(OperationId = "AcceptInvitation")]
        [HttpPost("projects/invitations/accept")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<AcceptInvitationResult>> AcceptInvitation(
            [FromBody] AcceptInvitationParams acceptInvitationParams,
            [FromServices] AcceptInvitationHandler acceptInvitationHandler)
        {
            var result = await acceptInvitationHandler.Accept(
                Request.GetUserName(),
                acceptInvitationParams.ProjectName,
                acceptInvitationParams.InvitingUserName);
            return result.Unwrap(new AcceptInvitationResult());
        }
        
        [SwaggerOperation(OperationId = "RejectInvitation")]
        [HttpPost("projects/invitations/reject")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<RejectInvitationResult>> RejectInvitation(
            [FromBody] RejectInvitationParams rejectInvitationParams,
            [FromServices] RejectInvitationHandler rejectInvitationHandler)
        {
            var result = await rejectInvitationHandler.Reject(
                Request.GetUserName(),
                rejectInvitationParams.ProjectName,
                rejectInvitationParams.InvitingUserName);
            return result.Unwrap(new RejectInvitationResult());
        }
        
        [SwaggerOperation(OperationId = "CancelInvitation")]
        [HttpPost("projects/invitations/cancel")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<CancelInvitationResult>> CancelInvitation(
            [FromBody] CancelInvitationParams rejectInvitationParams,
            [FromServices] CancelInvitationHandler cancelInvitationHandler)
        {
            var result = await cancelInvitationHandler.Reject(
                Request.GetUserName(),
                rejectInvitationParams.ProjectName,
                rejectInvitationParams.InvitedUserName);
            return result.Unwrap(new CancelInvitationResult());
        }
        
        [SwaggerOperation(OperationId = "SendInvitation")]
        [HttpPost("projects/invitations")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<SendInvitationResult>> SendInvitation(
            [FromQuery] string projectName,
            [FromBody] SendInvitationParams sendInvitationParams,
            [FromServices] SendInvitationHandler sendInvitationHandler)
        {
            var result = await sendInvitationHandler.Send(
                Request.GetUserName(),
                projectName,
                sendInvitationParams.InvitedUserName);
            return result.Unwrap(new SendInvitationResult());
        }
        
        [SwaggerOperation(OperationId = "RemoveContributor")]
        [HttpDelete("projects/contributors")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<RemoveContributorResult>> RemoveContributor(
            [FromQuery] string projectName,
            [FromQuery] string contributorName,
            [FromServices] RemoveContributorHandler removeContributorHandler)
        {
            var result = await removeContributorHandler.Remove(
                Request.GetUserName(),
                projectName,
                contributorName);
            return result.Unwrap(new RemoveContributorResult());
        }
        
        [SwaggerOperation(OperationId = "QueryInvitations")]
        [HttpGet("projects/invitations")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<QueryInvitationsResult>> QueryInvitations(
            [FromServices] ListInvitationsHandler listInvitationsHandler)
        {
            var result = await listInvitationsHandler.Query(
                Request.GetUserName());
            return result.Unwrap();
        }
        
        [SwaggerOperation(OperationId = "AddProject")]
        [HttpPost("projects")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<AddProjectResult>> AddProject(
            [FromQuery] string projectName,
            [FromQuery] bool publicallyReadable,
            [FromServices] AddProjectHandler addProjectHandler)
        {
            var result = await addProjectHandler.Add(
                Request.GetUserName(),
                projectName,
                publicallyReadable);
            return result.Unwrap(new AddProjectResult());
        }
        
        [SwaggerOperation(OperationId = "DeleteProject")]
        [HttpDelete("projects")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<DeleteProjectResult>> DeleteProject(
            [FromQuery] string projectName,
            [FromServices] DeleteProjectHandler deleteProjectHandler)
        {
            var result = await deleteProjectHandler.Delete(
                Request.GetUserName(),
                projectName);
            return result.Unwrap(new DeleteProjectResult());
        }
        
        [SwaggerOperation(OperationId = "ListProjects")]
        [HttpGet("projects")]
        [Authorize("ApiAllowAnonymous")]
        public async Task<ActionResult<QueryProjectsResult>> ListProjects(
            [FromServices] ListProjectsHandler listProjectsHandler)
        {
            var result = await listProjectsHandler.Query(
                Request.GetUserName());
            return result.Unwrap();
        }
    }
}