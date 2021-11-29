using System.Threading.Tasks;
using DidacticalEnigma.Mem.Mappings;
using DidacticalEnigma.Mem.Translation;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DidacticalEnigma.Mem.Controllers
{
    [ApiController]
    [Route("mem")]
    public class TranslationsController : ControllerBase
    {
        [SwaggerOperation(OperationId = "AddTranslations")]
        [HttpPost("translations")]
        [Authorize("ModifyTranslations")]
        public async Task<ActionResult<AddTranslationsResult>> AddTranslations(
            [FromQuery] string projectName,
            [FromBody] AddTranslationsParams request,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.AddTranslations(projectName, request.Translations, request.AllowPartialAdd);
            return result.Unwrap();
        }
        
        [SwaggerOperation(OperationId = "Query")]
        [HttpGet("translations")]
        [Authorize("ReadTranslations")]
        public async Task<ActionResult<QueryTranslationsResult>> Query(
            [FromQuery] string? projectName,
            [FromQuery] string? correlationId,
            [FromQuery] string? query,
            [FromQuery] string? category,
            [FromQuery] string? paginationToken,
            [FromQuery] int? limit,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.Query(projectName, correlationId, query, category, paginationToken, limit ?? 50);
            return result.Unwrap();
        }
        
        [SwaggerOperation(OperationId = "DeleteTranslation")]
        [HttpDelete("translations")]
        [Authorize("ModifyTranslations")]
        public async Task<ActionResult<DeleteTranslationResult>> DeleteTranslation(
            [FromQuery] string projectName,
            [FromQuery] string correlationId,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.DeleteTranslation(projectName, correlationId);
            return result.Unwrap(new DeleteTranslationResult());
        }
        
        [SwaggerOperation(OperationId = "UpdateTranslation")]
        [HttpPatch("translations")]
        [Authorize("ModifyTranslations")]
        public async Task<ActionResult<UpdateTranslationResult>> UpdateTranslation(
            [FromQuery] string projectName,
            [FromQuery] string correlationId,
            [FromBody] UpdateTranslationParams request,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.UpdateTranslation(
                projectName,
                correlationId,
                request);
            return result.Unwrap(new UpdateTranslationResult());
        }
    }
}