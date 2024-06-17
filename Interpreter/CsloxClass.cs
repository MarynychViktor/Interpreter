namespace Interpreter;


public class CsloxClass : ICsloxCallable
{
    public CsloxClass? Superclass { get; }
    private readonly Dictionary<string, CsloxFunction> _methods;
    public string Name { get; }

    public CsloxClass(string name, CsloxClass? superclass, Dictionary<string, CsloxFunction> methods)
    {
        Superclass = superclass;
        _methods = methods;
        Name = name;
    }

    public override string ToString() => Name;

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new CsloxInstance(this);
        var initializer = FindMethod("init"); 

        if (initializer != null) {
            initializer.Bind(instance).Call(interpreter, arguments);
        }

        return instance;
    }

    public int Arity()
    {
        var initializer = FindMethod("init");
        if (initializer == null) return 0;
        return initializer.Arity();
    }

    public CsloxFunction FindMethod(string name)
    {
        if (_methods.ContainsKey(name)) {
            return _methods[name];
        }

        if (Superclass != null)
        {
            return Superclass.FindMethod(name);
        }

        return null;
    }
}