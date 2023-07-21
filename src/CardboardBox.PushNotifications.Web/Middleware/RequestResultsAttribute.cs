namespace CardboardBox.PushNotifications.Web;

/// <summary>
/// Wraps the <see cref="ProducesResponseTypeAttribute"/> for the <see cref="RequestResults"/> types
/// </summary>
public class RequestResultsAttribute : ProducesResponseTypeAttribute
{
    /// <summary></summary>
    public RequestResultsAttribute() : this(200) { }

    /// <summary></summary>
    /// <param name="statusCode"></param>
    public RequestResultsAttribute(int statusCode) : base(typeof(RequestResult), statusCode) { }
}

/// <summary>
/// Wraps the <see cref="ProducesResponseTypeAttribute"/> for the <see cref="RequestResult{T}"/> types
/// </summary>
/// <typeparam name="T"></typeparam>
public class RequestResultsAttribute<T> : ProducesResponseTypeAttribute
{
    /// <summary></summary>
    public RequestResultsAttribute() : this(200) { }

    /// <summary></summary>
    /// <param name="statusCode"></param>
    public RequestResultsAttribute(int statusCode) : base(typeof(RequestResult<T>), statusCode) { }
}

/// <summary>
/// Wraps the <see cref="ProducesResponseTypeAttribute"/> for the <see cref="ExceptionResult"/> types
/// </summary>
public class ExceptionResultsAttribute : ProducesResponseTypeAttribute
{
    /// <summary></summary>
    /// <param name="code"></param>
    public ExceptionResultsAttribute(int code) : base(typeof(ExceptionResult), code) { }

    /// <summary></summary>
    public ExceptionResultsAttribute() : base(typeof(ExceptionResult), 500) { }
}
