namespace Interpreter;

public class CsloxFunction : ICsloxCallable
{
    private readonly Stmt.Function _declaration;
    private readonly LanguageEnvironment _closure;
    private bool _isInitializer;

    public CsloxFunction(Stmt.Function declaration, LanguageEnvironment closure, bool isInitializer)
    {
        _isInitializer = isInitializer;
        _declaration = declaration;
        _closure = closure;
    }

    public override string ToString()
    {
        return "<fn " + _declaration.Name.lexeme + ">";
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var environment = new LanguageEnvironment(_closure);
        for (int i = 0; i < _declaration.FunParams.Count(); i++) {
            environment.Define(_declaration.FunParams[i].lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            if (_isInitializer) return _closure.GetAt(0, "this");

            return returnValue.Value;
        }
        if (_isInitializer) return _closure.GetAt(0, "this");
        return null;
    }

    public int Arity() => _declaration.FunParams.Count;

    public CsloxFunction Bind(CsloxInstance instance)
    {
        var environment = new LanguageEnvironment(_closure);
        environment.Define("this", instance);
        return new CsloxFunction(_declaration, environment, _isInitializer);
    }
}