namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// Represents a response message for a specific notification response
/// </summary>
public class NotificationResponseMessage
{
    /// <summary>
    /// The combination of the data from the <see cref="Message"/> or <see cref="MulticastMessage"/>
    /// </summary>
    [JsonPropertyName("identity")]
    public string Identity { get; set; }

    /// <summary>
    /// The response message from the server (null if an error occurred)
    /// </summary>
    [JsonPropertyName("response")]
    public string? Response { get; set; }

    /// <summary>
    /// The error message from the server (null if not an error occurred)
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Represents a response message for a specific notification response
    /// </summary>
    /// <param name="identity">The combination of the data from the <see cref="Message"/> or <see cref="MulticastMessage"/></param>
    /// <param name="response">The response message from the server (null if an error occurred)</param>
    /// <param name="error">The error message from the server (null if not an error occurred)</param>
    public NotificationResponseMessage(string identity, string? response = null, string? error = null)
    {
        Identity = identity;
        Response = response;
        Error = error;
    }

    /// <summary>
    /// Returns a string representation of the response
    /// </summary>
    /// <returns>The string representation of the response for audit</returns>
    public override string ToString()
    {
        return string.IsNullOrEmpty(Response) 
            ? $"Notification Response Failure [{Identity}]: {Error}" 
            : $"Notification Response Success [{Identity}]: {Response}";
    }

    /// <summary>
    /// Gets the identifying information from a <see cref="Message"/> for audit purposes
    /// </summary>
    /// <param name="message">The message that was sent</param>
    /// <returns>The identity of the message</returns>
    public static string GetIdentity(Message message)
    {
        if (!string.IsNullOrEmpty(message.Token)) return $"Notification Device Target: {message.Token}";
        if (!string.IsNullOrEmpty(message.Topic)) return $"Notification Topic Target: {message.Topic}";
        if (!string.IsNullOrEmpty(message.Condition)) return $"Notification Condition Target: {message.Condition}";

        return "Notification Target: Unknown";
    }

    /// <summary>
    /// Gets the message response for an error
    /// </summary>
    /// <param name="message">The message that threw the error</param>
    /// <param name="ex">The exception that was thrown</param>
    /// <param name="logger">The logger to log the message to</param>
    /// <returns>The response message for audit</returns>
    public static NotificationResponseMessage From(Message message, Exception ex, ILogger logger)
    {
        var identity = GetIdentity(message);
        logger?.LogError(ex, "Error occurred while sending message: {identity}", identity);
        return new(identity, null, ex.Message);
    }

    /// <summary>
    /// Gets the message response for a successful request
    /// </summary>
    /// <param name="message">The message that was sent</param>
    /// <param name="responseId">The returned response ID</param>
    /// <returns>The response message for audit</returns>
    public static NotificationResponseMessage From(Message message, string responseId)
    {
        var identity = GetIdentity(message);
        return new(identity, responseId, null);
    }

    /// <summary>
    /// Gets the message response from a successful multicast request
    /// </summary>
    /// <param name="token">The device the multicast was sent to</param>
    /// <param name="responseId">The returned response ID</param>
    /// <returns>The response message for audit</returns>
    public static NotificationResponseMessage From(string token, string responseId)
    {
        var identity = $"Notification Multicast Target: {token}";
        return new(identity, responseId);
    }

    /// <summary>
    /// Gets the message response for a multicast error
    /// </summary>
    /// <param name="token">The device the multicast was sent to</param>
    /// <param name="ex">The exception that was thrown</param>
    /// <param name="logger">The logger to log the message to</param>
    /// <returns>The response message for audit</returns>
    public static NotificationResponseMessage From(string token, Exception ex, ILogger logger)
    {
        var identity = $"Notification Multicast Target: {token}";
        logger?.LogError(ex, "Error occurred while sending message: {identity}", identity);
        return new(identity, null, ex.Message);
    }
}
