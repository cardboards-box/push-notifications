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
}
