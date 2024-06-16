namespace Interpreter;

public class LanguageEnvironment
{
    public readonly LanguageEnvironment? Enclosing;
    private readonly Dictionary<string, object> values = new();

    public LanguageEnvironment()
    {
    }

    public LanguageEnvironment(LanguageEnvironment enclosing)
    {
        Enclosing = enclosing;
    }

    public void Define(string name, object value)
    {
        values.Add(name, value);
    }

    public object Get(Token name)
    {
        if (values.ContainsKey(name.lexeme))
        {
            return values[name.lexeme];
        }

        if (Enclosing != null)
        {
            return Enclosing.Get(name);
        }

        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }
    
    public void Assign(Token name, object value)
    {
        if (values.ContainsKey(name.lexeme))
        {
            values[name.lexeme] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }

    public object GetAt(int? distance, string name)
    {
        return Ancestor(distance).values[name];
    }

    private LanguageEnvironment Ancestor(int? distance)
    {
        var environment = this;
        for (int i = 0; i < distance; i++) {
            environment = environment.Enclosing; 
        }

        return environment;
    }

    public void AssignAt(int? distance, Token name, object value)
    {
        Ancestor(distance).values[name.lexeme] = value;
    }
}