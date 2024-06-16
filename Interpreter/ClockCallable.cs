namespace Interpreter;

public class ClockCallable : ICsloxCallable
{
    public object Call(Interpreter interpreter, List<object> arguments)
    {
       return (double) DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    public int Arity() => 0;
}