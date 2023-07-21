namespace CardboardBox.PushNotifications.Web;

/// <summary>
/// Helper class that contains all of the roles for the <see cref="AuthorizeAttribute"/>
/// </summary>
public static class Roles
{
    /// <summary>
    /// The role that represents a logged in application
    /// </summary>
    public const string APPLICATION = "Application";

    /// <summary>
    /// The role that represents a user with admin privileges
    /// </summary>
    public const string ADMIN = "Admin";
}
