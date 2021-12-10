using System.Threading.Tasks;
using DidacticalEnigma.Mem.Mappings;
using DidacticalEnigma.Mem.Translation;
using DidacticalEnigma.Mem.Translation.IoModels;
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
        [SwaggerOperation(OperationId = "AddProject")]
        [HttpPost("projects")]
        [Authorize("ModifyProjects", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<ActionResult<AddProjectResult>> AddProject(
            [FromQuery] string projectName,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.AddProject(projectName);
            return result.Unwrap(new AddProjectResult());
        }
        
        [SwaggerOperation(OperationId = "DeleteProject")]
        [HttpDelete("projects")]
        [Authorize("ModifyProjects", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<ActionResult<DeleteProjectResult>> DeleteProject(
            [FromQuery] string projectName,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.DeleteProject(projectName);
            return result.Unwrap(new DeleteProjectResult());
        }
        
        [SwaggerOperation(OperationId = "ListProjects")]
        [HttpGet("projects")]
        [Authorize("EnumerateProjects", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<ActionResult<QueryProjectsResult>> ListProjects(
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.ListProjects();
            return result.Unwrap();
        }
    }
}