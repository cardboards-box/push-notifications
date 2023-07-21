namespace CardboardBox.PushNotifications.Database.Models;

/// <summary>
/// Represents the `noti_topic_group_subscription` table.
/// Contains a map between users and groups.
/// This can allow a user to be automatically subscribed to a new topic when it's added to a group they're subscribed to.
/// </summary>
[Table("noti_topic_group_subscription")]
public class TopicGroupSubscription : DbObject
{
    /// <summary>
    /// The unique identifier of the user who owns this subscription
    /// </summary>
    [JsonPropertyName("profileId")]
    [Column("profile_id", Unique = true)]
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the group that the user is subscribed to
    /// </summary>
    [JsonPropertyName("groupId")]
    [Column("group_id", Unique = true)]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Represents the `noti_topic_group_subscription` table.
    /// Contains a map between users and groups.
    /// This can allow a user to be automatically subscribed to a new topic when it's added to a group they're subscribed to.
    /// </summary>
    public TopicGroupSubscription() { }

    /// <summary>
    /// Represents the `noti_topic_group_subscription` table.
    /// Contains a map between users and groups.
    /// This can allow a user to be automatically subscribed to a new topic when it's added to a group they're subscribed to.
    /// </summary>
    /// <param name="profileId">The unique identifier of the user who owns this subscription</param>
    /// <param name="groupId">The unique identifier of the group that the user is subscribed to</param>
    public TopicGroupSubscription(string profileId, Guid groupId)
    {
        ProfileId = profileId;
        GroupId = groupId;
    }
}
