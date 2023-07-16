namespace CardboardBox.PushNotifications.Database.Models;

/// <summary>
/// Represents the `noti_topic_groups` table.
/// Contains the information regarding a group of topics.
/// </summary>
[Table("noti_topic_groups")]
public class TopicGroup : DbObject
{
    /// <summary>
    /// The foreign key that references which application this topic group belongs to.
    /// </summary>
    [JsonPropertyName("applicationId")]
    [Column("application_id", Unique = true)]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// The resource this group represents.
    /// This is provided by the application and can be used to identify the group.
    /// </summary>
    [JsonPropertyName("resourceId")]
    [Column("resource_id", Unique = true)]
    public string ResourceId { get; set; } = string.Empty;
}
