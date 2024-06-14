namespace Interpreter;

public abstract class Stmt
{
	public class Expression(Expr expr) : Stmt {
		public Expr Expr => expr;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitExpressionStmt(this);
		}
	}
	public class Print(Expr expr) : Stmt {
		public Expr Expr => expr;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitPrintStmt(this);
		}
	}
	public class Var(Token name, Expr initializer) : Stmt {
		public Token Name => name;
		public Expr Initializer => initializer;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitVarStmt(this);
		}
	}

		public abstract T Accept<T>(IVisitor<T> visitor);

	public interface IVisitor<T> {
		T VisitExpressionStmt(Expression stmt);
		T VisitPrintStmt(Print stmt);
		T VisitVarStmt(Var stmt);
	}
}
