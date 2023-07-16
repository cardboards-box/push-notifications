namespace CardboardBox.PushNotifications.Web;

public static class AuthExtensions
{
    public static string? Claim(this ClaimsPrincipal principal, string claim)
    {
        return principal?.FindFirst(claim)?.Value;
    }

    public static string? Claim(this ControllerBase ctrl, string claim)
    {
        return ctrl.User?.Claim(claim);
    }

    public static bool IsInRole(this ControllerBase ctrl, string role)
    {
        return ctrl.User?.IsInRole(role) ?? false;
    }

    public static bool IsApplication(this ControllerBase ctrl)
    {
        return ctrl.IsInRole(Roles.APPLICATION);
    }

    public static bool IsAdmin(this ControllerBase ctrl)
    {
        return ctrl.IsInRole(Roles.ADMIN);
    }

    public static Guid? ApplicationId(this ControllerBase ctrl)
    {
        var claim = ctrl.Claim(ClaimTypes.NameIdentifier);

        return string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var res) ? null : res;
    }

    public static string? ApplicationName(this ControllerBase ctrl)
    {
        return ctrl.Claim(ClaimTypes.Name);
    }
}
