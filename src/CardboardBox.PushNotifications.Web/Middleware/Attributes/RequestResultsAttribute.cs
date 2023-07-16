namespace CardboardBox.PushNotifications.Web;

public class RequestResultsAttribute : ProducesResponseTypeAttribute
{
    public RequestResultsAttribute() : this(200) { }

    public RequestResultsAttribute(int statusCode) : base(typeof(RequestResult), statusCode) { }
}

public class RequestResultsAttribute<T> : ProducesResponseTypeAttribute
{
    public RequestResultsAttribute() : this(200) { }

    public RequestResultsAttribute(int statusCode) : base(typeof(RequestResult<T>), statusCode) { }
}

public class ExceptionResultsAttribute : ProducesResponseTypeAttribute
{
    public ExceptionResultsAttribute(int code) : base(typeof(ExceptionResult), code) { }

    public ExceptionResultsAttribute() : base(typeof(ExceptionResult), 500) { }
}
