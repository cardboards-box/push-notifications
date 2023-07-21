namespace CardboardBox.PushNotifications;

using Core;
using Devices;
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
            .Transient<IKeyGenService, KeyGenService>()
            .Transient<INotificationService, NotificationService>()
            .Transient<ISubscriptionService, SubscriptionService>()
            .Transient<IUnsubscriptionService, UnsubscriptionService>()
            .Transient<IDeviceService, DeviceService>()

            .Transient<IPushRollupService, PushRollupService>();
    }
}
