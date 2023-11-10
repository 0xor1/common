namespace Common.Shared;

public class Message
{
    public Message(string key, object? model = null)
    {
        Key = key;
        Model = model;
    }

    public string Key { get; set; }
    public object? Model { get; set; }
}
