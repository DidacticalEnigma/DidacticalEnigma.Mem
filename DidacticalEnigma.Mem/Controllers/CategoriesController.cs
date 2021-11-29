using System;
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
    public class CategoriesController : ControllerBase
    {
        [SwaggerOperation(OperationId = "AddCategories")]
        [HttpPost("categories")]
        [Authorize("ModifyCategories")]
        public async Task<ActionResult<AddCategoriesResult>> AddCategories(
            [FromQuery] string projectName,
            [FromBody] AddCategoriesParams request,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.AddCategories(
                projectName,
                request);
            return result.Unwrap(new AddCategoriesResult());
        }
        
        [SwaggerOperation(OperationId = "DeleteCategory")]
        [HttpDelete("categories")]
        [Authorize("ModifyCategories")]
        public async Task<ActionResult<DeleteCategoryResult>> AddCategories(
            [FromQuery] string projectName,
            [FromQuery] Guid categoryId,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.DeleteCategory(
                categoryId);
            return result.Unwrap(new DeleteCategoryResult());
        }
        
        [SwaggerOperation(OperationId = "GetCategories")]
        [HttpGet("categories")]
        [Authorize("ReadTranslations")]
        public async Task<ActionResult<QueryCategoriesResult>> GetCategories(
            [FromQuery] string projectName,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.QueryCategories(
                projectName);
            return result.Unwrap();
        }
    }
}