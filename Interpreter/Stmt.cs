namespace Interpreter;

public abstract class Stmt
{
	public class Block(List<Stmt> statements) : Stmt {
		public List<Stmt> Statements => statements;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitBlockStmt(this);
		}
	}
	public class Class(Token name, Expr.Variable superclass, List<Stmt.Function> methods) : Stmt {
		public Token Name => name;
		public Expr.Variable Superclass => superclass;
		public List<Stmt.Function> Methods => methods;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitClassStmt(this);
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
	public class Function(Token name, List<Token> funParams, List<Stmt> body) : Stmt {
		public Token Name => name;
		public List<Token> FunParams => funParams;
		public List<Stmt> Body => body;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitFunctionStmt(this);
		}
	}
	public class While(Expr condition, Stmt statement) : Stmt {
		public Expr Condition => condition;
		public Stmt Statement => statement;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitWhileStmt(this);
		}
	}
	public class Print(Expr expr) : Stmt {
		public Expr Expr => expr;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitPrintStmt(this);
		}
	}
	public class Return(Token keyword, Expr value) : Stmt {
		public Token Keyword => keyword;
		public Expr Value => value;

		public override T Accept<T>(IVisitor<T> visitor) {
			return visitor.VisitReturnStmt(this);
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
		T VisitClassStmt(Class stmt);
		T VisitExpressionStmt(Expression stmt);
		T VisitIfStmt(If stmt);
		T VisitFunctionStmt(Function stmt);
		T VisitWhileStmt(While stmt);
		T VisitPrintStmt(Print stmt);
		T VisitReturnStmt(Return stmt);
		T VisitVarStmt(Var stmt);
	}
}
