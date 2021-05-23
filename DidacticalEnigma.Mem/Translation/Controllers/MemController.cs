using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace DidacticalEnigma.Mem.Translation.Controllers
{
    [ApiController]
    [Route("mem")]
    public class MemController : ControllerBase
    {
        [SwaggerOperation(OperationId = "AddProject")]
        [HttpPost("projects")]
        public async Task<ActionResult<AddProjectResult>> AddProject(
            [FromQuery] string projectName,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.AddProject(projectName);
            await translationMemory.SaveChanges();
            return Unwrap(result, new AddProjectResult());
        }
        
        [SwaggerOperation(OperationId = "AddTranslations")]
        [HttpPost("translations")]
        public async Task<ActionResult<AddTranslationsResult>> AddTranslations(
            [FromQuery] string projectName,
            [FromBody] AddTranslationsParams request,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.AddTranslations(projectName, request.Translations);
            if (result.Error != null)
            {
                return Unwrap(result, new AddTranslationsResult());
            }

            await translationMemory.SaveChanges();
            return Ok(new AddTranslationsResult());
        }

        [SwaggerOperation(OperationId = "AddContexts")]
        [HttpPost("contexts")]
        public async Task<ActionResult<AddContextsResult>> AddContexts(
            [FromBody] AddContextsParams request,
            [FromServices] ITranslationMemory translationMemory)
        {
            foreach (var addContext in request.Contexts ?? Enumerable.Empty<AddContextParams>())
            {
                var result = await translationMemory.AddContext(
                    addContext.Id,
                    addContext.Content,
                    addContext.MediaType,
                    addContext.Text);
                if (result.Error != null)
                {
                    return Unwrap(result, new AddContextsResult());
                }
            }
            await translationMemory.SaveChanges();
            return Ok(new AddContextsResult());
        }
    
        [SwaggerOperation(OperationId = "Query")]
        [HttpGet("translations")]
        public async Task<ActionResult<QueryResult>> Query(
            [FromQuery] string? projectName,
            [FromQuery] string? correlationId,
            [FromQuery] string query,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.Query(projectName, correlationId, query);
            
            return Unwrap(result);
        }
        
        [SwaggerOperation(OperationId = "GetContext")]
        [HttpGet("contexts")]
        public async Task<ActionResult<QueryContextResult>> GetContext(
            [FromQuery] Guid id,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.GetContext(id);
            
            return Unwrap(result);
        }
        
        [SwaggerOperation(OperationId = "DeleteContext")]
        [HttpDelete("contexts")]
        public async Task<ActionResult<DeleteContextResult>> DeleteContext(
            [FromQuery] Guid id,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.DeleteContext(id);
            await translationMemory.SaveChanges();
            return Unwrap(result, new DeleteContextResult());
        }
        
        [SwaggerOperation(OperationId = "DeleteTranslation")]
        [HttpDelete("translations")]
        public async Task<ActionResult<DeleteTranslationResult>> DeleteTranslation(
            [FromQuery] string projectName,
            [FromQuery] string correlationId,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.DeleteTranslation(projectName, correlationId);
            await translationMemory.SaveChanges();
            return Unwrap(result, new DeleteTranslationResult());
        }
        
        [SwaggerOperation(OperationId = "DeleteProject")]
        [HttpDelete("projects")]
        public async Task<ActionResult<DeleteProjectResult>> DeleteProject(
            [FromQuery] string projectName,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.DeleteProject(projectName);
            await translationMemory.SaveChanges();
            return Unwrap(result, new DeleteProjectResult());
        }
        
        [SwaggerOperation(OperationId = "UpdateTranslation")]
        [HttpPatch("translations")]
        public async Task<ActionResult<UpdateTranslationResult>> UpdateTranslation(
            [FromQuery] string projectName,
            [FromQuery] string correlationId,
            [FromBody] UpdateTranslationParams request,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.UpdateTranslation(
                projectName,
                correlationId,
                request.Source,
                request.Target,
                request.Context);
            await translationMemory.SaveChanges();
            return Unwrap(result, new UpdateTranslationResult());
        }

        private ActionResult<T> Unwrap<T>(Result<T> result) where T : notnull
        {
            if (result.Error == null)
            {
                return Ok(result.Value);
            }
            else
            {
                return this.StatusCode((int)result.Error.Code, result.Error);
            }
        }
        
        private ActionResult<T> Unwrap<T>(Result<Unit> result, T value)
        {
            if (result.Error == null)
            {
                return Ok(value);
            }
            else
            {
                return this.StatusCode((int)result.Error.Code, result.Error);
            }
        }
    }
}