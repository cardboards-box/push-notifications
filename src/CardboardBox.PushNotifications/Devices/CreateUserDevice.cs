using System.ComponentModel.DataAnnotations;

namespace CardboardBox.PushNotifications.Devices;

/// <summary>
/// Represents the request to create a user device
/// </summary>
/// <param name="Token">The users device token</param>
/// <param name="Name">The name of the users device</param>
/// <param name="UserAgent">The optional user-agent that created the device</param>
/// <param name="DeviceType">The optional device type, will default to <see cref="PushDeviceType.Web"/></param>
/// <param name="ProviderType">The optional provider type, will default to <see cref="ProviderType.FCM"/></param>
public record class CreateUserDevice(
    [property: JsonPropertyName("token"), Required] string Token,
    [property: JsonPropertyName("name"), Required] string Name,
    [property: JsonPropertyName("userAgent")] string? UserAgent,
    [property: JsonPropertyName("deviceType")] PushDeviceType? DeviceType,
    [property: JsonPropertyName("providerType")] ProviderType? ProviderType);
