namespace Interpreter;

public abstract class Expr
{
	public class Binary(Expr left, Token operatorr, Expr right) : Expr {
		public Expr Left => left;
		public Token Operatorr => operatorr;
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
	public class Unary(Token operatorr, Expr right) : Expr {
		public Token Operatorr => operatorr;
		public Expr Right => right;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitUnaryExpr(this);
		}
	}

     public abstract T Accept<T>(IVisitor<T> visitor);

	public interface IVisitor<T> {
		T VisitBinaryExpr(Binary expr);
		T VisitGroupingExpr(Grouping expr);
		T VisitLiteralExpr(Literal expr);
		T VisitUnaryExpr(Unary expr);
	}
}
