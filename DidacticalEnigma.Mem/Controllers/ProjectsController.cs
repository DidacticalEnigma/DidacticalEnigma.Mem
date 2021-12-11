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
                Request.GetUserId(),
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
                Request.GetUserId(),
                rejectInvitationParams.ProjectName,
                rejectInvitationParams.InvitingUserName);
            return result.Unwrap(new RejectInvitationResult());
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
                Request.GetUserId(),
                projectName,
                sendInvitationParams.InvitedUserName);
            return result.Unwrap(new SendInvitationResult());
        }
        
        [SwaggerOperation(OperationId = "QueryInvitations")]
        [HttpGet("projects/invitations")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<QueryInvitationsResult>> QueryInvitations(
            [FromServices] ListInvitationsHandler listInvitationsHandler)
        {
            var result = await listInvitationsHandler.Query(
                Request.GetUserId());
            return result.Unwrap();
        }
        
        [SwaggerOperation(OperationId = "AddProject")]
        [HttpPost("projects")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<AddProjectResult>> AddProject(
            [FromQuery] string projectName,
            [FromServices] AddProjectHandler addProjectHandler)
        {
            var result = await addProjectHandler.Add(
                Request.GetUserId(),
                projectName);
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
                Request.GetUserId(),
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
                Request.GetUserId());
            return result.Unwrap();
        }
    }
}