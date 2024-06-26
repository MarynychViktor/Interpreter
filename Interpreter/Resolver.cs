namespace Interpreter;

public class Resolver(Interpreter interpreter) : Expr.IVisitor<object>, Stmt.IVisitor<object>
{
    private readonly Stack<Dictionary<string, bool>> _scopes = new();
    private FunctionType currentFunction = FunctionType.NONE;
    private ClassType currentClass = ClassType.NONE;

    public object VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (var exprArgument in expr.Arguments)
        {
            Resolve(exprArgument);
        }

        return null;
    }

    public object VisitGetExpr(Expr.Get expr)
    {
        Resolve(expr.Obj);
        return null;
    }

    public object VisitSetExpr(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Obj);
        return null;
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object VisitThisExpr(Expr.This expr)
    {
        if (currentClass == ClassType.NONE) {
            Cslox.Error(expr.Keyword, "Can't use 'this' outside of a class.");
            return null;
        }

        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object VisitSuperExpr(Expr.Super expr)
    {
        if (currentClass == ClassType.NONE) {
            Cslox.Error(expr.Keyword,
                "Can't use 'super' outside of a class.");
        } else if (currentClass != ClassType.SUBCLASS) {
            Cslox.Error(expr.Keyword,
                "Can't use 'super' in a class with no superclass.");
        }

        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        if (_scopes.Count > 0 &&
            _scopes.Peek().ContainsKey(expr.Name.lexeme) && _scopes.Peek()[expr.Name.lexeme] == false) {
            Cslox.Error(expr.Name,"Can't read local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        var scopeList = _scopes.ToList();
        scopeList.Reverse();

        for (int i = _scopes.Count - 1; i >= 0; i--) {
            if (scopeList[i].ContainsKey(name.lexeme)) {
                interpreter.Resolve(expr, _scopes.Count - 1 - i);
                return;
            }
        }
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public object VisitClassStmt(Stmt.Class stmt)
    {
        ClassType enclosingClass = currentClass;
        currentClass = ClassType.CLASS;
        Declare(stmt.Name);
        Define(stmt.Name);

        if (stmt.Superclass != null && stmt.Name.lexeme.Equals(stmt.Superclass.Name.lexeme)) {
            Cslox.Error(stmt.Superclass.Name, "A class can't inherit from itself.");
        }

        if (stmt.Superclass != null)
        {
            currentClass = ClassType.SUBCLASS;
            Resolve(stmt.Superclass);
        }

        if (stmt.Superclass != null) {
            BeginScope();
            _scopes.Peek().Add("super", true);
        }

        BeginScope();

        _scopes.Peek().Add("this", true);
        
        foreach (var method in stmt.Methods)
        {
            var declaration = FunctionType.METHOD;
            if (method.Name.lexeme.Equals("init")) {
                declaration = FunctionType.INITIALIZER;
            }

            ResolveFunction(method, declaration);
        }
        
        EndScope();
        if (stmt.Superclass != null) EndScope();

        currentClass = enclosingClass;
        return null;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Stmt statement)
    {
        statement.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void BeginScope()
    {
        _scopes.Push(new Dictionary<String, bool>());
    }

    private void EndScope()
    {
        _scopes.Pop();
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
        return null;
    }

    public object VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = type;

        BeginScope();
        foreach (var param in function.FunParams) {
            Declare(param);
            Define(param);
        }
        Resolve(function.Body);
        EndScope();

        currentFunction = enclosingFunction;
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Statement);
        return null;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object VisitReturnStmt(Stmt.Return stmt)
    {
        if (currentFunction == FunctionType.NONE) {
            Cslox.Error(stmt.Keyword, "Can't return from top-level code.");
        }

        if (stmt.Value != null) {
            if (currentFunction == FunctionType.INITIALIZER) {
                Cslox.Error(stmt.Keyword, "Can't return a value from an initializer.");
            }

            Resolve(stmt.Value);
        }

        return null;
    }

    public object VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null) {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);
        return null;
    }

    private void Define(Token name)
    {
        if (_scopes.Count== 0) return;
        _scopes.Peek()[name.lexeme] = true;
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;

        Dictionary<String, Boolean> scope = _scopes.Peek();
        if (scope.ContainsKey(name.lexeme)) {
            Cslox.Error(name, "Already a variable with this name in this scope.");
        }
        scope[name.lexeme] = false;
    }
}

enum FunctionType {
    NONE,
    FUNCTION,
    INITIALIZER,
    METHOD,
}

enum ClassType
{
    NONE,
    CLASS,
    SUBCLASS,
}