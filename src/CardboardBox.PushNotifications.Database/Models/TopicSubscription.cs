namespace CardboardBox.PushNotifications.Database.Models;

/// <summary>
/// Represents the `noti_topic_subscriptions` table.
/// Contains a map between topics and users. 
/// </summary>
[Table("noti_topic_subscriptions")]
public class TopicSubscription : DbObject
{
    /// <summary>
    /// The unique identifier of the user who owns this subscription
    /// </summary>
    [JsonPropertyName("profileId")]
    [Column("profile_id", Unique = true)]
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The foreign key that references the topic that the user is subscribed to
    /// </summary>
    [JsonPropertyName("topicId")]
    [Column("topic_id", Unique = true)]
    public Guid TopicId { get; set; }

    /// <summary>
    /// The foriegn key that references the group the topic was created because of (if it wasn't created because of a group, this will be null)
    /// </summary>
    [JsonPropertyName("groupId")]
    [Column("group_id")]
    public Guid? GroupId { get; set; }

    /// <summary>
    /// Represents the `noti_topic_subscriptions` table.
    /// Contains a map between topics and users. 
    /// </summary>
    public TopicSubscription() { }

    /// <summary>
    /// Represents the `noti_topic_subscriptions` table.
    /// Contains a map between topics and users. 
    /// </summary>
    /// <param name="profileId">The unique identifier of the user who owns this subscription</param>
    /// <param name="topicId">The foreign key that references the topic that the user is subscribed to</param>
    /// <param name="groupId">The foriegn key that references the group the topic was created because of</param>
    public TopicSubscription(string profileId, Guid topicId, Guid? groupId = null)
    {
        ProfileId = profileId;
        TopicId = topicId;
        GroupId = groupId;
    }
}
