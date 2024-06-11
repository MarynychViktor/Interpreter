// See https://aka.ms/new-console-template for more information

using System.Text;
using Interpreter;

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
    
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }
    
    public static void Report(int line, String where, String message) {
        Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
        HadError = true;
    }
}

