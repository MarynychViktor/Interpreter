using System.Text;

namespace Interpreter;

public class AstPrinter : Expr.IVisitor<string>
{
    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parentisize(expr.Operatorr.lexeme, expr.Left, expr.Right);
    }

    public string VisitGroupingExpr(Expr.Grouping expr) => Parentisize("group", expr.Expression);

    public string VisitLiteralExpr(Expr.Literal expr) => expr == null ? "nil" : expr.Value.ToString();

    public string VisitUnaryExpr(Expr.Unary expr) => Parentisize(expr.Operatorr.lexeme, expr.Right);

    private string Parentisize(String name, params Expr[] exprs) {
        StringBuilder builder = new StringBuilder();

        builder.Append("(").Append(name);
        foreach (var expr in exprs)
        {
            builder.Append(" ");
            builder.Append(expr.Accept(this));
        }
        builder.Append(")");

        return builder.ToString();
    }
}