// See https://aka.ms/new-console-template for more information

namespace Tools;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }

        var outputDir = args[0];
        DefineAst(outputDir, "Expr", [
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token operatorToken, Expr right",
                "Call     : Expr callee, Token paren, List<Expr> arguments",
                "Logical  : Expr left, Token operatorToken, Expr right",
                "Grouping : Expr expression",
                "Literal  : Object value",
                "Unary    : Token operatorToken, Expr right",
                "Variable : Token name",
            ]
        );

        DefineAst(outputDir, "Stmt", [
            "Block      : List<Stmt> statements",
            "Expression : Expr expr",
            "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
            "Function   : Token name, List<Token> funParams, List<Stmt> body",
            "While      : Expr condition, Stmt statement",
            "Print      : Expr expr",
            "Return     : Token keyword, Expr value",
            "Var        : Token name, Expr initializer"
        ]);
    }

    private static void DefineAst(string outputDir, string baseName, List<string> types)
    {
        var path = $"{outputDir}/{baseName}.cs";
        using var writer = new StreamWriter(File.Open(path, FileMode.OpenOrCreate | FileMode.Truncate));

        writer.WriteLine("namespace Interpreter;");
        writer.WriteLine();
        writer.WriteLine($"public abstract class {baseName}");
        writer.WriteLine("{");

        foreach (var type in types)
        {
            var className = type.Split(":")[0].Trim();
            var fields = type.Split(":")[1].Trim();
            DefineType(writer, baseName, className, fields);
        }
        writer.WriteLine();
        writer.WriteLine("\t\tpublic abstract T Accept<T>(IVisitor<T> visitor);");
        writer.WriteLine();
        DefineVisitor(writer, baseName, types);
        writer.WriteLine("}");
        writer.Flush();
        writer.Close();
    }

    private static void DefineType(StreamWriter writer, string baseName, string className, string fields)
    {
        writer.WriteLine($"\tpublic class {className}({fields}) : {baseName} {{");
        foreach (var field in fields.Split(", "))
        {
            var type = field.Split(" ")[0].Trim();
            var name = field.Split(" ")[1].Trim();
            writer.WriteLine($"\t\tpublic {type} {Char.ToUpper(name[0])}{name[1..]} => {name};");
        }
        writer.WriteLine();
        writer.WriteLine("\t\tpublic override T Accept<T>(IVisitor<T> visitor) {");
        writer.WriteLine($"\t\t\treturn visitor.Visit{className}{baseName}(this);");
        writer.WriteLine("\t\t}");
        writer.WriteLine("\t}");
    }

    private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
    {
        writer.WriteLine("\tpublic interface IVisitor<T> {");
        foreach (var type in types)
        {
            var typeName = type.Split(":")[0].Trim();
            writer.WriteLine($"\t\tT Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
        }
        writer.WriteLine("\t}");
    }
}