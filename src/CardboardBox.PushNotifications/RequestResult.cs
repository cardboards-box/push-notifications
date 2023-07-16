namespace CardboardBox.PushNotifications;

/// <summary>
/// Represents the result of a request
/// </summary>
public class RequestResult
{
    /// <summary>
    /// Whether or not the request was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success => (int)Code >= 200 && (int)Code < 300;

    /// <summary>
    /// The status code of the request
    /// </summary>
    [JsonPropertyName("code")]
    public HttpStatusCode Code { get; set; }

    /// <summary>
    /// Any error or message associated with the result
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>Represents the result of a request</summary>
    public RequestResult() { }

    /// <summary>Represents the result of a request</summary>
    /// <param name="code">The status code of the request</param>
    /// <param name="message">Any error or message associated with the result</param>
    public RequestResult(HttpStatusCode code, string? message = null)
    {
        Code = code;
        Message = message;
    }
}

/// <summary>
/// Represents the result of a request that returns data
/// </summary>
/// <typeparam name="T">The type of data</typeparam>
public class RequestResult<T> : RequestResult
{
    /// <summary>
    /// The data result of the request
    /// </summary>
    public T Data { get; set; }

    /// <summary>Represents the result of a request</summary>
    /// <param name="data">The data result of the request</param>
    public RequestResult(T data) : this(HttpStatusCode.OK, data) { }

    /// <summary>Represents the result of a request</summary>
    /// <param name="code">The status code of the request</param>
    /// <param name="data">The data result of the request</param>
    /// <param name="message">Any error or message associated with the result</param>
    public RequestResult(HttpStatusCode code, T data, string? message = null) : base(code, message)
    {
        Data = data;
    }
}

/// <summary>
/// Represents a bad result
/// </summary>
public class ExceptionResult : RequestResult<string[]>
{
    public ExceptionResult(HttpStatusCode code) : base(code, Array.Empty<string>()) { }

    public ExceptionResult(HttpStatusCode code, params string[] data) : base(code, data, "An error occurred while processing your request") { }

    public ExceptionResult(HttpStatusCode code, string[] data, string? message = null) : base(code, data, message) { }
}

public static class RequestResults
{
    public static RequestResult WasOk() => new(HttpStatusCode.OK);

    public static RequestResult<T> WasOk<T>(T data) => new(HttpStatusCode.OK, data);

    public static RequestResult WasNoContent(string? message = null) => new(HttpStatusCode.NoContent, message);

    public static ExceptionResult WasException(string? message = null) => 
        new(HttpStatusCode.InternalServerError, message ?? "An error occurred while processing your request");

    public static ExceptionResult WasException(string[] issues) => new(HttpStatusCode.InternalServerError, issues, "An error occurred while processing your request");

    public static ExceptionResult WasNotFound(string resource) => new(HttpStatusCode.NotFound, $"That {resource} could not be found.");

    public static ExceptionResult WasUnauthorized() => new(HttpStatusCode.Unauthorized, "You are not authorized to perform this action.");

    public static ExceptionResult WasBadRequest(RequestValidator validator) => new(HttpStatusCode.BadRequest, validator.Issues, "There were some issues with your request");

    public static ExceptionResult WasBadRequest(params string[] issues) => new(HttpStatusCode.BadRequest, issues, "There were some issues with your request");
}