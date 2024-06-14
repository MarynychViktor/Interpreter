namespace Interpreter;

public abstract class Expr
{
	public class Assign(Token name, Expr value) : Expr {
		public Token Name => name;
		public Expr Value => value;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitAssignExpr(this);
		}
	}
	public class Binary(Expr left, Token operatorToken, Expr right) : Expr {
		public Expr Left => left;
		public Token OperatorToken => operatorToken;
		public Expr Right => right;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitBinaryExpr(this);
		}
	}
	public class Grouping(Expr expression) : Expr {
		public Expr Expression => expression;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitGroupingExpr(this);
		}
	}
	public class Literal(Object value) : Expr {
		public Object Value => value;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitLiteralExpr(this);
		}
	}
	public class Unary(Token operatorToken, Expr right) : Expr {
		public Token OperatorToken => operatorToken;
		public Expr Right => right;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitUnaryExpr(this);
		}
	}
	public class Variable(Token name) : Expr {
		public Token Name => name;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitVariableExpr(this);
		}
	}

		public abstract T Accept<T>(IVisitor<T> visitor);

	public interface IVisitor<T> {
		T VisitAssignExpr(Assign expr);
		T VisitBinaryExpr(Binary expr);
		T VisitGroupingExpr(Grouping expr);
		T VisitLiteralExpr(Literal expr);
		T VisitUnaryExpr(Unary expr);
		T VisitVariableExpr(Variable expr);
	}
}
