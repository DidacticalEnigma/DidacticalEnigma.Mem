using DidacticalEnigma.Mem.Translation.IoModels;
using Microsoft.AspNetCore.Mvc;
using FileResult = DidacticalEnigma.Mem.Translation.IoModels.FileResult;

namespace DidacticalEnigma.Mem.Mappings
{
    public static class ResultMapper
    {
        public static ActionResult<T> Unwrap<T, E>(this Result<T, E> result) where T : notnull
        {
            if (result.Error == null)
            {
                return new OkObjectResult(result.Value);
            }
            else
            {
                return new ObjectResult(result.Error)
                {
                    StatusCode = (int)result.Error.Code
                };
            }
        }

        public static ActionResult<T> Unwrap<T, E>(this Result<Unit, E> result, T value)
        {
            if (result.Error == null)
            {
                return new OkObjectResult(result.Value);
            }
            else
            {
                return new ObjectResult(result.Error)
                {
                    StatusCode = (int)result.Error.Code
                };
            }
        }

        public static ActionResult UnwrapFile<E>(this Result<FileResult, E> result)
        {
            if (result.Error == null)
            {
                var value = result.Value!;
                return new FileStreamResult(value.Content, value.MediaType) { FileDownloadName = value.FileName };
            }
            else
            {
                return new ObjectResult(result.Error)
                {
                    StatusCode = (int)result.Error.Code
                };
            }
        }
    }
}