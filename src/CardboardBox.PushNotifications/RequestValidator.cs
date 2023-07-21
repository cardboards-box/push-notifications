namespace CardboardBox.PushNotifications;

/// <summary>
/// Validates a request
/// </summary>
public class RequestValidator
{
    /// <summary>
    /// The validation methods
    /// </summary>
    private readonly List<Validator> validators = new();

    /// <summary>
    /// All of the issues that occurred during validation
    /// </summary>
    public string[] Issues => GetIssues().ToArray();

    /// <summary>
    /// Whether or not the request was valid
    /// </summary>
    public bool Valid => !Issues.Any();

    /// <summary>
    /// Adds the given validation to the validator
    /// </summary>
    /// <param name="valid">The validation function</param>
    /// <param name="message">The message to return if the validation function failed</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator Add(Func<bool> valid, string message)
    {
        validators.Add(new Validator(valid, message));
        return this;
    }

    /// <summary>
    /// Validates the given value to ensure it's not null, empty, or whitespace
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="property">The name of the property that was validated</param>
    /// <returns>The current validator for chaining</returns>
    public RequestValidator NotNull(string? value, string property)
    {
        validators.Add(new Validator(() => !string.IsNullOrWhiteSpace(value), $"{property} cannot be null or empty."));
        return this;
    }

    /// <summary>
    /// Gets all of the issues that occurred in the validation
    /// </summary>
    /// <returns>All of the issues that were found</returns>
    public IEnumerable<string> GetIssues()
    {
        foreach(var validator in validators)
            if (!validator.Valid())
                yield return validator.Message;
    }

    /// <summary>
    /// Represents a validation function
    /// </summary>
    /// <param name="Valid">The validator function</param>
    /// <param name="Message">The message to return if the validation function failed</param>
    private record class Validator(Func<bool> Valid, string Message);
}
