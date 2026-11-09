using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetEngine;

namespace SpreadsheetEngine.Tests
{
    [TestClass]
    public class SpreadsheetTests
    {
        [TestMethod]
        public void TestExpression()
        {
            Table table = new Table(10, 10);
            table.SetExpression("A1", "5");
            table.SetExpression("A2", "max(A1, 2) + 3 * 2^2");

            Assert.AreEqual(17, (int)table.GetCell("A2").Value);
        }

        [TestMethod]
        public void TestDivideByZero()
        {
            Table table = new Table(10, 10);
            table.SetExpression("A1", "10 / 0");

            Assert.AreEqual("Ділення на нуль", table.GetCell("A1").Error);
        }

        [TestMethod]
        public void TestCircularReference()
        {
            Table table = new Table(10, 10);
            table.SetExpression("A1", "B1 + 1");
            table.SetExpression("B1", "A1 + 1");

            Assert.AreNotEqual("", table.GetCell("A1").Error);
            Assert.AreNotEqual("", table.GetCell("B1").Error);
        }
    }
}
