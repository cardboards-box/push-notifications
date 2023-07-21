namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// The response from a topic management request
/// </summary>
/// <param name="Topic">The topic that was managed</param>
/// <param name="IsSubscribe">Whether or not it was a subscription (true) or an unsubscription (false) request</param>
/// <param name="Errors">Any errors that occurred during the request</param>
/// <param name="GlobalError">The global error that occurred (or null if none occurred)</param>
public record class TopicSubResponse(
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("isSubscribe")] bool IsSubscribe,
    [property: JsonPropertyName("errors")] Dictionary<string, string> Errors,
    [property: JsonPropertyName("globalError")] string? GlobalError = null);
