using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetEngine;
using System.Numerics;

namespace SpreadsheetEngine.Tests
{
    [TestClass]
    public class CalculatorTests
    {
        private BigInteger Calc(string expression)
        {
            Table table = new Table(10, 10);
            return Calculator.Evaluate(expression, table);
        }

        [TestMethod]
        public void AddAndSubtract()
        {
            Assert.AreEqual(new BigInteger(7), Calc("10 - 5 + 2"));
        }

        [TestMethod]
        public void MultiplyHasPriority()
        {
            Assert.AreEqual(new BigInteger(14), Calc("2 + 3 * 4"));
        }

        [TestMethod]
        public void Parentheses()
        {
            Assert.AreEqual(new BigInteger(20), Calc("(2 + 3) * 4"));
        }

        [TestMethod]
        public void IntegerDivision()
        {
            Assert.AreEqual(new BigInteger(3), Calc("7 / 2"));
        }

        [TestMethod]
        public void UnaryMinusAndPlus()
        {
            Assert.AreEqual(new BigInteger(-5), Calc("-5"));
            Assert.AreEqual(new BigInteger(5), Calc("+5"));
            Assert.AreEqual(new BigInteger(-9), Calc("(1 + 2) * -3"));
        }

        [TestMethod]
        public void PowerIsRightAssociative()
        {
            Assert.AreEqual(new BigInteger(512), Calc("2^3^2"));
        }

        [TestMethod]
        public void MaxAndMin()
        {
            Assert.AreEqual(new BigInteger(10), Calc("max(3, 10)"));
            Assert.AreEqual(new BigInteger(3), Calc("MIN(3, 10)"));
            Assert.AreEqual(new BigInteger(7), Calc("max(1, min(7, 20))"));
        }

        [TestMethod]
        public void BigNumbers()
        {
            BigInteger expected = BigInteger.Parse("9999999999999999999800000000000000000001");
            Assert.AreEqual(expected, Calc("99999999999999999999 * 99999999999999999999"));
        }

        [TestMethod]
        public void DivideByZeroThrows()
        {
            Assert.ThrowsException<DivideByZeroException>(() => Calc("5 / 0"));
        }

        [TestMethod]
        public void NegativePowerThrows()
        {
            Assert.ThrowsException<Exception>(() => Calc("2 ^ -1"));
        }

        [TestMethod]
        public void SyntaxErrorThrows()
        {
            Assert.ThrowsException<ArgumentException>(() => Calc("1 +"));
            Assert.ThrowsException<ArgumentException>(() => Calc("(1 + 2"));
            Assert.ThrowsException<ArgumentException>(() => Calc("2 $ 3"));
        }
    }
}
