using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetEngine;
using System.Numerics;

namespace SpreadsheetEngine.Tests
{
    [TestClass]
    public class TableTests
    {
        [TestMethod]
        public void ColumnNames()
        {
            Assert.AreEqual("A", Table.ColumnName(0));
            Assert.AreEqual("Z", Table.ColumnName(25));
            Assert.AreEqual("AA", Table.ColumnName(26));
            Assert.AreEqual("B3", Table.CellName(2, 1));
        }

        [TestMethod]
        public void ParseCellName()
        {
            int row, col;
            Assert.IsTrue(Table.ParseName("AB12", out row, out col));
            Assert.AreEqual(11, row);
            Assert.AreEqual(27, col);
            Assert.IsFalse(Table.ParseName("A0", out row, out col));
            Assert.IsFalse(Table.ParseName("12", out row, out col));
        }

        [TestMethod]
        public void CellReferences()
        {
            Table table = new Table(10, 10);
            table.SetExpression("A1", "5");
            table.SetExpression("A2", "A1 * 2");
            table.SetExpression("A3", "a1 + a2");

            Assert.AreEqual(new BigInteger(15), table.GetCell("A3").Value);
        }

        [TestMethod]
        public void EmptyCellIsZero()
        {
            Table table = new Table(10, 10);
            table.SetExpression("A1", "B5 + 3");

            Assert.AreEqual(new BigInteger(3), table.GetCell("A1").Value);
            Assert.AreEqual("", table.GetCell("A1").Error);
        }

        [TestMethod]
        public void ChangingCellUpdatesDependents()
        {
            Table table = new Table(10, 10);
            table.SetExpression("A1", "1");
            table.SetExpression("B1", "A1 + 1");
            table.SetExpression("A1", "100");

            Assert.AreEqual(new BigInteger(101), table.GetCell("B1").Value);
        }

        [TestMethod]
        public void CircularReference()
        {
            Table table = new Table(10, 10);
            table.SetExpression("A1", "B1");
            table.SetExpression("B1", "A1");

            Assert.AreNotEqual("", table.GetCell("A1").Error);
            Assert.AreNotEqual("", table.GetCell("B1").Error);
        }

        [TestMethod]
        public void SelfReference()
        {
            Table table = new Table(10, 10);
            table.SetExpression("A1", "A1 + 1");

            Assert.AreNotEqual("", table.GetCell("A1").Error);
        }

        [TestMethod]
        public void ReferenceOutsideTable()
        {
            Table table = new Table(5, 5);
            table.SetExpression("A1", "Z99");

            Assert.AreNotEqual("", table.GetCell("A1").Error);
        }

        [TestMethod]
        public void ErrorGoesToDependentCell()
        {
            Table table = new Table(10, 10);
            table.SetExpression("A1", "1 / 0");
            table.SetExpression("A2", "A1 + 1");

            Assert.AreEqual("Ділення на нуль", table.GetCell("A1").Error);
            Assert.AreNotEqual("", table.GetCell("A2").Error);
        }

        [TestMethod]
        public void InsertRowShiftsCellsAndReferences()
        {
            Table table = new Table(5, 5);
            table.SetExpression("A1", "1");
            table.SetExpression("A2", "2");
            table.SetExpression("A3", "A1 + A2");

            table.InsertRow(1);

            Assert.AreEqual(6, table.RowCount);
            Assert.AreEqual("", table.GetCell("A2").Expression);
            Assert.AreEqual("A1+A3", table.GetCell("A4").Expression.Replace(" ", ""));
            Assert.AreEqual(new BigInteger(3), table.GetCell("A4").Value);
        }

        [TestMethod]
        public void DeleteRowGivesRefError()
        {
            Table table = new Table(5, 5);
            table.SetExpression("A1", "1");
            table.SetExpression("A2", "2");
            table.SetExpression("A3", "A2 * 10");

            table.DeleteRow(0);

            Assert.AreEqual(4, table.RowCount);
            Assert.AreEqual("A1*10", table.GetCell("A2").Expression.Replace(" ", ""));
            Assert.AreEqual(new BigInteger(20), table.GetCell("A2").Value);

            table.DeleteRow(0);

            Assert.IsTrue(table.GetCell("A1").Expression.Contains("#REF!"));
            Assert.AreNotEqual("", table.GetCell("A1").Error);
        }

        [TestMethod]
        public void InsertAndDeleteColumn()
        {
            Table table = new Table(5, 5);
            table.SetExpression("A1", "4");
            table.SetExpression("B1", "A1 + 1");

            table.InsertColumn(0);
            Assert.AreEqual(6, table.ColCount);
            Assert.AreEqual(new BigInteger(5), table.GetCell("C1").Value);

            table.DeleteColumn(0);
            Assert.AreEqual(5, table.ColCount);
            Assert.AreEqual(new BigInteger(5), table.GetCell("B1").Value);
        }

        [TestMethod]
        public void CannotDeleteLastRow()
        {
            Table table = new Table(1, 1);
            table.DeleteRow(0);
            table.DeleteColumn(0);

            Assert.AreEqual(1, table.RowCount);
            Assert.AreEqual(1, table.ColCount);
        }

        [TestMethod]
        public void SaveAndLoad()
        {
            Table table = new Table(7, 4);
            table.SetExpression("A1", "10");
            table.SetExpression("B2", "max(A1, 3) ^ 2");

            string path = Path.Combine(Path.GetTempPath(), "spreadsheet_test.xml");
            table.Save(path);

            Table loaded = new Table(1, 1);
            loaded.Load(path);
            File.Delete(path);

            Assert.AreEqual(7, loaded.RowCount);
            Assert.AreEqual(4, loaded.ColCount);
            Assert.AreEqual("max(A1, 3) ^ 2", loaded.GetCell("B2").Expression);
            Assert.AreEqual(new BigInteger(100), loaded.GetCell("B2").Value);
        }

        [TestMethod]
        public void LoadWrongFileThrows()
        {
            string path = Path.Combine(Path.GetTempPath(), "spreadsheet_wrong.xml");
            File.WriteAllText(path, "<book><title>test</title></book>");

            Table table = new Table(5, 5);
            Assert.ThrowsException<Exception>(() => table.Load(path));
            File.Delete(path);
        }
    }
}
