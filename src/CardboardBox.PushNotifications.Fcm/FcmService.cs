namespace CardboardBox.PushNotifications.Fcm;

using Core;

/// <summary>
/// A service for handling the instances of the <see cref="FirebaseMessaging"/> service
/// </summary>
public interface IFcmService
{
    /// <summary>
    /// Get the <see cref="FirebaseMessaging"/> instance with the creds and scope.
    /// </summary>
    /// <returns></returns>
    FirebaseMessaging GetMessaging();
}

/// <summary>
/// The implementation of the <see cref="IFcmService"/>
/// </summary>
public class FcmService : IFcmService
{
    private readonly IConfiguration _config;
    private FirebaseMessaging? _messaging;

    /// <summary>
    /// Where to find the credentials JSON file for the FCM service
    /// </summary>
    public string CredentialsPath => _config.Required("FCM:CredentialsPath");

    /// <summary>
    /// The scope to use for the FCM credentials
    /// </summary>
    public string Scope => _config.Optional("FCM:Scope", "https://www.googleapis.com/auth/firebase.messaging");

    /// <summary>
    /// The dependency injection constructor for the <see cref="FcmService"/>
    /// </summary>
    /// <param name="config">The environment configuration for the application</param>
    public FcmService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Get the <see cref="FirebaseMessaging"/> instance with the creds and scope.
    /// </summary>
    /// <returns></returns>
    public FirebaseMessaging GetMessaging()
    {
        return _messaging ??= CreateInstance();
    }

    /// <summary>
    /// Create an instance of the firebase messaging handler
    /// </summary>
    /// <returns></returns>
    public FirebaseMessaging CreateInstance()
    {
        var creds = GoogleCredential
            .FromFile(CredentialsPath)
            .CreateScoped(Scope);
        var opts = new AppOptions
        {
            Credential = creds
        };
        var app = FirebaseApp.Create(opts);
        return FirebaseMessaging.GetMessaging(app);
    }
}
