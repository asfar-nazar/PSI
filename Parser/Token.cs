namespace PSI;
using static Token.E;

// Represents a PSI language Token
public class Token {
   public Token (Tokenizer source, E kind, string text, int line, int column) 
      => (Source, Kind, Text, Line, Column) = (source, kind, text, line, column);
   public Tokenizer Source { get; }
   public E Kind { get; }
   public string Text { get; }
   public int Line { get; }
   public int Column { get; }

   // The various types of token
   public enum E {
      // Keywords
      PROGRAM, VAR, IF, THEN, WHILE, ELSE, FOR, TO, DOWNTO,
      DO, BEGIN, END, PRINT, TYPE, NOT, OR, AND, MOD, _ENDKEYWORDS,
      // Operators
      ADD, SUB, MUL, DIV, NEQ, LEQ, GEQ, EQ, LT, GT, ASSIGN, 
      _ENDOPERATORS,
      // Punctuation
      SEMI, PERIOD, COMMA, OPEN, CLOSE, COLON, 
      _ENDPUNCTUATION,
      // Others
      IDENT, INTEGER, REAL, BOOLEAN, STRING, CHAR, EOF, ERROR
   }

   // Print a Token
   public override string ToString () => Kind switch {
      EOF or ERROR => Kind.ToString (),
      < _ENDKEYWORDS => $"\u00ab{Kind.ToString ().ToLower ()}\u00bb",
      STRING => $"\"{Text}\"",
      CHAR => $"'{Text}'",
      _ => Text,
   };

   // Utility function used to echo an error to the console
   public void PrintError () {
      Console.OutputEncoding = Encoding.Unicode;
      StringBuilder mSB = new ();
      if (Kind != ERROR) throw new Exception ("PrintError called on a non-error token");
      string fileName = $"File: {Source.FileName}";
      mSB.AppendLine (fileName);
      int cPos = fileName.Length;
      mSB.AppendLine ($"\u2500\u2500\u2500\u252c{string.Join ("", Enumerable.Repeat ("\u2500", cPos - 3))}");
      string preceding = mSB.ToString () + PrecedingLines (3);
      preceding = preceding.TrimEnd ('\n');
      // No. of spaces after 'line no|'
      cPos = Column + preceding.Split ('\n').Last ()[4..].TakeWhile (a => a is ' ').Count () - 1;
      string eCarat = "\n" + string.Join ("", Enumerable.Repeat (' ', cPos)) + "^\n";
      preceding += eCarat;
      string eText = string.Join ("", Enumerable.Repeat (' ', 1 + cPos - Text.Length / 2)) + Text + "\n";
      string succeeding = SucceedingLines (2);
      Console.Write (preceding);
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Write (eText);
      Console.ResetColor ();     
      Console.Write (succeeding);

      // Returns 'nLines - 1' preceding lines and the error line
      string PrecedingLines (int nLines) {
         var lines = Source.Lines;
         string text = "";
         for (int i = nLines; i > 0; i--) {
            if (Line - i >= 0) text += $"{Line - i + 1,3}\u2502{lines?[Line - i]}\n";
         }
         return text;
      }
      // Returns 'nLines' no. of lines after the error line
      string SucceedingLines (int nLines) {
         var lines = Source.Lines;
         string text = "";
         for (int i = 1; i <= nLines; i++) {
            if (Line + i < lines?.Length) text += $"{Line + i,3}\u2502{lines?[Line + i]}\n";
         }
         return text;
      }
   }

   // Helper used by the parser (maps operator sequences to E values)
   public static List<(E Kind, string Text)> Match = new () {
      (NEQ, "<>"), (LEQ, "<="), (GEQ, ">="), (ASSIGN, ":="), (ADD, "+"),
      (SUB, "-"), (MUL, "*"), (DIV, "/"), (EQ, "="), (LT, "<"),
      (LEQ, "<="), (GT, ">"), (SEMI, ";"), (PERIOD, "."), (COMMA, ","),
      (OPEN, "("), (CLOSE, ")"), (COLON, ":")
   };
}
