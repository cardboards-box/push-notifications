namespace CardboardBox.PushNotifications.Web.Controllers;

[ApiController, Authorize(Roles = Roles.APPLICATION)]
public abstract class ControllerAuthedBase : ControllerBase
{
    public RequestValidator Validator => new();
}
