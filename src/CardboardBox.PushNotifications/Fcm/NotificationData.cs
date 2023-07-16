namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// Represents the contents of a push notification
/// </summary>
/// <param name="Title">The title of the notification</param>
/// <param name="Body">The content of the notitifcation</param>
/// <param name="ImageUrl">The optional image url for the notification</param>
/// <param name="Data">The extra properties of the notification</param>
public record class NotificationData(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl = null,
    [property: JsonPropertyName("data")] Dictionary<string, string>? Data = null);
