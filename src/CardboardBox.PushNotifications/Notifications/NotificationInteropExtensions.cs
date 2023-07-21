namespace CardboardBox.PushNotifications.Notifications;

/// <summary>
/// Provides helpful extensions for interoperability between the database and FCM notification services
/// </summary>
public static class NotificationInteropExtensions
{
    /// <summary>
    /// Send a notification to a user's devices
    /// </summary>
    /// <param name="fcm">The notification service to send to</param>
    /// <param name="data">The notification data to send</param>
    /// <param name="devices">The devices to send the notification to</param>
    /// <returns>All of the notification results</returns>
    public static ValueTask<NotificationResponse[]> Direct(this IFcmNotificationService fcm, NotificationData data, params DeviceToken[] devices)
    {
        var tokens = devices.Select(t => t.Token).ToArray();
        return fcm.Send(b => 
        {
            b.Direct(data, tokens);
        }).ToArrayAsync();
    }

    /// <summary>
    /// Send a notification to all of the given topics
    /// </summary>
    /// <param name="fcm">The notification service to send to</param>
    /// <param name="data">The notification data to send</param>
    /// <param name="topics">The topics to send the notifications to</param>
    /// <returns>All of the notification results</returns>
    public static ValueTask<NotificationResponse[]> Topic(this IFcmNotificationService fcm, NotificationData data, params Topic[] topics)
    {
        var topicHashes = topics.Select(t => t.TopicHash).ToArray();
        return fcm.Send(b =>
        {
            b.Topic(data, topicHashes);
        }).ToArrayAsync();
    }
}
