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
        
        [SwaggerOperation(OperationId = "Add")]
        [HttpPost("translations")]
        public async Task<ActionResult<AddTranslationResult>> Add(
            [FromQuery] string projectName,
            [FromBody] AddTranslations request,
            [FromServices] ITranslationMemory translationMemory)
        {
            foreach (var addContext in request.Contexts ?? Enumerable.Empty<AddContext>())
            {
                var result = await translationMemory.AddContext(
                    addContext.Id,
                    addContext.Content,
                    addContext.MediaType,
                    addContext.Text);
                if (result.Error != null)
                {
                    return Unwrap(result, new AddTranslationResult());
                }
            }

            if (request.Translations != null)
            {
                var result = await translationMemory.AddTranslations(projectName, request.Translations);
                if (result.Error != null)
                {
                    return Unwrap(result, new AddTranslationResult());
                }
            }
            
            await translationMemory.SaveChanges();
            return Ok(new AddTranslationResult());
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
        [HttpGet("contexts/{contextId}")]
        public async Task<ActionResult<QueryContextResult>> GetContext(
            [FromRoute] Guid contextId,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.GetContext(contextId);
            
            return Unwrap(result);
        }

        private ActionResult<T> Unwrap<T>(Result<T> result)
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