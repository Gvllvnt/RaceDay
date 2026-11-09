using System.Net;

namespace RaceDay.Web.Infrastructure;

public class ApiResult
{
    public bool Success { get; init; }
    public HttpStatusCode StatusCode { get; init; }

    /// <summary>A user-friendly message. Never contains raw exception text.</summary>
    public string? Error { get; init; }

    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; init; }
}
