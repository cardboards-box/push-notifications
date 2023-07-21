namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// The responses from the topic management requests
/// </summary>
/// <param name="Tokens">The devices that were managed</param>
/// <param name="IsSubscribe">Whether or not it was a subscription (true) or an unsubscription (false) request</param>
/// <param name="Responses">The responses from the token management requests</param>
/// <param name="Failures">The number of failures that occurred during the requests</param>
public record class TopicSubResponses(
    [property: JsonPropertyName("tokens")] string[] Tokens,
    [property: JsonPropertyName("isSubscribe")] bool IsSubscribe,
    [property: JsonPropertyName("responses")] TopicSubResponse[] Responses,
    [property: JsonPropertyName("failures")] int Failures);
