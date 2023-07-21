namespace CardboardBox.PushNotifications;

using Core;
using Devices;
using Groups;
using Notifications;
using Subscriptions;

/// <summary>
/// Dependency injection extensions to register the push notification services
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers all of the services in this assembly
    /// </summary>
    /// <param name="builder">The dependency resolver to add the services to</param>
    /// <returns>The dependency resolver for chaining</returns>
    public static IDependencyResolver AddNotifications(this IDependencyResolver builder)
    {
        return builder
            .Transient<IGroupService, GroupService>()
            .Transient<IKeyGenService, KeyGenService>()
            .Transient<IDeviceService, DeviceService>()
            .Transient<INotificationService, NotificationService>()
            .Transient<ISubscriptionService, SubscriptionService>()
            .Transient<IUnsubscriptionService, UnsubscriptionService>()

            .Transient<IPushRollupService, PushRollupService>();
    }
}
