namespace CardboardBox.PushNotifications.Web.Controllers;

using Subscriptions;

/// <summary>
/// Handles all of the subscription management for a user
/// </summary>
public class SubscriptionController : ControllerAuthedBase
{
    private readonly IPushRollupService _rollup;

    /// <summary>
    /// Handles all of the subscription management for a user
    /// </summary>
    /// <param name="rollup">The service that handles all interactions</param>
    public SubscriptionController(IPushRollupService rollup)
    {
        _rollup = rollup;
    }

    /// <summary>
    /// Gets all of the subscriptions for a user
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <returns>All of the users subscriptions, scoped to the current application</returns>
    [HttpGet, Route("subscription/{profileId}")]
    [RequestResults<UserSubscriptions>, ExceptionResults(401), ExceptionResults(400)]
    public async Task<IActionResult> Get([FromRoute] string profileId)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId)) return BadRequest(WasBadRequest("Profile ID cannot be null"));

        var subs = await _rollup.Subscriptions.Get(appId.Value, profileId);
        return Ok(subs);
    }

    /// <summary>
    /// Subscribes the user to a topic
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <param name="topic">The topic to subscribe to</param>
    /// <returns>That status code of the request</returns>
    [HttpGet, Route("subscription/{profileId}/topic/{topic}")]
    [RequestResults, RequestResults(204), 
     ExceptionResults(401), ExceptionResults(400), ExceptionResults(500)]
    public async Task<IActionResult> SubTopic([FromRoute] string profileId, [FromRoute] string topic)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(topic)) return BadRequest(WasBadRequest("Profile ID and topic cannot be null"));

        var result = await _rollup.Subscriptions.Topic(appId.Value, topic, profileId);
        return StatusCode((int)result.Code, result);
    }

    /// <summary>
    /// Unsubscribes the user from a topic
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <param name="topic">The topic to subscribe to</param>
    /// <returns>That status code of the request</returns>
    [HttpDelete, Route("subscription/{profileId}/topic/{topic}")]
    [RequestResults, RequestResults(204),
     ExceptionResults(401), ExceptionResults(400), ExceptionResults(500)]
    public async Task<IActionResult> UnsubTopic([FromRoute] string profileId, [FromRoute] string topic)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(topic)) return BadRequest(WasBadRequest("Profile ID and topic cannot be null"));

        var result = await _rollup.Unsubscriptions.Topic(appId.Value, topic, profileId);
        return StatusCode((int)result.Code, result);
    }

    /// <summary>
    /// Subscribes the user to a group of topics
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <param name="resourceId">The topic to subscribe to</param>
    /// <returns>That status code of the request</returns>
    [HttpGet, Route("subscription/{profileId}/group/{resourceId}")]
    [RequestResults, RequestResults(204),
     ExceptionResults(401), ExceptionResults(400), ExceptionResults(500)]
    public async Task<IActionResult> SubGroup([FromRoute] string profileId, [FromRoute] string resourceId)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(resourceId)) return BadRequest(WasBadRequest("Profile ID and Resource ID cannot be null"));

        var result = await _rollup.Subscriptions.Group(appId.Value, resourceId, profileId);
        return StatusCode((int)result.Code, result);
    }

    /// <summary>
    /// Subscribes the user to a group of topics
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <param name="resourceId">The topic to subscribe to</param>
    /// <returns>That status code of the request</returns>
    [HttpDelete, Route("subscription/{profileId}/group/{resourceId}")]
    [RequestResults, RequestResults(204),
     ExceptionResults(401), ExceptionResults(400), ExceptionResults(500)]
    public async Task<IActionResult> UnsubGroup([FromRoute] string profileId, [FromRoute] string resourceId)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId) || string.IsNullOrEmpty(resourceId)) return BadRequest(WasBadRequest("Profile ID and Resource ID cannot be null"));

        var result = await _rollup.Unsubscriptions.Group(appId.Value, resourceId, profileId);
        return StatusCode((int)result.Code, result);
    }
}
