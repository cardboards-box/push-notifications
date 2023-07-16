namespace CardboardBox.PushNotifications.Web;

public class AdminRoleAttribute : AuthorizeAttribute
{
    public AdminRoleAttribute()
    {
        Roles = Web.Roles.ADMIN;
    }
}
