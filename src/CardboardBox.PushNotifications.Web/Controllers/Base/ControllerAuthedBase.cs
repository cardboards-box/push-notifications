namespace CardboardBox.PushNotifications.Web.Controllers;

/// <summary>
/// Represents an api controller that requires the <see cref="Roles.APPLICATION"/> role
/// </summary>
[ApiController, Authorize(Roles = Roles.APPLICATION)]
public abstract class ControllerAuthedBase : ControllerBase
{
    /// <summary>
    /// Gets an instance of the validator
    /// </summary>
    public RequestValidator Validator => new();
}
