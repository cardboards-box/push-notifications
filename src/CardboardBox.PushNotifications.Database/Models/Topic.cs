namespace CardboardBox.PushNotifications.Database.Models;

/// <summary>
/// Represents the `noti_topics` table.
/// Contains all of the topics that are available to subscribe to.
/// Topics are automatically added to this table if they don't exist when they are requested.
/// </summary>
[Table("noti_topics")]
public class Topic : DbObject
{
    /// <summary>
    /// This is the actual topic text that gets sent to the notification provider
    /// Different applications can share the same topic hash.
    /// </summary>
    [JsonPropertyName("topicHash")]
    [Column("topic_hash", Unique = true)]
    public string TopicHash { get; set; } = string.Empty;

    /// <summary>
    /// The foreign key that references which application this topic belongs to.
    /// </summary>
    [JsonPropertyName("applicationId")]
    [Column("application_id", Unique = true)]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Represents the `noti_topics` table.
    /// Contains all of the topics that are available to subscribe to.
    /// Topics are automatically added to this table if they don't exist when they are requested.
    /// </summary>
    public Topic() { }

    /// <summary>
    /// Represents the `noti_topics` table.
    /// Contains all of the topics that are available to subscribe to.
    /// Topics are automatically added to this table if they don't exist when they are requested.
    /// </summary>
    /// <param name="appId">The foreign key that references which application this topic belongs to.</param>
    /// <param name="hash">This is the actual topic text that gets sent to the notification provider</param>
    public Topic(Guid appId, string hash)
    {
        ApplicationId = appId;
        TopicHash = hash;
    }
}
