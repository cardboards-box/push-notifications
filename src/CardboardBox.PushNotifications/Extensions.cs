namespace CardboardBox.PushNotifications;

using Core;
using Fcm;

public static class Extensions
{
    public static IDependencyResolver AddNotifications(this IDependencyResolver builder)
    {
        return builder
            .Singleton<IFcmNotificationService, FcmNotificationService>()
            .Transient<IKeyGenService, KeyGenService>()
            .Transient<ISubscriptionService, SubscriptionService>();
    }
}
