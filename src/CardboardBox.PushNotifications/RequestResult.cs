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
    /// <summary>
    /// Represents a bad result
    /// </summary>
    /// <param name="code">The status code of the request</param>
    /// <param name="data">The data result of the request</param>
    /// <param name="message">Any error or message associated with the result</param>
    public ExceptionResult(
        HttpStatusCode code, 
        string[]? data = null, 
        string? message = null) : base(
            code, 
            data ?? Array.Empty<string>(), 
            message ?? RequestResults.EXCEPTION) { }
}

/// <summary>
/// A helper class for handling results and corresponding codes
/// </summary>
public static class RequestResults
{
    #region Generic response codes
    /// <summary>
    /// Request was fine and has data associated with it
    /// </summary>
    public const string OK = "ok";
    /// <summary>
    /// Request was fine but there was no content or change that happened
    /// </summary>
    public const string NO_CONTENT = "ok-no-content";
    /// <summary>
    /// Request resulted in an exception being thrown
    /// </summary>
    public const string EXCEPTION = "unknown-error-occurred";
    /// <summary>
    /// Request was for a resource that does not exist
    /// </summary>
    public const string NOT_FOUND = "resource-not-found";
    /// <summary>
    /// Application that made the request is not authorized to do so
    /// </summary>
    public const string UNAUTHORIZED = "application-not-authorized";
    /// <summary>
    /// The request contained invalid data
    /// </summary>
    public const string BAD_REQUEST = "user-input-invalid";
    #endregion

    #region No-content response codes
    /// <summary>
    /// The requested user has no devices subscribed so there was no notification sent but the request was fine
    /// Specific for <see cref="Notifications.NotificationService.Direct(Guid, string, NotificationData)"/>
    /// </summary>
    public const string USER_NO_DEVICES = "user-has-no-devices";
    /// <summary>
    /// The requested topic was not found in associated with this application so there was no notification sent but the request was fine
    /// Specific for <see cref="Notifications.NotificationService.Topic(Guid, string, NotificationData)"/>
    /// </summary>
    public const string TOPIC_NOT_FOUND = "topic-not-found";
    /// <summary>
    /// The user was not subscribed to the requested topic so there was no need to unsubscribe them but the request was fine
    /// </summary>
    public const string USER_NOT_SUBSCRIBED = "user-not-subed";
    /// <summary>
    /// The requested group has no topics subscribed so there was no need to subscribe to them
    /// </summary>
    public const string GROUP_NO_TOPICS = "group-has-no-topics";
    /// <summary>
    /// The user was not subscribed to the requested group so there was no need to unsubscribe them but the request was fine
    /// </summary>
    public const string GROUP_NOT_SUBSCRIBED = "group-not-subscribed";
    /// <summary>
    /// The requested user has no topics so there was nothing to subscribe them to
    /// </summary>
    public const string USER_NO_TOPICS = "user-has-no-topics";
    #endregion

    #region Bad request response codes
    /// <summary>
    /// The given topic name did not match the <see cref="TopicHelper.TopicRegex"/> regex.
    /// </summary>
    public const string TOPIC_NAME_INVALID = "topic-name-invalid";
    #endregion

    /// <summary>
    /// Result was successful and changes were made
    /// </summary>
    /// <returns></returns>
    public static RequestResult WasOk() => new(HttpStatusCode.OK, NO_CONTENT);

    /// <summary>
    /// Request was successful, changes were made, and there is data associated with the request
    /// </summary>
    /// <typeparam name="T">The type of data returned</typeparam>
    /// <param name="data">The data returned by the request</param>
    /// <returns></returns>
    public static RequestResult<T> WasOk<T>(T data) => new(HttpStatusCode.OK, data, OK);

    /// <summary>
    /// Request was successful, but no changes were made, and there was no content returned
    /// </summary>
    /// <param name="message">Any associated message or defaults to <see cref="NO_CONTENT"/></param>
    /// <returns></returns>
    public static RequestResult WasNoContent(string? message = null) => new(HttpStatusCode.NoContent, message ?? NO_CONTENT);

    /// <summary>
    /// Request resulted in an error and there are issues associated with it
    /// </summary>
    /// <param name="issues">The issues that occurred with the request</param>
    /// <returns></returns>
    public static ExceptionResult WasException(params string[] issues) => new(HttpStatusCode.InternalServerError, issues);

    /// <summary>
    /// The requested resource was not found
    /// </summary>
    /// <param name="resources">The resource(s) that were requested</param>
    /// <returns></returns>
    public static ExceptionResult WasNotFound(params string[] resources) => new(HttpStatusCode.NotFound, resources, NOT_FOUND);

    /// <summary>
    /// The application was not authorized to make the request
    /// </summary>
    /// <param name="issues">The issues associated with the request</param>
    /// <returns></returns>
    public static ExceptionResult WasUnauthorized(params string[] issues) => new(HttpStatusCode.Unauthorized, issues, UNAUTHORIZED);

    /// <summary>
    /// The request contained invalid data
    /// </summary>
    /// <param name="validator">The validator that found the issues</param>
    /// <returns></returns>
    public static ExceptionResult WasBadRequest(RequestValidator validator) => WasBadRequest(validator.Issues);

    /// <summary>
    /// The request contained invalid data
    /// </summary>
    /// <param name="issues">The issues that were found with the data</param>
    /// <returns></returns>
    public static ExceptionResult WasBadRequest(params string[] issues) => new(HttpStatusCode.BadRequest, issues, BAD_REQUEST);
}