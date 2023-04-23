// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// Visitor.cs : Implements Visitor to traverse the syntax tree
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;

// The Visitor interface (to visit all nodes)
public abstract class Visitor<T> {
   public abstract T Visit (NProgram p);
   public abstract T Visit (NBlock b);
   public abstract T Visit (NDeclarations d);
   public abstract T Visit (NVarDecl d);

   public abstract T Visit (NCompoundStmt b);
   public abstract T Visit (NAssignStmt a);
   public abstract T Visit (NWriteStmt w);

   public abstract T Visit (NLiteral t);
   public abstract T Visit (NIdentifier d);
   public abstract T Visit (NUnary u);
   public abstract T Visit (NBinary b);
   public abstract T Visit (NFnCall f);
   internal abstract T Visit (NFnDecl nFnDecl);
   internal abstract T Visit (NIfStmt nIfStmt);
   internal abstract T Visit (NElseStmt nElseStmt);
   internal abstract T Visit (NProcDecl nProcDecl);
   internal abstract T Visit (NWhileStmt nWhileStmt);
   internal abstract T Visit (NRepeatStmt nRepeatStmt);
   internal abstract T Visit (NForStmt nForStmt);
   internal abstract T Visit (NReadStmt nReadStmt);
   internal abstract T Visit (NCallStmt nCallStmt);
}
