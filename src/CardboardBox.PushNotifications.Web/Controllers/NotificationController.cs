namespace CardboardBox.PushNotifications.Web.Controllers;

using Fcm;

public class NotificationController : ControllerAuthedBase
{
    private readonly ISubscriptionService _subscriptions;

    public NotificationController(ISubscriptionService subscriptions)
    {
        _subscriptions = subscriptions;
    }

    [HttpPost, Route("notification/user/{profileId}")]
    [RequestResults, RequestResults(200), ExceptionResults(500), ExceptionResults(401), ExceptionResults(400)]
    public async Task<IActionResult> DirectNotification([FromRoute] string profileId, [FromBody] NotificationData request)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());

        var validator = Validator
            .NotNull(profileId, nameof(profileId))
            .NotNull(request.Title, nameof(request.Title))
            .NotNull(request.Body, nameof(request.Body));

        if (!validator.Valid) return BadRequest(WasBadRequest(validator));

        var result = await _subscriptions.UserNotification(appId.Value, profileId, request);
        return StatusCode((int)result.Code, result);
    }
}
