using System.Net;
using Healthcare.Contracts.Common;

namespace Healthcare.Application.Exceptions;

public class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string message, IReadOnlyCollection<ErrorDetail>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyCollection<ErrorDetail>? Errors { get; }
}
