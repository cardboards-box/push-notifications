namespace CardboardBox.PushNotifications;

using Core;
using Fcm;

/// <summary>
/// Dependency injection extensions for the FCM service.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Register all of the FCM services with the dependency resolver
    /// </summary>
    /// <param name="resolver">The dependency resolver</param>
    /// <returns>The dependency resolver for chaining</returns>
    public static IDependencyResolver AddFcm(this IDependencyResolver resolver)
    {
        return resolver
            .Singleton<IFcmService, FcmService>()
            .Transient<IFcmTopicService, FcmTopicService>()
            .Transient<IFcmNotificationService, FcmNotificationService>();
    }
}
