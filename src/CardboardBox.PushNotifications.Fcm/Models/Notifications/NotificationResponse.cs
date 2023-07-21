namespace CardboardBox.PushNotifications.Fcm;

using Responses = IReadOnlyCollection<SendResponse>;

/// <summary>
/// Represents the results of a notification sent to FCM
/// </summary>
public class NotificationResponse
{
    /// <summary>
    /// The type of notification
    /// </summary>
    [JsonPropertyName("type")]
    public NotificationType Type { get; set; }

    /// <summary>
    /// The number of failures in the response
    /// </summary>
    [JsonPropertyName("failures")]
    public int Failures { get; set; }

    /// <summary>
    /// The message results from FCM
    /// </summary>
    [JsonPropertyName("responses")]
    public NotificationResponseMessage[] Responses { get; set; }

    /// <summary>
    /// The results of all of the <see cref="Responses"/>
    /// </summary>
    [JsonIgnore]
    public string[] Results => Responses.Select(t => t.ToString()).ToArray();

    /// <summary>
    /// Represents the results of a notification sent to FCM
    /// </summary>
    /// <param name="type">The type of notification</param>
    /// <param name="failures">The number of failures in the response</param>
    /// <param name="responses">The message results from FCM</param>
    public NotificationResponse(NotificationType type, int failures, params NotificationResponseMessage[] responses)
    {
        Type = type;
        Failures = failures;
        Responses = responses;
    }

    /// <summary>
    /// Gets the message response for a single message error
    /// </summary>
    /// <param name="message">The message that caused the error</param>
    /// <param name="ex">The error that was thrown</param>
    /// <param name="logger">The logger to log the error to</param>
    /// <returns>The message response</returns>
    public static NotificationResponse From(Message message, Exception ex, ILogger logger)
    {
        var response = NotificationResponseMessage.From(message, ex, logger);
        return new(NotificationType.Single, 1, response);
    }

    /// <summary>
    /// Gets the message response for a successful message
    /// </summary>
    /// <param name="message">The message that was sent</param>
    /// <param name="responseId">The ID of the response from FCM</param>
    /// <returns>The message response</returns>
    public static NotificationResponse From(Message message, string responseId)
    {
        var response = NotificationResponseMessage.From(message, responseId);
        return new(NotificationType.Single, 0, response);
    }

    /// <summary>
    /// Gets the message response for a batch message
    /// </summary>
    /// <param name="messages">The messages that were sent in this batch</param>
    /// <param name="responses">The responses that were received for this batch</param>
    /// <param name="logger">The logger to log errors to</param>
    /// <returns>The message response</returns>
    public static NotificationResponse From(Message[] messages, Responses responses, ILogger logger)
    {
        int errors = 0;
        var resps = responses
           .Select((t, i) =>
           {
               var message = messages[i];
               if (t.IsSuccess) return NotificationResponseMessage.From(message, t.MessageId);
               errors++;
               return NotificationResponseMessage.From(message, t.Exception, logger);
           })
           .ToArray();
        return new(NotificationType.Batch, errors, resps);
    }

    /// <summary>
    /// Gets the message response for a multicast message
    /// </summary>
    /// <param name="message">The message that was sent</param>
    /// <param name="responses">The response messages that were received</param>
    /// <param name="logger">The logger to log errors to</param>
    /// <returns>The message response</returns>
    public static NotificationResponse From(MulticastMessage message, Responses responses, ILogger logger)
    {
        int errors = 0;
        var resps = responses
           .Select((t, i) =>
           {
               var token = message.Tokens[i];
               if (t.IsSuccess) return NotificationResponseMessage.From(token, t.MessageId);
               errors++;
               return NotificationResponseMessage.From(token, t.Exception, logger);
           })
           .ToArray();
        return new(NotificationType.Multicast, errors, resps);
    }

    /// <summary>
    /// Gets the message response for a multicast message error
    /// </summary>
    /// <param name="message">The message that threw the error</param>
    /// <param name="ex">The error that was thrown</param>
    /// <param name="logger">The logger to log the errors to</param>
    /// <returns>The message response</returns>
    public static NotificationResponse From(MulticastMessage message, Exception ex, ILogger logger)
    {
        var response = NotificationResponseMessage.From($"Global FCM Multicast Message Tokens: {string.Join(", ", message.Tokens)}", ex, logger);
        return new(NotificationType.Multicast, 1, response);
    }

    /// <summary>
    /// Gets the message response for an error that occurred while sending a batch message
    /// </summary>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="logger">The logger to log the errors to</param>
    /// <returns>The message response</returns>
    public static NotificationResponse From(Exception ex, ILogger logger)
    {
        logger?.LogError(ex, "Error occurred while sending a batch message");
        return new(NotificationType.Batch, 1, new NotificationResponseMessage("global", null, ex.Message));
    }
}