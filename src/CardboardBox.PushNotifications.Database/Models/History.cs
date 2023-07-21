namespace CardboardBox.PushNotifications.Database.Models;

/// <summary>
/// Represents the `noti_history` table.
/// Contains a history of all notifications sent in the context of either a specific user or a topic.
/// If the target was a specific user, <see cref="ProfileId"/> will be set and <see cref="TopicId"/> will be null.
/// If the target was a topic, <see cref="TopicId"/> will be set and <see cref="ProfileId"/> will be null.
/// </summary>
[Table("noti_history")]
public class History : DbObject
{
    /// <summary>
    /// The application that sent the notification
    /// </summary>
    [JsonPropertyName("applicationId")]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// The data field sent on the notification
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>
    /// The optional image url sent on the notification
    /// </summary>
    [JsonPropertyName("imageUrl")]
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// The title sent on the notification
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The body sent on the notification
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// The result codes returned from the push notification service
    /// </summary>
    [JsonPropertyName("results")]
    public string[] Results { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The foreign key that references the topic that was used.
    /// </summary>
    [JsonPropertyName("topicId")]
    [Column("topic_id")]
    public Guid? TopicId { get; set; }

    /// <summary>
    /// The unique identifier of the user this notification was intended for.
    /// </summary>
    [JsonPropertyName("profileId")]
    [Column("profile_id")]
    public string? ProfileId { get; set; }
}