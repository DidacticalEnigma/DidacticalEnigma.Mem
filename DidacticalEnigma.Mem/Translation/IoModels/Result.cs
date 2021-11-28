using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace DidacticalEnigma.Mem.Translation.IoModels
{
    public class Result<T, E>
        where T : notnull
        where E : notnull
    {
        private readonly T value;

        [MaybeNull]
        public T Value => value;

        [MaybeNull]
        public Error<E> Error { get; }

        private Result([AllowNull] T value, [AllowNull] Error<E> error)
        {
            this.value = value;
            Error = error;
        }
        
        public static Result<T, E> Ok(T value)
        {
            return new Result<T, E>(value, null);
        }

        public static Result<T, E> Failure(HttpStatusCode code, string message)
        {
            return new Result<T, E>(default, new Error<E>(
                code, message, default));
        }
        
        public static Result<T, E> Failure(HttpStatusCode code, string message, E extra)
        {
            return new Result<T, E>(default, new Error<E>(
                code, message, extra));
        }
    }

    public class Error<E> where E : notnull
    {
        public HttpStatusCode Code { get; }
        
        public string Message { get; }
        
        public E? Extra { get; }

        public Error(HttpStatusCode code, string message, E? extra)
        {
            Extra = extra;
            Code = code;
            Message = message;
        }
    }

    public struct Unit
    {
        public static Unit Value => default(Unit);
    }
}