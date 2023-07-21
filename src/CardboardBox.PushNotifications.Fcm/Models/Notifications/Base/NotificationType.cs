namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// Represents the type of notification
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// The request was a single notification (<see cref="Message"/>)
    /// </summary>
    Single = 1,
    /// <summary>
    /// The request was sent to multiple devices (<see cref="MulticastMessage"/>)
    /// </summary>
    Multicast = 2,
    /// <summary>
    /// The request was a batch of single notifications (<see cref="Message"/>)
    /// </summary>
    Batch = 3,
}
