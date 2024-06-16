namespace Interpreter;

public class Return : Exception
{
    public object? Value { get; }

    public Return(object? value) : base(null, null)
    {
        Value = value;
    }
}