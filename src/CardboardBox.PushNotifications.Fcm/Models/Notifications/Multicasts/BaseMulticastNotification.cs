namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// The interface that represents a <see cref="MulticastMessage"/> notification that is sent to FCM
/// </summary>
public interface IMulticastNotification : INotification
{
    /// <summary>
    /// Generates all of the FCM messages to be sent for this notification
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    IEnumerable<MulticastMessage> GetMessages();
}

/// <summary>
/// The implementation of the <see cref="IMulticastNotification"/> interface
/// </summary>
public abstract class BaseMulticastNotification : BaseNotification, IMulticastNotification
{
    /// <summary>
    /// The base notification data
    /// </summary>
    /// <param name="data">The data to send with the notification</param>
    public BaseMulticastNotification(NotificationData data) : base(data) { }

    /// <summary>
    /// Validates the notification data and generates all of the FCM messages to be sent for this notification
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    public virtual IEnumerable<MulticastMessage> GetMessages()
    {
        ValidateNotification();
        return GenerateMessages();
    }

    /// <summary>
    /// Generates all of the FCM messages to be sent for this notification
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    public abstract IEnumerable<MulticastMessage> GenerateMessages();
}
