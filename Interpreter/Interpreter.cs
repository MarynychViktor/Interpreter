namespace Interpreter;

public class Interpreter : Expr.IVisitor<Object>, Stmt.IVisitor<Object>
{    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError e)
        {
            Cslox.RuntimeError(e);
        }
    }

    private void Execute(Stmt statement)
    {
        statement.Accept(this);
    }

    public void Interpret(Expr expr)
    {
        try
        {
            var value = expr.Accept(this);
            Console.WriteLine(Stringify(value));
        }
        catch (RuntimeError e)
        {
            Cslox.RuntimeError(e);
        }
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);
        switch (expr.OperatorToken.type)
        {
            case TokenType.MINUS:
                return (double)left - (double)right;
            case TokenType.PLUS:
                if ((left is double) && (right is double))
                    return (double)left + (double)right;
                else if ((left is string) && (right is string))
                    return (string)left + (string)right;

                throw new RuntimeError(expr.OperatorToken, "Operands must be two numbers or two strings.");
            case TokenType.STAR:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left * (double)right;
            case TokenType.SLASH:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left / (double)right;
            case TokenType.GREATER:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr.OperatorToken, left, right);
                return (double)left <= (double)right;
            case TokenType.BANG_EQUAL:
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
            default:
                return null;
        }
    }

    public object VisitGroupingExpr(Expr.Grouping expr) => (Evaluate(expr));

    public object VisitLiteralExpr(Expr.Literal expr) => expr.Value;

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);
        switch (expr.OperatorToken.type)
        {
            case TokenType.MINUS:
                CheckNumberOperand(expr.OperatorToken, right);
                return (double)right;
            case TokenType.BANG:
                return IsTruthy(right);
        };

        return null;
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    private string Stringify(object value)
    {
        if (value == null) return "nil";

        if (value is double) {
            String text = value.ToString();
            if (text.EndsWith(".0")) {
                text = text.Substring(0, text.Length - 2);
            }
            return text;
        }

        return value.ToString();
    }

    private void CheckNumberOperands(Token @operator, object left, object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(@operator, "Operand must be a number");
    }

    private void CheckNumberOperand(Token @operator, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(@operator, "Operand must be a number");
    }

    private object Evaluate(Expr expr) => expr.Accept(this);

    private bool IsTruthy(object right)
    {
        if (right == null) return false;
        if (right is Boolean boolRight) return boolRight;
        return true;
    }

    private bool IsEqual(object left, object right)
    {
        if (left == null && right == null) return true;
        if (left == null) return false;

        return left.Equals(right);
    }
}