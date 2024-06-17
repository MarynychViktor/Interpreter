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
	public class Call(Expr callee, Token paren, List<Expr> arguments) : Expr {
		public Expr Callee => callee;
		public Token Paren => paren;
		public List<Expr> Arguments => arguments;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitCallExpr(this);
		}
	}
	public class Get(Expr obj, Token name) : Expr {
		public Expr Obj => obj;
		public Token Name => name;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitGetExpr(this);
		}
	}
	public class Set(Expr obj, Token name, Expr value) : Expr {
		public Expr Obj => obj;
		public Token Name => name;
		public Expr Value => value;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitSetExpr(this);
		}
	}
	public class Logical(Expr left, Token operatorToken, Expr right) : Expr {
		public Expr Left => left;
		public Token OperatorToken => operatorToken;
		public Expr Right => right;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitLogicalExpr(this);
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
	public class This(Token keyword) : Expr {
		public Token Keyword => keyword;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitThisExpr(this);
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
		T VisitCallExpr(Call expr);
		T VisitGetExpr(Get expr);
		T VisitSetExpr(Set expr);
		T VisitLogicalExpr(Logical expr);
		T VisitGroupingExpr(Grouping expr);
		T VisitLiteralExpr(Literal expr);
		T VisitUnaryExpr(Unary expr);
		T VisitThisExpr(This expr);
		T VisitVariableExpr(Variable expr);
	}
}
