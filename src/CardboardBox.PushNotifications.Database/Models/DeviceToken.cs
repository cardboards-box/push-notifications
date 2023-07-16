namespace CardboardBox.PushNotifications.Database.Models;

using Types;

/// <summary>
/// Represents the `noti_device_tokens` table.
/// Contains all of the device tokens related to a specific user.
/// </summary>
[Table("noti_device_tokens")]
public class DeviceToken : DbObject
{
    /// <summary>
    /// The foreign key that references which application this device token belongs to.
    /// </summary>
    [JsonPropertyName("applicationId")]
    [Column("application_id", Unique = true)]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// The unique identifier for the user who owns this device.
    /// This is not managed by this service and is up to the granting application to provide
    /// </summary>
    [JsonPropertyName("profileId")]
    [Column("profile_id", Unique = true)]
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The device token that represents this device.
    /// </summary>
    [JsonPropertyName("token")]
    [Column(Unique = true)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The display name for this device (This is user defined).
    /// </summary>
    [JsonPropertyName("name")]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The user-agent from the device 
    /// </summary>
    [JsonPropertyName("userAgent")]
    [Column("user_agent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// The type of device that granted to the token
    /// </summary>
    [JsonPropertyName("deviceType")]
    [Column("device_type")]
    public PushDeviceType DeviceType { get; set; } = PushDeviceType.Web;

    /// <summary>
    /// The provider type that granted the token
    /// </summary>
    [JsonPropertyName("providerType")]
    [Column("provider_type")]
    public ProviderType ProviderType { get; set; } = ProviderType.FCM;
}
