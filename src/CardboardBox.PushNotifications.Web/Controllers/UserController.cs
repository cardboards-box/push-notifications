namespace CardboardBox.PushNotifications.Web.Controllers;

public class UserController : ControllerAuthedBase
{
    private readonly ISubscriptionService _subscriptions;
    private readonly IDbService _db;

    public UserController(
        ISubscriptionService subscriptions, 
        IDbService db)
    {
        _subscriptions = subscriptions;
        _db = db;
    }

    /// <summary>
    /// Gets all of the subscriptions for a user
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <returns>All of the users subscriptions, scoped to the current application</returns>
    [HttpGet, Route("user/{profileId}/subscriptions")]
    [RequestResults<UserSubscriptions>, ExceptionResults(401), ExceptionResults(400)]
    public async Task<IActionResult> Subscriptions([FromRoute] string profileId)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId)) return BadRequest(WasBadRequest("Profile ID cannot be null"));

        var subs = await _subscriptions.Subscriptions(appId.Value, profileId);
        return Ok(WasOk(subs));
    }

    /// <summary>
    /// Subscribes the user to notifications for a topic
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <param name="topicHash">The topic to subscribe the user to</param>
    /// <returns>The status code of the subscription request</returns>
    //[HttpGet, Route("user/{profileId}/subscribe/{topicHash}")]
    //[ProducesResponseType(401), ProducesResponseType(204), ProducesResponseType(400), 
    // ProducesResponseType(500), ProducesResponseType(200)]
    //public async Task<IActionResult> Subscribe([FromRoute] string profileId, [FromRoute] string topicHash)
    //{
    //    var appId = this.ApplicationId();
    //    if (appId == null) return Unauthorized();
    //    if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(topicHash)) return BadRequest();

    //    var code = await _subscriptions.SubscribeTopic(appId.Value, topicHash, profileId);
    //    return StatusCode((int)code);
    //}

    /// <summary>
    /// Gets all of the devices that a user has registered
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <returns>A collection of all of the users devices scoped to the current application</returns>
    [HttpGet, Route("user/{profileId}/devices")]
    [RequestResults<DeviceToken[]>, ExceptionResults(401), ExceptionResults(400)]
    public async Task<IActionResult> Devices([FromRoute] string profileId)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId)) return BadRequest(WasBadRequest("Profile ID cannot be null"));

        var devices = await _db.DeviceTokens.Get(appId.Value, profileId);
        return Ok(WasOk(devices));
    }

    /// <summary>
    /// Adds a new device to the users profile.
    /// This also subscribes them to any topics or groups they are subscribed to on other devices.
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <param name="request">The device information for the user</param>
    /// <returns>The status code of the device creation and subsequent subscription requests</returns>
    [HttpPost, Route("user/{profileId}/device")]
    [RequestResults, RequestResults(204), ExceptionResults(401), ExceptionResults(400), ExceptionResults(500)]
    public async Task<IActionResult> Device([FromRoute] string profileId, [FromBody] CreateUserDevice request)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId)) return BadRequest(WasBadRequest("Profile ID cannot be null"));

        var result = await _subscriptions.AddDevice(appId.Value, profileId, request);
        return StatusCode((int)result.Code, result);
    }
}
