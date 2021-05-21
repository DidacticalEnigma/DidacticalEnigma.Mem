using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class Result<T> where T : notnull
    {
        private readonly T value;

        [MaybeNull]
        public T Value => value;

        [MaybeNull]
        public Error Error { get; }

        private Result([AllowNull] T value, [AllowNull] Error error)
        {
            this.value = value;
            Error = error;
        }
        
        public static Result<T> Ok(T value)
        {
            return new Result<T>(value, null);
        }

        public static Result<T> Failure(HttpStatusCode code, string message)
        {
            return new Result<T>(default, new Error(
                code, message));
        }
    }

    public class Error
    {
        public HttpStatusCode Code { get; }
        
        public string Message { get; }

        public Error(HttpStatusCode code, string message)
        {
            Code = code;
            Message = message;
        }
    }

    public struct Unit
    {
        public static Unit Value => default(Unit);
    }
}