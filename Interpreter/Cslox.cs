// See https://aka.ms/new-console-template for more information

using System.Text;

namespace Interpreter;

class Cslox
{
    private static bool HadError { get; set; }
    private static bool HadRuntimeError { get; set; }
    private static Interpreter _iterpreter = new Interpreter();

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: cslox [script]");
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
            Environment.Exit(64);
        }
        else
        {
            RunPrompt();
        }

        if (HadError) Environment.Exit(65);
    }

    private static void RunFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        Run(Encoding.Default.GetString(bytes));
        
        if (HadError) Environment.Exit(65);
        if (HadRuntimeError) Environment.Exit(70);
    }

    private static void RunPrompt()
    {
        for (;;)
        {
            Console.WriteLine("> ");
            var line = Console.ReadLine();
            if (line == null)
            {
                break;
            }

            Run(line);
            HadError = false;
        }
    }

    private static void Run(string line)
    {
        var scanner = new Scanner(line);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var statements = parser.Parse();
        if (HadError) return;

        _iterpreter.Interpret(statements);
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, " at '" + token.lexeme + "'", message);
        }
    }

    public static void Report(int line, String where, String message)
    {
        Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
        HadError = true;
    }


    public static void RuntimeError(RuntimeError error)
    {
        Console.WriteLine(error.Message + "\n[line " + error.Token.line + "]");
        HadRuntimeError = true;
    }
}