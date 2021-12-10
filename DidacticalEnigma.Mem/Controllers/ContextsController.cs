using System;
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
    public class ContextsController : ControllerBase
    {
        [SwaggerOperation(OperationId = "AddContext")]
        [HttpPost("contexts")]
        [Authorize("ModifyContexts", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<ActionResult<AddContextResult>> AddContext(
            [FromForm] AddContextParams request,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.AddContext(
                request.Id,
                request.CorrelationId,
                request.ProjectName,
                request.Content.OpenReadStream(),
                string.IsNullOrEmpty(request.ContentTypeOverride)
                    ? request.Content.ContentType
                    : request.ContentTypeOverride,
                request.Text);
            return result.Unwrap(new AddContextResult());
        }
        
        [SwaggerOperation(OperationId = "GetContexts")]
        [HttpGet("contexts")]
        [Authorize("ReadContexts", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<ActionResult<QueryContextsResult>> GetContext(
            [FromQuery] Guid? id,
            [FromQuery] string? projectId,
            [FromQuery] string? correlationId,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.GetContexts(id, projectId, correlationId);
            return result.Unwrap();
        }
        
        [SwaggerOperation(OperationId = "GetContextData")]
        [HttpGet("contexts/data")]
        [Authorize("ReadContexts", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetContextData(
            [FromQuery] Guid id,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.GetContextData(id);
            return result.UnwrapFile();
        }
        
        [SwaggerOperation(OperationId = "DeleteContext")]
        [HttpDelete("contexts")]
        [Authorize("ModifyContexts", AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public async Task<ActionResult<DeleteContextResult>> DeleteContext(
            [FromQuery] Guid id,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.DeleteContext(id);
            return result.Unwrap(new DeleteContextResult());
        }
    }
}