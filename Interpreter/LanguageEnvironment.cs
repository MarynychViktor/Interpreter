namespace Interpreter;

public class LanguageEnvironment
{
    private readonly Dictionary<string, object> values = new();

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

        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }
    
    public void Assign(Token name, object value)
    {
        if (values.ContainsKey(name.lexeme))
        {
            values[name.lexeme] = value;
            return;
        }

        throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
    }
}