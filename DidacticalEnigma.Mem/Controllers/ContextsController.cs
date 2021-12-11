using System;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Mappings;
using DidacticalEnigma.Mem.Translation;
using DidacticalEnigma.Mem.Translation.Contexts;
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
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<AddContextResult>> AddContext(
            [FromForm] AddContextParams request,
            [FromServices] AddContextHandler addContextHandler)
        {
            var result = await addContextHandler.Add(
                Request.GetUserId(),
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
        [Authorize("ApiAllowAnonymous")]
        public async Task<ActionResult<QueryContextsResult>> GetContext(
            [FromQuery] Guid? id,
            [FromQuery] string? projectId,
            [FromQuery] string? correlationId,
            [FromServices] GetContextsHandler getContextsHandler)
        {
            var result = await getContextsHandler.Get(
                Request.GetUserId(),
                id,
                projectId,
                correlationId);
            return result.Unwrap();
        }
        
        [SwaggerOperation(OperationId = "GetContextData")]
        [HttpGet("contexts/data")]
        [Authorize("ApiAllowAnonymous")]
        public async Task<ActionResult> GetContextData(
            [FromQuery] Guid id,
            [FromServices] GetContextDataHandler getContextDataHandler)
        {
            var result = await getContextDataHandler.Get(
                Request.GetUserId(),
                id);
            return result.UnwrapFile();
        }
        
        [SwaggerOperation(OperationId = "DeleteContext")]
        [HttpDelete("contexts")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<DeleteContextResult>> DeleteContext(
            [FromQuery] Guid id,
            [FromServices] DeleteContextHandler deleteContextHandler)
        {
            var result = await deleteContextHandler.Delete(
                Request.GetUserId(),
                id);
            return result.Unwrap(new DeleteContextResult());
        }
    }
}