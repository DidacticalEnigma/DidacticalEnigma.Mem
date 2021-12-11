using System;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Extensions;
using DidacticalEnigma.Mem.Mappings;
using DidacticalEnigma.Mem.Translation;
using DidacticalEnigma.Mem.Translation.Categories;
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
    public class CategoriesController : ControllerBase
    {
        [SwaggerOperation(OperationId = "AddCategories")]
        [HttpPost("categories")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<AddCategoriesResult>> AddCategories(
            [FromQuery] string projectName,
            [FromBody] AddCategoriesParams request,
            [FromServices] AddCategoriesHandler addCategoriesHandler)
        {
            var result = await addCategoriesHandler.Add(
                Request.GetUserName(),
                projectName,
                request);
            return result.Unwrap(new AddCategoriesResult());
        }
        
        [SwaggerOperation(OperationId = "DeleteCategory")]
        [HttpDelete("categories")]
        [Authorize("ApiRejectAnonymous")]
        public async Task<ActionResult<DeleteCategoryResult>> DeleteCategory(
            [FromQuery] string projectName,
            [FromQuery] Guid categoryId,
            [FromServices] DeleteCategoryHandler deleteCategoryHandler)
        {
            var result = await deleteCategoryHandler.Delete(
                Request.GetUserName(),
                projectName,
                categoryId);
            return result.Unwrap(new DeleteCategoryResult());
        }
        
        [SwaggerOperation(OperationId = "GetCategories")]
        [HttpGet("categories")]
        [Authorize("ApiAllowAnonymous")]
        public async Task<ActionResult<QueryCategoriesResult>> GetCategories(
            [FromQuery] string projectName,
            [FromServices] QueryCategoriesHandler queryCategoriesHandler)
        {
            var result = await queryCategoriesHandler.Query(
                Request.GetUserName(),
                projectName);
            return result.Unwrap();
        }
    }
}