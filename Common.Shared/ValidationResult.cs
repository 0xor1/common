namespace Common.Shared;

public record ValidationResult
{
    public static ValidationResult New(string msgKey = S.Invalid, object? msgModel = null)
    {
        return new(true, msgKey, msgModel);
    }

    private ValidationResult(bool valid, string msgKey = S.Invalid, object? msgModel = null)
    {
        _valid = valid;
        Message = new Message(msgKey, msgModel);
    }

    private bool _valid;
    public bool Valid => _valid && _subResults.All(x => x.Valid);
    public Message Message { get; private set; }
    private List<ValidationResult> _subResults = new();
    public IReadOnlyList<ValidationResult> SubResults => _subResults;

    public bool InvalidIf(bool condition, string? msgKey = null, object? msgModel = null)
    {
        if (condition)
        {
            _valid = false;
            if (!msgKey.IsNullOrEmpty())
            {
                _subResults.Add(new(false, msgKey, msgModel));
            }
        }
        return condition;
    }

    public ValidationResult NewSubResult(string msgKey = S.Invalid, object? msgModel = null)
    {
        var res = new ValidationResult(true, msgKey, msgModel);
        _subResults.Add(res);
        return res;
    }
}
