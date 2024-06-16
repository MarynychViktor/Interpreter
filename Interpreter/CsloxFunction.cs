namespace Interpreter;

public class CsloxFunction : ICsloxCallable
{
    private readonly Stmt.Function _declaration;

    public CsloxFunction(Stmt.Function declaration)
    {
        _declaration = declaration;
    }

    public override string ToString()
    {
        return "<fn " + _declaration.Name.lexeme + ">";
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var environment = new LanguageEnvironment(interpreter.Globals);
        for (int i = 0; i < _declaration.FunParams.Count(); i++) {
            environment.Define(_declaration.FunParams[i].lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.Value;
        }

        return null;
    }

    public int Arity() => _declaration.FunParams.Count;
}