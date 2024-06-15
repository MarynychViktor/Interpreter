namespace Interpreter;

public abstract class Stmt
{
	public class Block(List<Stmt> statements) : Stmt {
		public List<Stmt> Statements => statements;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitBlockStmt(this);
		}
	}
	public class Expression(Expr expr) : Stmt {
		public Expr Expr => expr;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitExpressionStmt(this);
		}
	}
	public class If(Expr condition, Stmt thenBranch, Stmt elseBranch) : Stmt {
		public Expr Condition => condition;
		public Stmt ThenBranch => thenBranch;
		public Stmt ElseBranch => elseBranch;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitIfStmt(this);
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
		T VisitBlockStmt(Block stmt);
		T VisitExpressionStmt(Expression stmt);
		T VisitIfStmt(If stmt);
		T VisitPrintStmt(Print stmt);
		T VisitVarStmt(Var stmt);
	}
}
