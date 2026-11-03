using Antlr4.Runtime;
using System.Numerics;

namespace SpreadsheetEngine
{
    public static class Calculator
    {
        public static BigInteger Evaluate(string expression, Table table)
        {
            var lexer = new LabCalculatorLexer(new AntlrInputStream(expression));
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new ThrowExceptionErrorListener());

            var tokens = new CommonTokenStream(lexer);
            var parser = new LabCalculatorParser(tokens);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ThrowExceptionErrorListener());

            var tree = parser.compileUnit();

            var visitor = new LabCalculatorVisitor(table);

            return visitor.Visit(tree);
        }
    }
}
