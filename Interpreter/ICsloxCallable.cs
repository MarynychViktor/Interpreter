namespace Interpreter;

public interface ICsloxCallable
{
    Object Call(Interpreter interpreter, List<object> arguments);
    int Arity();
}