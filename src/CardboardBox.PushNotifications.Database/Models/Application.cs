namespace CardboardBox.PushNotifications.Database.Models;

/// <summary>
/// Represents the `noti_application` table.
/// Contains all of the applications that are registered with the service.
/// </summary>
[Table("noti_applications")]
public class Application : DbObject
{
    /// <summary>
    /// The display name of the application.
    /// </summary>
    [JsonPropertyName("name")]
    [Column(Unique = true)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The secret token used by the application to authenticate with the service.
    /// </summary>
    [JsonIgnore]
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// A collection of unique indentifiers of the users who own this application. 
    /// </summary>
    [JsonPropertyName("ownerIds")]
    [Column("owner_ids")]
    public string[] OwnerIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether or not the application has permission to modify other applications
    /// </summary>
    [JsonPropertyName("isAdmin")]
    [Column("is_admin")]
    public bool IsAdmin { get; set; } = false;
}
