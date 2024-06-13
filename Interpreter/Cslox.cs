// See https://aka.ms/new-console-template for more information

using System.Text;

namespace Interpreter;

class Cslox
{
    private static bool HadError { get; set; }

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
        var expr = parser.Parse();
        Console.WriteLine(new AstPrinter().Print(expr));
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
}