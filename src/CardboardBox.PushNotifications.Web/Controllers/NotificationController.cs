namespace CardboardBox.PushNotifications.Web.Controllers;

using Fcm;

/// <summary>
/// Handles all interactions with sending push notifications
/// </summary>
public class NotificationController : ControllerAuthedBase
{
    private readonly IPushRollupService _rollup;

    /// <summary>
    /// Handles all interactions with sending push notifications
    /// </summary>
    /// <param name="rollup">The service that handles all interactions</param>
    public NotificationController(IPushRollupService rollup)
    {
        _rollup = rollup;
    }

    /// <summary>
    /// Sends a push notification directly to the given user
    /// </summary>
    /// <param name="profileId">The unique identifier for the user to notification is for</param>
    /// <param name="request">The notification to be sent to the user</param>
    /// <returns>The status indicating the result code of the request</returns>
    [HttpPost, Route("notification/user/{profileId}")]
    [RequestResults, RequestResults(200), ExceptionResults(500), ExceptionResults(401), ExceptionResults(400)]
    public async Task<IActionResult> Direct([FromRoute] string profileId, [FromBody] NotificationData request)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());

        var validator = Validator
            .NotNull(profileId, nameof(profileId))
            .NotNull(request.Title, nameof(request.Title))
            .NotNull(request.Body, nameof(request.Body));

        if (!validator.Valid) return BadRequest(WasBadRequest(validator));

        var result = await _rollup.Notifications.Direct(appId.Value, profileId, request);
        return StatusCode((int)result.Code, result);
    }

    /// <summary>
    /// Sends a push notification to the given topic 
    /// </summary>
    /// <param name="topic">The topic to send the notification to</param>
    /// <param name="request">The notification to be sent to the topic</param>
    /// <returns>That status indicating the result code of the request</returns>
    [HttpPost, Route("notification/topic/{topic}")]
    [RequestResults, RequestResults(200), ExceptionResults(500), ExceptionResults(401), ExceptionResults(400)]
    public async Task<IActionResult> Topic([FromRoute] string topic, [FromBody] NotificationData request)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());

        var validator = Validator
            .NotNull(topic, nameof(topic))
            .NotNull(request.Title, nameof(request.Title))
            .NotNull(request.Body, nameof(request.Body));

        if (!validator.Valid) return BadRequest(WasBadRequest(validator));

        var result = await _rollup.Notifications.Topic(appId.Value, topic, request);
        return StatusCode((int)result.Code, result);
    }
}
