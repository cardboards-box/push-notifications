namespace CardboardBox.PushNotifications.Database.Types;

/// <summary>
/// The notification provider type
/// </summary>
public enum ProviderType
{
    /// <summary>
    /// Unknown and not supported
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Firebase Cloud Messaging (FCM)
    /// </summary>
    FCM = 1,
    /// <summary>
    /// Apple Push Notification Service (APNS) (Not supported yet)
    /// </summary>
    APNS = 2
}
