using System;
using System.Threading.Tasks;
using DidacticalEnigma.Mem.Translation;
using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FileResult = DidacticalEnigma.Mem.Translation.IoModels.FileResult;

namespace DidacticalEnigma.Mem.Controllers
{
    [ApiController]
    [Route("mem")]
    public class MemController : ControllerBase
    {
        [SwaggerOperation(OperationId = "AddProject")]
        [HttpPost("projects")]
        [Authorize("ModifyProjects")]
        public async Task<ActionResult<AddProjectResult>> AddProject(
            [FromQuery] string projectName,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.AddProject(projectName);
            return Unwrap(result, new AddProjectResult());
        }
        
        [SwaggerOperation(OperationId = "AddTranslations")]
        [HttpPost("translations")]
        [Authorize("ModifyTranslations")]
        public async Task<ActionResult<AddTranslationsResult>> AddTranslations(
            [FromQuery] string projectName,
            [FromBody] AddTranslationsParams request,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.AddTranslations(projectName, request.Translations, request.AllowPartialAdd);
            return Unwrap(result);
        }

        [SwaggerOperation(OperationId = "AddContext")]
        [HttpPost("contexts")]
        [Authorize("ModifyContexts")]
        public async Task<ActionResult<AddContextResult>> AddContext(
            [FromForm] AddContextParams request,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.AddContext(
                request.Id,
                request.CorrelationId,
                request.ProjectName,
                request.Content.OpenReadStream(),
                request.Content.ContentType,
                request.Text);
            return Unwrap(result, new AddContextResult());
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
            return Unwrap(result);
        }
        
        [SwaggerOperation(OperationId = "GetContexts")]
        [HttpGet("contexts")]
        [Authorize("ReadContexts")]
        public async Task<ActionResult<QueryContextsResult>> GetContext(
            [FromQuery] Guid? id,
            [FromQuery] string? projectId,
            [FromQuery] string? correlationId,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.GetContexts(id, projectId, correlationId);
            return Unwrap(result);
        }

        [SwaggerOperation(OperationId = "GetContextData")]
        [HttpGet("contexts/data")]
        [Authorize("ReadContexts")]
        public async Task<ActionResult> GetContextData(
            [FromQuery] Guid id,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.GetContextData(id);
            return UnwrapFile(result);
        }

        [SwaggerOperation(OperationId = "DeleteContext")]
        [HttpDelete("contexts")]
        [Authorize("ModifyContexts")]
        public async Task<ActionResult<DeleteContextResult>> DeleteContext(
            [FromQuery] Guid id,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.DeleteContext(id);
            return Unwrap(result, new DeleteContextResult());
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
            return Unwrap(result, new DeleteTranslationResult());
        }
        
        [SwaggerOperation(OperationId = "DeleteProject")]
        [HttpDelete("projects")]
        [Authorize("ModifyProjects")]
        public async Task<ActionResult<DeleteProjectResult>> DeleteProject(
            [FromQuery] string projectName,
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.DeleteProject(projectName);
            return Unwrap(result, new DeleteProjectResult());
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
            return Unwrap(result, new UpdateTranslationResult());
        }
        
        [SwaggerOperation(OperationId = "ListProjects")]
        [HttpGet("projects")]
        [Authorize("EnumerateProjects")]
        public async Task<ActionResult<QueryProjectsResult>> ListProjects(
            [FromServices] ITranslationMemory translationMemory)
        {
            var result = await translationMemory.ListProjects();
            return Unwrap(result);
        }
        
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
            return Unwrap(result, new AddCategoriesResult());
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
            return Unwrap(result, new DeleteCategoryResult());
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
            return Unwrap(result);
        }

        private ActionResult<T> Unwrap<T, E>(Result<T, E> result) where T : notnull
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
        
        private ActionResult<T> Unwrap<T, E>(Result<Unit, E> result, T value)
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
        
        private ActionResult UnwrapFile<E>(Result<FileResult, E> result)
        {
            if (result.Error == null)
            {
                var value = result.Value!;
                return File(value.Content, value.MediaType, value.FileName);
            }
            else
            {
                return this.StatusCode((int)result.Error.Code, result.Error);
            }
        }
    }
}