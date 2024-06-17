namespace Interpreter;

public class CsloxInstance
{
    private readonly CsloxClass _klass;
    private readonly Dictionary<string, object> _fields = new();

    public CsloxInstance(CsloxClass klass)
    {
        _klass = klass;
    }

    public override string ToString()
    {
        return _klass.ToString() + " instance";
    }

    public object Get(Token name)
    {
        if (_fields.ContainsKey(name.lexeme))
        {
            return _fields[name.lexeme];
        }

        var method = _klass.FindMethod(name.lexeme);
        if (method != null) return method.Bind(this);

        throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
    }

    public void Set(Token name, object value)
    {
        _fields[name.lexeme] = value;
    }
}