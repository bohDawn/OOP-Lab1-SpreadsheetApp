using System.Numerics;

namespace SpreadsheetEngine
{
    class LabCalculatorVisitor : LabCalculatorBaseVisitor<BigInteger>
    {
        Table table;

        public LabCalculatorVisitor(Table table)
        {
            this.table = table;
        }

        public override BigInteger VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override BigInteger VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            return BigInteger.Parse(context.GetText());
        }

        public override BigInteger VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            string name = context.GetText().ToUpper();
            return table.GetCellValue(name);
        }

        public override BigInteger VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override BigInteger VisitUnaryExpr(LabCalculatorParser.UnaryExprContext context)
        {
            var value = Visit(context.expression());

            if (context.operatorToken.Type == LabCalculatorLexer.SUBTRACT)
            {
                return -value;
            }
            else
            {
                return value;
            }
        }

        public override BigInteger VisitExponentialExpr(LabCalculatorParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (right < 0)
            {
                throw new Exception("Від'ємний степінь не дає цілого числа");
            }
            if (right > 100000)
            {
                throw new Exception("Занадто великий степінь");
            }

            return BigInteger.Pow(left, (int)right);
        }

        public override BigInteger VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
            {
                return left + right;
            }
            else
            {
                return left - right;
            }
        }

        public override BigInteger VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
            {
                return left * right;
            }
            else
            {
                if (right == 0)
                {
                    throw new DivideByZeroException("Ділення на нуль");
                }
                return left / right;
            }
        }

        public override BigInteger VisitFunctionExpr(LabCalculatorParser.FunctionExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.functionToken.Type == LabCalculatorLexer.MAX)
            {
                return BigInteger.Max(left, right);
            }
            else
            {
                return BigInteger.Min(left, right);
            }
        }

        private BigInteger WalkLeft(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(0));
        }

        private BigInteger WalkRight(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(1));
        }
    }
}
