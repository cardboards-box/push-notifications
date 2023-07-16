namespace CardboardBox.PushNotifications.Database.Models;

/// <summary>
/// A model that represents a database object
/// </summary>
public abstract class DbObject
{
    /// <summary>
    /// The unique identifier for this object
    /// </summary>
    [JsonPropertyName("id")]
    [Column(PrimaryKey = true, ExcludeInserts = true, ExcludeUpdates = true)]
    public virtual Guid Id { get; set; }

    /// <summary>
    /// The date and time the object was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    [Column("created_at", ExcludeInserts = true, ExcludeUpdates = true, OverrideValue = "CURRENT_TIMESTAMP")]
    public virtual DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time the object was last updated
    /// </summary>
    [JsonPropertyName("updatedAt")]
    [Column("updated_at", ExcludeInserts = true, OverrideValue = "CURRENT_TIMESTAMP")]
    public virtual DateTime UpdatedAt { get; set; }
}
