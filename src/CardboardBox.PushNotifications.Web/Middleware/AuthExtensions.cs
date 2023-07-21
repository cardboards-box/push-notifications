namespace CardboardBox.PushNotifications.Web;

/// <summary>
/// A handful of useful extensions for handling <see cref="ClaimsPrincipal"/>s
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Gets the given claim from the principal
    /// </summary>
    /// <param name="principal">The principal to get the claim from</param>
    /// <param name="claim">The claim to get</param>
    /// <returns>The value of the claim or null if the claim wasn't found</returns>
    public static string? Claim(this ClaimsPrincipal principal, string claim)
    {
        return principal?.FindFirst(claim)?.Value;
    }

    /// <summary>
    /// Gets the given claim from the principal attached to the given controller
    /// </summary>
    /// <param name="ctrl">The controller to get the principal from</param>
    /// <param name="claim">The claim to get</param>
    /// <returns>The value of the claim or null if the claim wasn't found</returns>
    public static string? Claim(this ControllerBase ctrl, string claim)
    {
        return ctrl.User?.Claim(claim);
    }

    /// <summary>
    /// Checks if the current user has the given role
    /// </summary>
    /// <param name="ctrl">The controller to get the user from</param>
    /// <param name="role">The role to check</param>
    /// <returns>Whether or not the user has the given role</returns>
    public static bool IsInRole(this ControllerBase ctrl, string role)
    {
        return ctrl.User?.IsInRole(role) ?? false;
    }

    /// <summary>
    /// Checks whether or not the current user has the <see cref="Roles.APPLICATION"/> role
    /// </summary>
    /// <param name="ctrl">The controller to get the user from</param>
    /// <returns>Whether or not the user has the <see cref="Roles.APPLICATION"/> role</returns>
    public static bool IsApplication(this ControllerBase ctrl)
    {
        return ctrl.IsInRole(Roles.APPLICATION);
    }

    /// <summary>
    /// Checks whether or not the current user has the <see cref="Roles.ADMIN"/> role
    /// </summary>
    /// <param name="ctrl">The controller to get the user from</param>
    /// <returns>Whether or not the user has the <see cref="Roles.ADMIN"/> role</returns>
    public static bool IsAdmin(this ControllerBase ctrl)
    {
        return ctrl.IsInRole(Roles.ADMIN);
    }

    /// <summary>
    /// Gets the currently authentication user's application ID
    /// </summary>
    /// <param name="ctrl">The controller to get the application ID from</param>
    /// <returns>The application ID or null if not found or invalid</returns>
    public static Guid? ApplicationId(this ControllerBase ctrl)
    {
        var claim = ctrl.Claim(ClaimTypes.NameIdentifier);

        return string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var res) ? null : res;
    }

    /// <summary>
    /// Gets the name of the currently authenticated user's application
    /// </summary>
    /// <param name="ctrl">The controller to get the application name from</param>
    /// <returns>The application name or null if not found</returns>
    public static string? ApplicationName(this ControllerBase ctrl)
    {
        return ctrl.Claim(ClaimTypes.Name);
    }
}
