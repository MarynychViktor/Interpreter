namespace Interpreter;

public abstract class Expr
{
    public class Binary(Expr left, Token operatorr, Expr right) : Expr;
    public class Grouping(Expr expression) : Expr;
    public class Literal(Object value) : Expr;
    public class Unary(Token operatorr, Expr right) : Expr;
}
