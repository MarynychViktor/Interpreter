namespace Interpreter;

public class Interpreter : Expr.IVisitor<Object>, Stmt.IVisitor<Object>
{
    public readonly LanguageEnvironment Globals = new();
    private LanguageEnvironment Environment;
    private Dictionary<Expr, int> _locals = new();

    public Interpreter()
    {
        Environment = Globals;
        Globals.Define("clock", new ClockCallable());
    }
    
    public void Interpret(List<Stmt> statements)
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

    public void Resolve(Expr expr, int depth)
    {
        _locals[expr] = depth;
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

    public object VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        int? distance = _locals.TryGetValue(expr, out var local) ? local : null;

        if (distance != null) {
            Environment.AssignAt(distance, expr.Name, value);
        } else {
            Globals.Assign(expr.Name, value);
        }

        return value;
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

    public object VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);

        var arguments = expr.Arguments.Select(Evaluate).ToList();
        if (callee is not ICsloxCallable function) {
            throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
        }
        if (arguments.Count() != function.Arity()) {
            throw new RuntimeError(expr.Paren, "Expected " +
                                               function.Arity() + " arguments but got " +
                                               arguments.Count() + ".");
        }

        return function.Call(this, arguments);
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);
        if (expr.OperatorToken.type == TokenType.OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return Evaluate(expr.Right);
        }

        return Evaluate(expr.Right);
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

    public object VisitVariableExpr(Expr.Variable expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    private object LookUpVariable(Token name, Expr.Variable expr)
    {
        int? distance = _locals.TryGetValue(expr, out var local) ? local : null;
        if (distance != null)
        {
            return Environment.GetAt(distance, name.lexeme);
        }
        else
        {
            return Globals.Get(name);
        }
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new LanguageEnvironment(Environment));
        return null;
    }

    public void ExecuteBlock(List<Stmt> statements, LanguageEnvironment environment)
    {
        var prevEnvironment = Environment;
        try
        {
            Environment = environment;
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            Environment = prevEnvironment;
        }
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return null;
    }

    public object VisitFunctionStmt(Stmt.Function stmt)
    {
        var function = new CsloxFunction(stmt, Environment);
        Environment.Define(stmt.Name.lexeme, function);
        return null;
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Statement);
        }

        return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object VisitReturnStmt(Stmt.Return stmt)
    {
        Object value = null;
        if (stmt.Value != null) value = Evaluate(stmt.Value);
        throw new Return(value);
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
        object value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }
        Environment.Define(stmt.Name.lexeme, value);
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