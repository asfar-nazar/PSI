// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// PSIPrint.cs ~ Prints a PSI syntax tree in Pascal format
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;

public class PSIPrint : Visitor<StringBuilder> {
   public override StringBuilder Visit (NProgram p) {
      Write ($"program {p.Name}; ");
      Visit (p.Block);
      return Write (".");
   }

   public override StringBuilder Visit (NBlock b) 
      => Visit (b.Decls, b.Body);

   public override StringBuilder Visit (NDeclarations d) {
      if (d.Vars.Length > 0) {
         NWrite ("var"); N++;
         foreach (var g in d.Vars.GroupBy (a => a.Type))
            NWrite ($"{g.Select (a => a.Name).ToCSV ()} : {g.Key};");
         N--;
      }
      d.Fns.ForEach (a => Visit (a));
      return S;
   }

   public override StringBuilder Visit (NVarDecl d)
      => NWrite ($"{d.Name} : {d.Type}");

   internal override StringBuilder Visit (NFnDecl f) {
      var key = f.Type is NType.Void ? "procedure" : "function";
      NWrite ($"{key} {f.Name} (");
      foreach (var g in f.Pars.GroupBy (a => a.Type))
         Write ($"{g.Select (a => a.Name).ToCSV ()}: {g.Key})");
      if (f.Type != NType.Void) Write ($": {f.Type}");
      Write (";");
      Visit (f.Block);
      return S;
   }

   internal override StringBuilder Visit (NIfStmt i) {
      NWrite ($"if ("); Visit (i.Expr); Write (") then");
      N++;
      Visit (i.IfPart);
      N--;
      if (i.ElsePart != null) {
         NWrite ("else"); N++;
         Visit (i.ElsePart); N--;
      }
      return S;
   }

   internal override StringBuilder Visit (NWhileStmt nWhileStmt) {
      NWrite ("while ");  
      Visit (nWhileStmt.Expr); 
      Write (" do "); N++;
      Visit(nWhileStmt.Stmt); N--;
      return S;
   }

   internal override StringBuilder Visit (NRepeatStmt nRepeatStmt) {
      NWrite ("repeat"); N++;
      nRepeatStmt.Stmts.ForEach (a=> Visit (a)); N--;
      NWrite ("until ");
      Visit (nRepeatStmt.Expr); NWrite ("");
      return S;
   }

   internal override StringBuilder Visit (NForStmt nForStmt) {
      NWrite ("for ");
      Write ($"{nForStmt.Var} := "); nForStmt.Start.Accept (this); 
      string t = nForStmt.Increment ? " TO ": " DOWNTO " ;
      Write (t);
      Visit (nForStmt.End); 
      Write (" do"); N++;
      Visit (nForStmt.Body); N--;
      return S;
   }

   internal override StringBuilder Visit (NReadStmt nReadStmt) 
      => NWrite ($"read ({nReadStmt.Identifiers.Select (a => a.Name).ToCSV ()});");

   internal override StringBuilder Visit (NCallStmt nCallStmt) {
      NWrite (""); 
      Visit (nCallStmt.Name);
      Write (" (");
      for (int i = 0; i < nCallStmt.Args.Length; i++) {
         if (i > 0) Write (", "); nCallStmt.Args[i].Accept (this);
      }
      return Write (");");
   }

   public override StringBuilder Visit (NCompoundStmt b) {
      NWrite ("begin"); N++;  Visit (b.Stmts); N--; return NWrite ("end"); 
   }

   public override StringBuilder Visit (NAssignStmt a) {
      NWrite ($"{a.Name} := "); a.Expr.Accept (this); return Write (";");
   }

   public override StringBuilder Visit (NWriteStmt w) {
      NWrite (w.NewLine ? "WriteLn (" : "Write (");
      for (int i = 0; i < w.Exprs.Length; i++) {
         if (i > 0) Write (", ");
         w.Exprs[i].Accept (this);
      }
      return Write (");");
   }

   public override StringBuilder Visit (NLiteral t)
      => Write (t.Value.ToString ());

   public override StringBuilder Visit (NIdentifier d)
      => Write (d.Name.Text);

   public override StringBuilder Visit (NUnary u) {
      Write (u.Op.Text); return u.Expr.Accept (this);
   }

   public override StringBuilder Visit (NBinary b) {
      Write ("("); b.Left.Accept (this); Write ($" {b.Op.Text} ");
      b.Right.Accept (this); return Write (")");
   }

   public override StringBuilder Visit (NFnCall f) {
      Write ($"{f.Name} (");
      for (int i = 0; i < f.Params.Length; i++) {
         if (i > 0) Write (", "); f.Params[i].Accept (this);
      }
      return Write (")");
   }

   StringBuilder Visit (params Node[] nodes) {
      nodes.ForEach (a => a.Accept (this));
      return S;
   }

   // Writes in a new line
   StringBuilder NWrite (string txt) 
      => Write ($"\n{new string (' ', N * 3)}{txt}");
   int N;   // Indent level

   // Continue writing on the same line
   StringBuilder Write (string txt) {
      Console.Write (txt);
      S.Append (txt);
      return S;
   }

   readonly StringBuilder S = new ();
}