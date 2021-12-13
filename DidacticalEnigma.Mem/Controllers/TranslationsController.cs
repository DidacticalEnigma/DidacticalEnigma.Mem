using System.Threading.Tasks;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Mappings;
using DidacticalEnigma.Mem.Models;
using DidacticalEnigma.Mem.Translation;
using DidacticalEnigma.Mem.Translation.IoModels;
using DidacticalEnigma.Mem.Translation.Translations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DidacticalEnigma.Mem.Controllers
{
    [ApiController]
    [Route("mem")]
    public class TranslationsController : ControllerBase
    {
        [SwaggerOperation(OperationId = "AddTranslations")]
        [HttpPost("translations")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<AddTranslationsResult>> AddTranslations(
            [FromQuery] string projectName,
            [FromBody] AddTranslationsParams request,
            [FromServices] AddTranslationsHandler addTranslationsHandler)
        {
            var result = await addTranslationsHandler.Add(
                Request.GetUserName(),
                projectName,
                request.Translations,
                request.AllowPartialAdd);
            return result.Unwrap();
        }

        /// <summary>
        /// Query for translations
        /// </summary>
        /// <param name="queryTranslationsHandler">Internal</param>
        /// <param name="projectName">The name of the project</param>
        /// <param name="correlationId">The prefix of the correlation id</param>
        /// <param name="query">Search query</param>
        /// <param name="category">Translation category</param>
        /// <param name="paginationToken">A pagination token that was returned from the previous query with the same set of parameters</param>
        /// <param name="limit">How many translations should be returned? Values above 250 are treated as if 250 was passed.</param>
        /// <param name="translatedOnly">Return only the sentences which have corresponding translations</param>
        [SwaggerOperation(OperationId = "Query")]
        [HttpGet("translations")]
        [Authorize("ApiAllowAnonymous")]
        public async Task<ActionResult<QueryTranslationsResult>> Query(
            [FromServices] QueryTranslationsHandler queryTranslationsHandler,
            [FromQuery] string? projectName,
            [FromQuery] string? correlationId,
            [FromQuery] string? query,
            [FromQuery] string? category,
            [FromQuery] string? paginationToken = null,
            [FromQuery] int? limit = 50,
            [FromQuery] bool? translatedOnly = false)
        {
            var result = await queryTranslationsHandler.Query(
                Request.GetUserName(),
                projectName,
                correlationId,
                query,
                category,
                paginationToken,
                limit ?? 50,
                translatedOnly ?? false);
            return result.Unwrap();
        }

        [SwaggerOperation(OperationId = "DeleteTranslation")]
        [HttpDelete("translations")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<DeleteTranslationResult>> DeleteTranslation(
            [FromQuery] string projectName,
            [FromQuery] string correlationId,
            [FromServices] DeleteTranslationHandler deleteTranslationHandler)
        {
            var result = await deleteTranslationHandler.Delete(
                Request.GetUserName(),
                projectName,
                correlationId);
            return result.Unwrap(new DeleteTranslationResult());
        }
        
        [SwaggerOperation(OperationId = "UpdateTranslation")]
        [HttpPatch("translations")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<UpdateTranslationResult>> UpdateTranslation(
            [FromQuery] string projectName,
            [FromQuery] string correlationId,
            [FromBody] UpdateTranslationParams request,
            [FromServices] UpdateTranslationHandler updateTranslationHandler)
        {
            var result = await updateTranslationHandler.Update(
                Request.GetUserName(),
                projectName,
                correlationId,
                request);
            return result.Unwrap(new UpdateTranslationResult());
        }
    }
}