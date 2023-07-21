namespace CardboardBox.PushNotifications.Database.Models;

/// <summary>
/// Represents the `noti_topic_group_map` table.
/// The map between topics and the groups they're in.
/// </summary>
[Table("noti_topic_group_map")]
public class TopicGroupMap : DbObject
{
    /// <summary>
    /// The unique identifier of the topic
    /// </summary>
    [JsonPropertyName("topicId")]
    [Column("topic_id", Unique = true)]
    public Guid TopicId { get; set; }

    /// <summary>
    /// The unique identifier of the group
    /// </summary>
    [JsonPropertyName("groupId")]
    [Column("group_id", Unique = true)]
    public Guid GroupId { get; set; }

    /// <summary>
    /// Represents the `noti_topic_group_map` table.
    /// </summary>
    public TopicGroupMap() { }

    /// <summary>
    /// Represents the `noti_topic_group_map` table.
    /// </summary>
    /// <param name="topicId">The unique identifier of the topic</param>
    /// <param name="groupId">The unique identifier of the group</param>
    public TopicGroupMap(Guid topicId, Guid groupId)
    {
        TopicId = topicId;
        GroupId = groupId;
    }
}
