namespace Interpreter;


public class CsloxClass : ICsloxCallable
{
    public string Name { get; }

    public CsloxClass(string name)
    {
        Name = name;
    }

    public override string ToString() => Name;

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new CsloxInstance(this);
        return instance;
    }

    public int Arity()
    {
        return 0;
    }
}