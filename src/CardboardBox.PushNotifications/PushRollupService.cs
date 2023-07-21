namespace CardboardBox.PushNotifications;

using Devices;
using Groups;
using Notifications;
using Subscriptions;

/// <summary>
/// A rollup service that contains all of the services for handling notifications
/// </summary>
public interface IPushRollupService
{
    /// <summary>
    /// Service handling device management
    /// </summary>
    IDeviceService Devices { get; }

    /// <summary>
    /// Service handling sending notifications
    /// </summary>
    INotificationService Notifications { get; }

    /// <summary>
    /// Service handling subscriptions
    /// </summary>
    ISubscriptionService Subscriptions { get; }

    /// <summary>
    /// Service handling unsubscriptions
    /// </summary>
    IUnsubscriptionService Unsubscriptions { get; }

    /// <summary>
    /// Service handling groups
    /// </summary>
    IGroupService Groups { get; }
}

/// <summary>
/// The implementation of the <see cref="IPushRollupService"/>
/// </summary>
public class PushRollupService : IPushRollupService
{
    /// <summary>
    /// Service handling device management
    /// </summary>
    public IDeviceService Devices { get; }

    /// <summary>
    /// Service handling sending notifications
    /// </summary>
    public INotificationService Notifications { get; }

    /// <summary>
    /// Service handling subscriptions
    /// </summary>
    public ISubscriptionService Subscriptions { get; }

    /// <summary>
    /// Service handling unsubscriptions
    /// </summary>
    public IUnsubscriptionService Unsubscriptions { get; }

    /// <summary>
    /// Service handling groups
    /// </summary>
    public IGroupService Groups { get; }

    /// <summary>
    /// The implementation of the <see cref="IPushRollupService"/>
    /// </summary>
    /// <param name="devices">Service handling device management</param>
    /// <param name="notifications">Service handling sending notifications</param>
    /// <param name="subscriptions">Service handling subscriptions</param>
    /// <param name="unsubscriptions">Service handling unsubscriptions</param>
    /// <param name="groups">Service handling groups</param>
    public PushRollupService(
        IDeviceService devices,
        INotificationService notifications,
        ISubscriptionService subscriptions,
        IUnsubscriptionService unsubscriptions,
        IGroupService groups)
    {
        Devices = devices;
        Notifications = notifications;
        Subscriptions = subscriptions;
        Unsubscriptions = unsubscriptions;
        Groups = groups;
    }
}
