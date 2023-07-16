namespace CardboardBox.PushNotifications;

public class RequestValidator
{
    private readonly List<Validator> validators = new();

    public string[] Issues => GetIssues().ToArray();

    public bool Valid => !Issues.Any();

    public RequestValidator Add(Func<bool> valid, string message)
    {
        validators.Add(new Validator(valid, message));
        return this;
    }

    public RequestValidator NotNull(string? value, string property)
    {
        validators.Add(new Validator(() => !string.IsNullOrWhiteSpace(value), $"{property} cannot be null or empty."));
        return this;
    }

    public IEnumerable<string> GetIssues()
    {
        foreach(var validator in validators)
            if (!validator.Valid())
                yield return validator.Message;
    }

    private record class Validator(Func<bool> Valid, string Message);
}
