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
        [SwaggerOperation("AddProject")]
        [HttpPost("projects")]
        public async Task<ActionResult> AddProject(
            [FromQuery] string projectName,
            [FromServices] ITranslationMemory translationMemory)
        {
            await translationMemory.AddProject(projectName);
            await translationMemory.SaveChanges();
            return Ok();
        }
        
        [SwaggerOperation("Add")]
        [HttpPost("translations")]
        public async Task<ActionResult<AddTranslationResult>> Add(
            [FromQuery] string projectName,
            [FromBody] AddTranslations request,
            [FromServices] ITranslationMemory translationMemory)
        {
            foreach (var addContext in request.Contexts ?? Enumerable.Empty<AddContext>())
            {
                await translationMemory.AddContext(
                    addContext.Id,
                    addContext.Content,
                    addContext.MediaType,
                    addContext.Text);
            }

            if (request.Translations != null)
            {
                await translationMemory.AddTranslations(projectName, request.Translations);
            }
            
            await translationMemory.SaveChanges();
            return Ok(new AddTranslationResult());
        }
    
        [SwaggerOperation("Query")]
        [HttpGet("translations")]
        public async Task<ActionResult<QueryResult>> Query(
            [FromQuery] string? projectName,
            [FromQuery] string? correlationId,
            [FromQuery] string query,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.Query(projectName, correlationId, query);
            
            return Ok(result);
        }
    }
}