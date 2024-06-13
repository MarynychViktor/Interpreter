namespace Interpreter;

public class RuntimeError : Exception
{
    public readonly Token Token;

    public RuntimeError(Token token, string? message) : base(message)
    {
        Token = token;
    }
}