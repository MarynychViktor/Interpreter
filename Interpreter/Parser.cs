
namespace Interpreter;

public class Parser(List<Token> tokens, int current = 0)
{
    class ParseError : Exception {}

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.CLASS))
            {
                return ClassDeclaration();
            }

            if (Match(TokenType.FUN))
            {
                return Function("function");
            }
            
            if (Match(TokenType.VAR))
            {
                return VarDeclaration();
            }

            return Statement();

        }
        catch (ParseError error)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt ClassDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expected class name.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

        var methods = new List<Stmt.Function>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(Function("method"));
        }
        
        Consume(TokenType.RIGHT_BRACE, "Expect '{' before class body.");
        return new Stmt.Class(name, methods);
    }

    private Stmt.Function Function(string kind)
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect " + kind + " name.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after " + kind + " name.");
        var parameters = new List<Token>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }
                parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after " + kind + " name.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before " + kind + " body.");
        var body = Block();
        return new Stmt.Function(name, parameters, body);
    }

    private Stmt VarDeclaration()
    {
        var identifier = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expr? initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

        return new Stmt.Var(identifier, initializer);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.FOR))
        {
            return ForStatement();
        }

        if (Match(TokenType.IF))
        {
            return IfStatement();
        }

        if (Match(TokenType.WHILE))
        {
            return WhileStatement();
        }
        
        if (Match(TokenType.PRINT))
        {
            return PrintStatement();
        }

        if (Match(TokenType.RETURN))
        {
            return ReturnStatement();
        }

        if (Match(TokenType.LEFT_BRACE))
        {
            return new Stmt.Block(Block());
        }

        return ExpressionStatement();
    }

    private Stmt ReturnStatement()
    {
        var keyword = Previous();
        Expr value = null;

        if (!Check(TokenType.SEMICOLON)) {
            value = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
        return new Stmt.Return(keyword, value);
    }

    private Stmt ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
        Stmt? initializer = null;
        if (Match(TokenType.VAR))
        {
            initializer = VarDeclaration();
        }
        else if (!Match(TokenType.SEMICOLON))
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(TokenType.SEMICOLON))
        {
            condition = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
        {
            increment = Expression();
        }
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");
        var body = Statement();
        
        if (increment != null)
        {
            if (body is Stmt.Block block)
            {
                block.Statements.Add(new Stmt.Expression(increment));
            }
            else
            {
                throw new Exception("Unexpected body format");
            }
            // body = new Stmt.Block(
            // [
            //     body,
            //     new Stmt.Expression(increment)
            // ]);
        }

        if (condition == null)
        {
            condition = new Expr.Literal(true);
        }

        body = new Stmt.While(condition, body);

        if (initializer != null) {
            body = new Stmt.Block([initializer, body]);
        }

        return body;
    }

    private Stmt WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect '(' after 'if'.");
        var statement = Statement();
        return new Stmt.While(condition, statement);
    }

    private Stmt IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect '(' after 'if'.");
        var thenBranch = Statement();
        Stmt elseBranch = null;
        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = new();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());   
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");

        return statements;
    }

    private Stmt ExpressionStatement()
    {
        var value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new Stmt.Expression(value);
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        var expr = Or();

        if (Match(TokenType.EQUAL))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable variable)
            {
                Token name = variable.Name;
                return new Expr.Assign(name, value);
            }
            else if (expr is Expr.Get getExpr)
            {
                return new Expr.Set(getExpr, getExpr.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        Expr expr = And();

        while (Match(TokenType.OR)) {
            Token @operator = Previous();
            Expr right = And();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(TokenType.AND))
        {
            Token @operator = Previous();
            Expr right = Equality();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        var expr = Comparision();
        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            var @operator = Previous();
            var right = Comparision();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Comparision()
    {
        var expr = Term();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            var @operator = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();

        while (Match(TokenType.PLUS, TokenType.MINUS))
        {            var @operator = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();

        while (Match(TokenType.STAR, TokenType.SLASH))
        {            
            var @operator = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            return new Expr.Unary(Previous(), Unary());
        }

        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else if(Match(TokenType.DOT))
            {
                var name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }

                arguments.Add(Expression());
            } while (Match(TokenType.COMMA));
        }

        var paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
        return new Expr.Call(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.FALSE)) return new Expr.Literal(false);
        if (Match(TokenType.TRUE)) return new Expr.Literal(true);
        if (Match(TokenType.NIL)) return new Expr.Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING)) {
            return new Expr.Literal(Previous().literal);
        }

        if (Match(TokenType.LEFT_PAREN)) {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        if (Match(TokenType.IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        throw Error(Peek(), "expected expression");
    }

    private Token Consume(TokenType tokenType, string message)
    {
        if (Check(tokenType)) return Advance();

        throw Error(Peek(), message);
    }

    private Exception Error(Token token, string message)
    {
        Cslox.Error(token, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd()) {
            if (Previous().type == TokenType.SEMICOLON) return;

            switch (Peek().type) {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }

            Advance();
        }
    }

    private bool Match(params TokenType[] tokenTypes)
    {
        foreach (var type in tokenTypes)
        {
            if (Check(type)) {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private Token Previous() => tokens[current - 1];

    private bool Check(TokenType type) => !IsAtEnd() && Peek().type == type;

    private Token Peek() => tokens[current];

    private bool IsAtEnd() => Peek().type == TokenType.EOF;
}