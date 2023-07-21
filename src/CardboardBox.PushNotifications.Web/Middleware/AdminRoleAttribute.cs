namespace CardboardBox.PushNotifications.Web;

/// <summary>
/// Handles the <see cref="AuthorizeAttribute"/> for the admin role
/// </summary>
public class AdminRoleAttribute : AuthorizeAttribute
{
    /// <summary></summary>
    public AdminRoleAttribute()
    {
        Roles = Web.Roles.ADMIN;
    }
}
