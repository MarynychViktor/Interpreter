namespace Interpreter;


public class CsloxClass : ICsloxCallable
{
    private readonly Dictionary<string, CsloxFunction> _methods;
    public string Name { get; }

    public CsloxClass(string name, Dictionary<string, CsloxFunction> methods)
    {
        _methods = methods;
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

    public CsloxFunction FindMethod(string name)
    {
        if (_methods.ContainsKey(name)) {
            return _methods[name];
        }

        return null;
    }
}