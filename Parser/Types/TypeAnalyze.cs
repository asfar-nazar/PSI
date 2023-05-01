// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// TypeAnalyze.cs ~ Type checking, type coercion
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;
using static NType;
using static Token.E;

public class TypeAnalyze : Visitor<NType> {
   public TypeAnalyze () {
      mSymbols = SymTable.Root;
   }
   SymTable mSymbols;

   #region Declarations ------------------------------------
   public override NType Visit (NProgram p) 
      => Visit (p.Block);
   
   public override NType Visit (NBlock b) {
      mSymbols = new SymTable { Parent = mSymbols };
      Visit (b.Declarations); Visit (b.Body);
      mSymbols = mSymbols.Parent;
      return Void;
   }

   public override NType Visit (NDeclarations d) {
      Visit (d.Consts); Visit (d.Vars); 
      return Visit (d.Funcs);
   }
   bool AlreadyExists (Node n) {
      var name = n switch { NConstDecl c => c.Name, NVarDecl v => v.Name, NFnDecl f => f.Name, _ => null };
      var node = mSymbols.Consts.FirstOrDefault (a => a.Name == name);
      if (node is null) mSymbols.Vars.FirstOrDefault (a => a.Name == name);
      if (node is null) mSymbols.Funcs.FirstOrDefault (a => a.Name == name);
      if (node is not null) return true;
      return false;
   }

   public override NType Visit (NVarDecl d) {
      if (AlreadyExists (d)) return Error;
      mSymbols.Vars.Add (d);
      return d.Type;
   }

   public override NType Visit (NConstDecl nConstDecl) {
      if (AlreadyExists (nConstDecl)) return Error;
      mSymbols.Consts.Add (nConstDecl);
      return nConstDecl.Value.Kind switch {
         L_INTEGER => Int,
         L_REAL => Real,
         L_BOOLEAN => Bool,
         L_STRING => String,
         L_CHAR => Char,
         _ => Error,
      };
   }
   
   public override NType Visit (NFnDecl f) {
      if (AlreadyExists (f)) return Error;
      mSymbols.Funcs.Add (f);
      mSymbols = new SymTable { Parent = mSymbols };
      f.Params.ForEach (a => a.Accept (this));
      mSymbols.Vars.Add (new NVarDecl (f.Name, f.Return, false));
      f.Body?.Accept (this);
      mSymbols = mSymbols.Parent;
      return f.Return;

   }
   #endregion

   #region Statements --------------------------------------
   public override NType Visit (NCompoundStmt b)
      => Visit (b.Stmts);

   public override NType Visit (NAssignStmt a) {
      if (mSymbols.Find (a.Name.Text) is not NVarDecl v)
         throw new ParseException (a.Name, "Unknown variable");
      a.Expr.Accept (this);
      a.Expr = AddTypeCast (a.Name, a.Expr, v.Type);
      v.Assigned = true;
      return v.Type;
   }
   
   NExpr AddTypeCast (Token token, NExpr source, NType target) {
      if (source.Type == target) return source;
      bool valid = (source.Type, target) switch {
         (Int, Real) or (Char, Int) or (Char, String) => true,
         _ => false
      };
      if (!valid) throw new ParseException (token, "Invalid type");
      return new NTypeCast (source) { Type = target };
   }

   public override NType Visit (NWriteStmt w)
      => Visit (w.Exprs);

   public override NType Visit (NIfStmt f) {
      f.Condition.Accept (this);
      f.IfPart.Accept (this); f.ElsePart?.Accept (this);
      return Void;
   }

   public override NType Visit (NForStmt f) {
      f.Start.Accept (this); f.End.Accept (this); f.Body.Accept (this);
      return Void;
   }

   public override NType Visit (NReadStmt r) {
      throw new NotImplementedException ();
   }

   public override NType Visit (NWhileStmt w) {
      w.Condition.Accept (this); w.Body.Accept (this);
      return Void; 
   }

   public override NType Visit (NRepeatStmt r) {
      Visit (r.Stmts); r.Condition.Accept (this);
      return Void;
   }

   public override NType Visit (NCallStmt c) {
      var f = mSymbols.Find (c.Name.Text);
      return f == null ? Error : (f as NFnDecl).Return;
   }
   #endregion

   #region Expression --------------------------------------
   public override NType Visit (NLiteral t) {
      t.Type = t.Value.Kind switch {
         L_INTEGER => Int, L_REAL => Real, L_BOOLEAN => Bool, L_STRING => String,
         L_CHAR => Char, _ => Error,
      };
      return t.Type;
   }

   public override NType Visit (NUnary u) 
      => u.Type = u.Expr.Accept (this);

   public override NType Visit (NBinary bin) {
      NType a = bin.Left.Accept (this), b = bin.Right.Accept (this);
      bin.Type = (bin.Op.Kind, a, b) switch {
         (ADD or SUB or MUL or DIV, Int or Real, Int or Real) when a == b => a,
         (ADD or SUB or MUL or DIV, Int or Real, Int or Real) => Real,
         (MOD, Int, Int) => Int,
         (ADD, String, _) => String, 
         (ADD, _, String) => String,
         (LT or LEQ or GT or GEQ, Int or Real, Int or Real) => Bool,
         (LT or LEQ or GT or GEQ, Int or Real or String or Char, Int or Real or String or Char) when a == b => Bool,
         (EQ or NEQ, _, _) when a == b => Bool,
         (EQ or NEQ, Int or Real, Int or Real) => Bool,
         (AND or OR, Int or Bool, Int or Bool) when a == b => a,
         _ => Error,
      };
      if (bin.Type == Error)
         throw new ParseException (bin.Op, "Invalid operands");
      var (acast, bcast) = (bin.Op.Kind, a, b) switch {
         (_, Int, Real) => (Real, Void),
         (_, Real, Int) => (Void, Real), 
         (_, String, not String) => (Void, String),
         (_, not String, String) => (String, Void),
         _ => (Void, Void)
      };
      if (acast != Void) bin.Left = new NTypeCast (bin.Left) { Type = acast };
      if (bcast != Void) bin.Right = new NTypeCast (bin.Right) { Type = bcast };
      return bin.Type;
   }

   public override NType Visit (NIdentifier d) {
      if (mSymbols.Find (d.Name.Text) is NVarDecl v)
         if (v.Assigned) return d.Type = v.Type;
         else return Error;
      throw new ParseException (d.Name, "Unknown variable");
   }

   public override NType Visit (NFnCall f) {
      var n = mSymbols.Find (f.Name.Text);
      if (n == null) return Error;
      var fn = (n as NFnDecl);
      if (fn.Params.Length != f.Params.Length) return Error;
      f.Params.ForEach (a => a.Accept (this));
      for (int i = 0; i < fn.Params.Length; i++) {
         Token name = f.Params[i] switch {
            NFnCall nF => f.Name,
            NLiteral nL => nL.Value,
            NIdentifier nI => nI.Name,
            NUnary nU => nU.Op,
            NBinary nB => nB.Op,
            _ => f.Name,
         };
         
         f.Params[i] = AddTypeCast (name, f.Params[i], fn.Params[i].Type);
      }
      
      return f.Type = fn.Return;
   }

   public override NType Visit (NTypeCast c) {
      c.Expr.Accept (this); return c.Type;
   }
   #endregion

   NType Visit (IEnumerable<Node> nodes) {
      foreach (var node in nodes) node.Accept (this);
      return NType.Void;
   }
}
